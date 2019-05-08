﻿using System;
using System.Collections.Generic;
using log4net;
using CopperBend.Contract;
using CopperBend.Model;
using GoRogue;
using SadConsole;
using SadConsole.Input;
using SadConsole.Components;
using Microsoft.Xna.Framework;
using SadConState = SadConsole.Global;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Size = System.Drawing.Size;
using System.Linq;

namespace CopperBend.Engine
{
    public class Engine : ContainerConsole
    {
        private ILog log;

        public Size GameSize;
        public Size MapSize;
        public Size MapWindowSize;

        private ScrollingConsole MapConsole { get; set; }
        public Window MapWindow;
        public MessageLogWindow MessageLog;
        
        private Keyboard Kbd;
        public Queue<AsciiKey> InputQueue;
        private SpaceMap SpaceMap;
        private Being Player;
        private GameState GameState;

        private Schedule Schedule;
        private CommandDispatcher Dispatcher;

        #region Init
        public Engine(int gameWidth, int gameHeight)
            : base()
        {
            log = LogManager.GetLogger("CB", "CB.Engine");

            IsVisible = true;
            IsFocused = true;

            MapSize = new Size(200, 130);
            GameSize = new Size(gameWidth, gameHeight);
            MapWindowSize = new Size(GameSize.Width * 2 / 3, GameSize.Height - 8);

            Parent = SadConState.CurrentScreen;
            Kbd = SadConState.KeyboardState;

            InputQueue = new Queue<AsciiKey>();
            ModeStack = new Stack<EngineMode>();
            CallbackStack = new Stack<Func<bool>>();

            Init();
        }

        public void Init()
        {
            PushEngineMode(EngineMode.StartUp, null);

            var gen = new MapGen();
            SpaceMap = gen.GenerateMap(MapSize.Width, MapSize.Height, 100, 5, 15);
            log.Debug("Generated map");

            Schedule = new Schedule();
            Describer describer = new Describer();

            GameState = new GameState();
            Dispatcher = new CommandDispatcher(Schedule, GameState, describer, null );

            GameState.Player = Player = CreatePlayer(SpaceMap.PlayerStartPoint);
            GameState.PushEngineMode = PushEngineMode;
            GameState.ClearPendingInput = InputQueue.Clear;

            CompoundMap FullMap = new CompoundMap
            {
                Width = MapSize.Width,
                Height = MapSize.Height,
                SpaceMap = SpaceMap,
                BeingMap = new MultiSpatialMap<IBeing>(),
                ItemMap = new MultiSpatialMap<IItem>(),
                LocatedTriggers = new List<LocatedTrigger>(),
                BlightMap = null
            };
            GameState.Map = FullMap;
            Schedule.AddAgent(Player, 12);

            var builder = new UIBuilder(GameSize);
            (MapConsole, MapWindow) = builder.CreateMapWindow(MapWindowSize, MapSize, "Game Map", FullMap);
            Children.Add(MapWindow);
            MapConsole.Children.Add(Player);
            MapWindow.Show();

            Player.CommandSource = new InputCommandSource(InputQueue, describer, MapWindow, GameState, Dispatcher);

            MapConsole.CenterViewPortOnPoint(Player.Position);

            MessageLog = builder.CreateMessageLog();
            Children.Add(MessageLog);
            MessageLog.Show();

            PushEngineMode(EngineMode.Schedule, null);
        }

        private Being CreatePlayer(Point playerLocation)
        {
            var player = new Player(Color.AntiqueWhite, Color.Transparent);
            player.Animation.CurrentFrame[0].Glyph = '@';
            player.Animation.CurrentFrame[0].Foreground = Color.AntiqueWhite;
            player.Position = playerLocation;
            player.Components.Add(new EntityViewSyncComponent());
            player.Name = "Suvail";

            log.Debug("Created player entity.");
            return player;
        }
        #endregion

        private bool firstTimeInUpdate = true;

        public override void Update(TimeSpan timeElapsed)
        {
            if (firstTimeInUpdate)
            {
                log.Debug("First time in update.");
                firstTimeInUpdate = false;
            }

            QueueInput();
            ActOnMode();
            SyncStateChanges();

            base.Update(timeElapsed);
        }

        public void QueueInput()
        {
            //  0.5, later other sources of input
            foreach (var key in Kbd.KeysPressed)
            {
                InputQueue.Enqueue(key);
            }
        }

        public void SyncStateChanges()
        {
            if (GameState.PlayerMoved)
            {
                //TODO:  Events at locations on map:  CheckActorAtCoordEvent(actor, tile);
                //Map.UpdatePlayerFieldOfView(actor);
                //Map.IsDisplayDirty = true;
                MapConsole.CenterViewPortOnPoint(Player.Position);
                GameState.PlayerMoved = false;
            }

        }


        private AsciiKey GetNextKeyPress()
        {
            if (InputQueue.Count == 0) return new AsciiKey { Key = Keys.None };
            return InputQueue.Dequeue();
        }

        #region Mode mechanics
        //  We can stack modes of the game to any level.
        //  Say the schedule reaches the player, who is entering a command,
        //  and inspecting their quest log or inventory, the stack is:
        //    Large, Input, Schedule, Start
        //  Each mode on the stack has its own callback, so if we then
        //  we go into details of a quest, the states could be:
        //    Large, Large, Input, Schedule, Start
        //  ...and we can later leave the quest details without confusion
        //  about what we're doing.
        //  0.2:  later, mode and callback go into a GameState object.

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

            log.Debug($"Pushed mode {oldMode} down, now in {CurrentMode}.");
        }
        internal void PopEngineMode()
        {
            var oldMode = ModeStack.Pop();
            CallbackStack.Pop();

            log.Debug($"Popped mode {oldMode} off, now in {CurrentMode}.");
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
            case EngineMode.MessagesPending:
                HandlePendingMessages();
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
                nextAction?.Invoke(Dispatcher);
            }
        }

        //0.0: Actual output currently turned off.
        #region Short Message log
        public Queue<string> MessageQueue;
        public void AddMessage(string newMessage)
        {
            MessageQueue.Enqueue(newMessage);
            ShowMessages();
        }

        private int ShownMessages = 0;
        public int ShownLineLimitBeforePause = 3;
        public void ShowMessages()
        {
            while (CurrentMode != EngineMode.MessagesPending && MessageQueue.Any())
            {
                if (ShownMessages < ShownLineLimitBeforePause)
                {
                    var nextMessage = MessageQueue.Dequeue();
                    //0.0 WriteLine(nextMessage);
                    ShownMessages++;
                }
                else
                {
                    //0.0 WriteLine("-- more --");
                    PushEngineMode(EngineMode.MessagesPending, null);
                }
            }
        }

        public void HandlePendingMessages()
        {
            for (AsciiKey k = GetNextKeyPress(); k.Key != Keys.None; k = GetNextKeyPress())
            {
                if (k.Key == Keys.Space)
                {
                    ShownMessages = 0;
                    PopEngineMode();
                    ShowMessages();  // ...which may pause us again.  If so, we'll be right back.
                    return;
                }
            }
        }
        #endregion

        private void HandleMenus()
        {
            //TODO:  All these:
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
