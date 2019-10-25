﻿using System;
using System.Collections.Generic;
using System.Linq;
using Size = System.Drawing.Size;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using log4net;
using SadConsole;
using SadConsole.Input;
using SadConsole.Components;
using SadConState = SadConsole.Global;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using System.Text;

namespace CopperBend.Engine
{
    public partial class Engine : ContainerConsole
    {
        private readonly ILog log;

        public Size GameSize;
        public Size MapSize;
        public Size MapWindowSize;
        public Size MenuWindowSize;

        private ScrollingConsole MapConsole { get; set; }
        public Window MapWindow;
        public MessageLogWindow MessageLog;
        
        private readonly Keyboard Kbd;
        public Queue<AsciiKey> InputQueue;
        private Being Player;

        private GameState GameState;
        private Schedule Schedule;
        private CommandDispatcher Dispatcher;

        #region Init
        public Engine(int gameWidth, int gameHeight, ILog logger, string topSeed = null)
            : base()
        {
            log = logger;

            IsVisible = true;
            IsFocused = true;

            GameSize = new Size(gameWidth, gameHeight);
            MapWindowSize = new Size(GameSize.Width * 2 / 3, GameSize.Height - 8);
            MenuWindowSize = new Size(GameSize.Width / 2, GameSize.Height - 12);

            Parent = SadConState.CurrentScreen;
            Kbd = SadConState.KeyboardState;

            InputQueue = new Queue<AsciiKey>();
            ModeStack = new Stack<EngineMode>();
            CallbackStack = new Stack<Func<bool>>();

            Init(topSeed);
        }

        private ICompoundMap FullMap;
        public void Init(string topSeed)
        {
            PushEngineMode(EngineMode.StartUp, null);

            if (topSeed == null)
            {
                string clearLetters = "bcdefghjkmnpqrstvwxyz";
                var r = new Random();
                var b = new StringBuilder();
                b.Append(clearLetters[r.Next(0, 20)]);
                b.Append(clearLetters[r.Next(0, 20)]);
                b.Append('-');
                b.Append(clearLetters[r.Next(0, 20)]);
                b.Append(clearLetters[r.Next(0, 20)]);
                b.Append('-');
                b.Append(clearLetters[r.Next(0, 20)]);
                b.Append(clearLetters[r.Next(0, 20)]);

                topSeed = b.ToString();
            }

            log.Info($"Top seed:  {topSeed}");
            Cosmogenesis(topSeed);

            var loader = new Persist.MapLoader();  //TODO: IoC
            FullMap = loader.FarmMap();
            log.Debug("Loaded the map");

            //0.2.GFX: Set a non-square font in message areas
            //var fontMaster = SadConsole.Global.LoadFont("terminal16x16_gs_ro.font");
            //var font = fontMaster.GetFont(SadConsole.Font.FontSizes.One);
            //SadConsole.Global.FontDefault = font;
            Describer describer = new Describer();

            Being.EntityFactory = new EntityFactory();

            Schedule = new Schedule(log);
            Player = CreatePlayer(FullMap.SpaceMap.PlayerStartPoint);
            Schedule.AddAgent(Player, 12);

            var builder = new UIBuilder(GameSize, null, log); //font
            //0.2.MAP: Put map name in YAML -> CompoundMap -> CreateMapWindow
            (MapConsole, MapWindow) = builder.CreateMapWindow(MapWindowSize, "A Farmyard", FullMap);
            Children.Add(MapWindow);
            MapConsole.Children.Add(Player.Console);
            FullMap.SetInitialConsoleCells(MapConsole, FullMap.SpaceMap);
            FullMap.FOV = new FOV(FullMap.GetView_CanSeeThrough());
            FullMap.UpdateFOV(MapConsole, Player.Position);
            MapWindow.Show();

            var (menuConsole, menuWindow) = builder.CreateMenuWindow(MenuWindowSize, "Menu", FullMap);
            Children.Add(menuWindow);
            menuWindow.Show();
            menuWindow.Add(new ControlsConsole(23, 23, null));
            menuWindow.Add(new ControlsConsole(23, 23, "FONNNT!"));

            MessageLog = builder.CreateMessageLog();
            Children.Add(MessageLog);
            MessageLog.Show();

            GameState = new GameState
            {
                Player = Player,
                Map = FullMap,
                Story = Engine.Compendium.Dramaticon,
            };

            Dispatcher = new CommandDispatcher(Schedule, GameState, describer, MessageLog, log)
            {
                PushEngineMode = PushEngineMode,
                PopEngineMode = PopEngineMode,
                IsInputReady = () => InputQueue.Count > 0,
                GetNextInput = InputQueue.Dequeue,
                ClearPendingInput = InputQueue.Clear,
                WriteLine = AddMessage,
                Prompt = MessageLog.Prompt,
                More = this.PromptUserForMoreAndPend,
            };

            Player.CommandSource = new InputCommandSource(describer, GameState, Dispatcher, log);

            MapConsole.CenterViewPortOnPoint(Player.Position);

            PushEngineMode(EngineMode.Schedule, null);
        }

        private Being CreatePlayer(Coord playerLocation)
        {
            var player = new Player(Color.AntiqueWhite, Color.Transparent, '@')
            {
                Name = "Suvail",
                Position = playerLocation
            };
            player.AddComponent(new EntityViewSyncComponent());
            player.AddToInventory(Equipper.BuildItem("hoe"));
            player.AddToInventory(Equipper.BuildItem("seed:Healer", 2));

            log.Debug("Created player.");
            player.Console.Position = playerLocation;
            return player;
        }
        #endregion

        public override void Update(TimeSpan timeElapsed)
        {
            QueueInput();
            ActOnMode();
            SyncMapChanges();

            AnimateBackground(timeElapsed);

            base.Update(timeElapsed);
        }

        private void AnimateBackground(TimeSpan timeElapsed)
        {
            FullMap.EffectsManager.UpdateEffects(timeElapsed.TotalSeconds);
        }

        public void QueueInput()
        {
            //  0.5, later other sources of input
            foreach (var key in Kbd.KeysPressed)
            {
                InputQueue.Enqueue(key);
            }
        }

        private AsciiKey GetNextKeyPress()
        {
            if (InputQueue.Count == 0) return new AsciiKey { Key = Keys.None };
            return InputQueue.Dequeue();
        }

        #region Mode mechanics
        //  We can stack modes of the game to any level.
        //  Say the schedule reaches the player, who enters a command
        //  to inspect their quest/job list, the stack is:
        //    Large, Input, Schedule, Start
        //  Each mode on the stack has its own callback, so if we then
        //  look into details of a quest, the states could be:
        //    Large, Large, Input, Schedule, Start
        //  ...and we can later leave the quest details without confusion
        //  about what we're doing.

        private Stack<EngineMode> ModeStack;
        private Stack<Func<bool>> CallbackStack;

        internal EngineMode CurrentMode
        {
            get => (ModeStack?.Count == 0)
                ? EngineMode.Unknown
                : ModeStack.Peek();
        }
        internal Func<bool> CurrentCallback { get => CallbackStack.Peek(); }

        internal void PushEngineMode(EngineMode newMode, Func<bool> callback)
        {
            if (newMode == EngineMode.Unknown)
                throw new Exception($"Should never EnterMode({newMode}).");

            var oldMode = CurrentMode;
            ModeStack.Push(newMode);
            CallbackStack.Push(callback);

            if (oldMode != EngineMode.Schedule || CurrentMode != EngineMode.InputBound)
                log.Debug($"Enter mode {CurrentMode}, push down mode {oldMode}.");
        }
        internal void PopEngineMode()
        {
            var oldMode = ModeStack.Pop();
            CallbackStack.Pop();

            // We're always shifting between input and schedule during play,
            // so let's only log more interesting mode changes.
            if (oldMode != EngineMode.InputBound || CurrentMode != EngineMode.Schedule)
                log.Debug($"Pop mode {oldMode} off, enter mode {CurrentMode}.");
        }
        #endregion

        internal void ActOnMode()
        {
            bool leaveMode = false;
            switch (CurrentMode)
            {
            //  A menu will block even pending messages 
            case EngineMode.MenuOpen:
                HandleMenus();
                break;

            //  The large message pane (inventory, log, ...) overlays most of the game
            case EngineMode.LargeMessagePending:
                HandleLargeMessage();
                break;

            //  Messages waiting at "- more -" for the user will block input and scheduled events
            case EngineMode.MessagesPendingUserInput:
                HandlePendingState();
                break;

            //  Pause and InputBound have developed the same API.
            //  Update does nothing when paused
            //  Waiting for player input blocks Schedule
            case EngineMode.Pause:
            case EngineMode.InputBound:
                leaveMode = CurrentCallback();
                if (leaveMode) PopEngineMode();
                break;

            //  When the player has chosen an action that keeps them busy for a while,
            //  time passes and other actors act.
            case EngineMode.Schedule:
                DoNextScheduled();
                break;

            case EngineMode.Unknown:
                throw new Exception("Engine mode unknown, probably popped one too many.");

            default:
                throw new Exception($"Engine mode [{CurrentMode}] not written yet.");
            }
        }

        public void DoNextScheduled()
        {
            while (CurrentMode == EngineMode.Schedule)
            {
                var nextAction = Schedule.GetNextAction();
                Dispatcher.Dispatch(nextAction);
            }
        }

        #region Short Message log
        private Queue<string> MessageQueue = new Queue<string>();
        private int MessagesSentSincePause = 0;
        private int MessageLimitBeforePause = 3;  //0.1: artificially low for short-term testing

        public void AddMessage(string newMessage)
        {
            MessageQueue.Enqueue(newMessage);
            ShowMessages();
        }

        //0.1: ResetMessagesSentSincePause() needs to be called all through the ICS.
        public void ResetMessagesSentSincePause() => MessagesSentSincePause = 0;

        private void ShowMessages()
        {
            //0.2: There are probably other modes where we want to suspend messages.
            while (CurrentMode != EngineMode.MessagesPendingUserInput && MessageQueue.Any())
            {
                if (MessagesSentSincePause >= MessageLimitBeforePause)
                {
                    PromptUserForMoreAndPend();
                    return;
                }

                var nextMessage = MessageQueue.Dequeue();
                MessageLog.WriteLine(nextMessage);
                MessagesSentSincePause++;
            }
        }

        private void PromptUserForMoreAndPend()
        {
            AddMessage("-- more --");
            PushEngineMode(EngineMode.MessagesPendingUserInput, null);
        }

        private void HandlePendingState()
        {
            for (AsciiKey k = GetNextKeyPress(); k.Key != Keys.None; k = GetNextKeyPress())
            {
                if (k.Key == Keys.Space)
                {
                    ResetMessagesSentSincePause();
                    PopEngineMode();
                    ShowMessages();  // ...which may have enough messages to pend us again.
                    return;
                }
            }
        }
        #endregion


        public void SyncMapChanges()
        {
            //  If any spaces changed whether they can be seen through, rebuild FOV
            if (FullMap.VisibilityChanged)
            {
                FullMap.FOV = new FOV(FullMap.GetView_CanSeeThrough());
                FullMap.VisibilityChanged = false;
                Dispatcher.PlayerMoved = true;
            }

            if (Dispatcher.PlayerMoved)
            {
                //TODO:  Events at locations on map:  CheckActorAtCoordEvent(actor, tile);

                FullMap.UpdateFOV(MapConsole, Player.Position);
                MapConsole.CenterViewPortOnPoint(Player.Position);
                Dispatcher.PlayerMoved = false;
            }

            if (!FullMap.CoordsWithChanges.Any()) return;
            var changedAndInFOV = FullMap.CoordsWithChanges
                .Intersect(FullMap.FOV.CurrentFOV);
            FullMap.UpdateViewOfCoords(MapConsole, changedAndInFOV);
            FullMap.CoordsWithChanges.Clear();
        }

        #region LargeMessages and Menus, currently empty
        private void HandleMenus()
        {
            //0.1.SAVE  create menu actions related to save/load
            //  Start new game
            //  Load game
            //  Save and Quit
        }

        //  The engine calls here when we're in EngineMode.LargeMessagePending
        public void HandleLargeMessage()
        {
            //RLKeyPress press = GameWindow.GetNextKeyPress();
            //while (press != null
            //       && press.Key != RLKey.Escape
            //       && press.Key != RLKey.Enter
            //       && press.Key != RLKey.KeypadEnter
            //       && press.Key != RLKey.Space
            //)
            //{
            //    press = GameWindow.GetNextKeyPress();
            //}

            //if (press == null) return;

            HideLargeMessage();
        }

        private void HideLargeMessage()
        {
            throw new NotImplementedException();
        }
        #endregion

        //// The entities in the given map will be the MapConsole's only entities
        //private void SyncMapEntities(MultiSpatialMap<Being> map)
        //{
        //    // update the Map Console to hold only the Map's entities
        //    MapConsole.Children.Clear();
        //    foreach (var being in map.Items)
        //    {
        //        MapConsole.Children.Add(being);
        //    }

        //    // keep future changes to the map Entities up-to-date in the MapConsole
        //    map.ItemAdded += (s, a) => MapConsole.Children.Add(a.Item);
        //    map.ItemRemoved += (s, a) => MapConsole.Children.Remove(a.Item);
        //}
    }
}
