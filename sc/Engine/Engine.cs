using System;
using System.Collections.Generic;
using log4net;
using CopperBend.Contract;
using CopperBend.Model;
using SadConsole;
using SadConsole.Input;
using SadConsole.Components;
using Microsoft.Xna.Framework;
using GameState = SadConsole.Global;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Size = System.Drawing.Size;

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
        private Map Map;
        private Actor Player;

        private Schedule Schedule;
        private CommandDispatcher Dispatcher;

        private Stack<EngineMode> ModeStack;
        private Stack<Func<bool>> CallbackStack;

        internal EngineMode CurrentMode
        {
            get => (ModeStack?.Count == 0)
                ? EngineMode.Unknown
                : ModeStack.Peek();
        }
        internal Func<bool> CurrentCallback { get => CallbackStack.Peek(); }

        #region Init
        public Engine(int gameWidth, int gameHeight)
            : base()
        {
            log = LogManager.GetLogger("CB", "CB.NewEngine");

            IsVisible = true;
            IsFocused = true;

            MapSize = new Size(200, 130);
            GameSize = new Size(gameWidth, gameHeight);
            MapWindowSize = new Size(GameSize.Width * 2 / 3, GameSize.Height - 8);

            Parent = GameState.CurrentScreen;
            Kbd = GameState.KeyboardState;

            InputQueue = new Queue<AsciiKey>();
            ModeStack = new Stack<EngineMode>();
            CallbackStack = new Stack<Func<bool>>();

            Init();
        }

        public void Init()
        {
            PushMode(EngineMode.StartUp, null);

            var gen = new MapGen();
            Map = gen.GenerateMap(MapSize.Width, MapSize.Height, 100, 5, 15);
            log.Debug("Generated map");

            Schedule = new Schedule();
            Describer describer = new Describer();
            EventBus bus = new EventBus();
            Dispatcher = new CommandDispatcher(Schedule, null, describer, bus, null );
            
            Player = CreatePlayer(Map.PlayerStartPoint);
            Schedule.AddActor(Player, 12);


            var builder = new UIBuilder(GameSize);
            (MapConsole, MapWindow) = builder.CreateMapWindow(MapWindowSize, MapSize, "Game Map", Map.Tiles);
            Children.Add(MapWindow);
            MapConsole.Children.Add(Player);
            MapWindow.Show();

            Player.CommandSource = new InputCommandSource(InputQueue, describer, MapWindow, bus, Dispatcher);


            MapConsole.CenterViewPortOnPoint(Player.Position);

            MessageLog = builder.CreateMessageLog();
            Children.Add(MessageLog);
            MessageLog.Show();

            PushMode(EngineMode.Schedule, null);
        }



        private Actor CreatePlayer(Point playerLocation)
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

            base.Update(timeElapsed);
        }

        public void QueueInput()
        {
            //  0.5, later other sources of input
            foreach (var key in Kbd.KeysPressed)
            {
                InputQueue.Enqueue(key);
            }

            CheckKeyboard();
        }

        public void CheckKeyboard()  // 0.1, remove once ActOnMode in.
        {
            int xOff = 0;
            int yOff = 0;
            if (Kbd.IsKeyPressed(Keys.Left)) xOff = -1;
            if (Kbd.IsKeyPressed(Keys.Right)) xOff = 1;
            if (Kbd.IsKeyPressed(Keys.Up)) yOff = -1;
            if (Kbd.IsKeyPressed(Keys.Down)) yOff = 1;
            if (xOff == 0 && yOff == 0) return;

            Player.Position += new Point(xOff, yOff);
        }

        #region Mode mechanics
        //  We can nest states of the game to any level.
        //  Say the schedule has reached the player, who is entering a command,
        //  and inspecting their quest log:
        //    Schedule < Input Bound < Large Message
        //  Each state has a mode and a callback, so if, from the quest log,
        //  we go into details of a quest, the states could be:
        //    Schedule < Input Bound < Large Message < Large Message
        //  ...and we could leave the quest details without confusing the app
        //  about what we're doing.
        //  0.2:  later, mode and callback go into a GameState object, one stack.

        internal void PushMode(EngineMode newMode, Func<bool> callback)
        {
            if (newMode == EngineMode.Unknown)
                throw new Exception($"Should never EnterMode({newMode}).");

            var oldMode = CurrentMode;
            ModeStack.Push(newMode);
            CallbackStack.Push(callback);

            log.Debug($"PushMode, left {oldMode} and now in {CurrentMode}.");
        }
        internal void PopMode()
        {
            var oldMode = ModeStack.Pop();
            CallbackStack.Pop();

            log.Debug($"PopMode, left {oldMode} and back to {CurrentMode}.");
        }
        #endregion

        internal void ActOnMode()
        {
            switch (CurrentMode)
            {

            //  A menu will block even pending messages 
            case EngineMode.MenuOpen:
                HandleMenus();
                break;

            //  The large message pane overlays most of the game
            case EngineMode.LargeMessagePending:
                HandleLargeMessage();
                break;

            //  Messages waiting for the user will block input and scheduled events
            case EngineMode.MessagesPending:
                HandlePendingMessages();
                break;

            //  Pause and InputBound have developed the same API.
            //  Update does nothing when paused
            //  Waiting for player input blocks Schedule
            case EngineMode.Pause:
            case EngineMode.InputBound:
                bool exitMode = CurrentCallback();
                if (exitMode)
                {
                    PopMode();
                }
                //GameWindow.ClearMessagePause();
                //Dispatcher.HandlePlayerCommands();
                break;

            //  When the player has chosen an action that keeps them busy for a while,
            //  time passes and other actors act.
            case EngineMode.Schedule:
                DoNextScheduled();
                break;

            case EngineMode.Unknown:
                throw new Exception("Game mode unknown, perhaps Init() was missed.");

            default:
                throw new Exception($"Game mode [{CurrentMode}] not written yet.");
            }
        }


        public void HandlePendingMessages()
        {
            //if (!WaitingAtMorePrompt) return;

            //while (WaitingAtMorePrompt)
            //{
            //    //  Advance to next space keypress, if any
            //    RLKeyPress key = GetNextKeyPress();
            //    while (key != null && key.Key != RLKey.Space)
            //    {
            //        key = GetNextKeyPress();
            //    }

            //    //  If we run out of keypresses before we find a space,
            //    // the rest of the messages remain pending
            //    if (key?.Key != RLKey.Space) return;

            //    //  Otherwise, show more messages
            //    ClearMessagePause();
            //    ShowMessages();
            //}

            ////  If we reach this point, we sent all messages
            //EventBus.AllMessagesSent(this, new EventArgs());
        }

        public void DoNextScheduled()
        {
            while (CurrentMode == EngineMode.Schedule)
            {
                var nextAction = Schedule.GetNextAction();
                nextAction?.Invoke(Dispatcher);
            }
        }

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

            //EventBus.ClearLargeMessage(this, new EventArgs());
            HideLargeMessage();
        }

        private void HideLargeMessage()
        {
            throw new NotImplementedException();
        }

        // The entities in the given map will be the MapConsole's only entities
        private void SyncMapEntities(Map map)
        {
            // update the Map Console to hold only the Map's entities
            MapConsole.Children.Clear();
            foreach (var entity in map.Entities.Items)
            {
                MapConsole.Children.Add(entity);
            }

            // keep future changes to the map Entities up-to-date in the MapConsole
            map.Entities.ItemAdded += (s, a) => MapConsole.Children.Add(a.Item);
            map.Entities.ItemRemoved += (s, a) => MapConsole.Children.Remove(a.Item);
        }
    }
}
