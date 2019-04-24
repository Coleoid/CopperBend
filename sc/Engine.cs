using System;
using System.Collections.Generic;
using SadConsole;
using SadConsole.Input;
using SadConsole.Components;
using SadConsole.Controls;
using SadConsole.Entities;
using GameState = SadConsole.Global;
using Microsoft.Xna.Framework;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using log4net;
using CopperBend.App;

namespace CbRework
{

    public class Engine : ContainerConsole
    {
        private ILog log;

        public int MapWidth = 200;
        public int MapHeight = 130;
        public int WindowWidth;
        public int WindowHeight;
        private ScrollingConsole MapConsole { get; set; }
        public Window MapWindow;
        public MessageLogWindow MessageLog;
        
        private Keyboard Kbd;
        public Queue<AsciiKey> InputQueue;
        private Map Map;
        private Entity Player;


        public Engine(int windowWidth, int windowHeight)
            : base()
        {
            log = LogManager.GetLogger("CB", "CB.NewEngine");

            IsVisible = true;
            IsFocused = true;
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;

            Parent = GameState.CurrentScreen;
            Kbd = GameState.KeyboardState;
            InputQueue = new Queue<AsciiKey>();

            Init();
        }

        public void Init()
        {
            var gen = new MapGen();
            Map = gen.GenerateMap(MapWidth, MapHeight, 100, 5, 15);
            log.Debug("Generated map");

            Player = CreatePlayer(Map.PlayerStartPoint);

            MapWindow = CreateMapWindow(WindowWidth * 2 / 3, WindowHeight - 8, "Game Map");
            MapConsole.Children.Add(Player);
            MapWindow.Children.Add(MapConsole);
            Children.Add(MapWindow);
            MapWindow.Show();

            MapConsole.CenterViewPortOnPoint(Player.Position);

            MessageLog = CreateMessageLog();
            Children.Add(MessageLog);
            MessageLog.Show();
        }

        public MessageLogWindow CreateMessageLog()
        {
            var messageLog = new MessageLogWindow(WindowWidth, 8, "Message Log");
            MessageLog.Position = new Point(0, WindowHeight - 8);

            ////  Rudimentary fill the window
            //MessageLog.Add("Testing");
            //MessageLog.Add("Testing B");
            //MessageLog.Add("Testing three");
            //MessageLog.Add("Testing 4");
            //MessageLog.Add("Testing V");
            //MessageLog.Add("Testing x6");
            //MessageLog.Add("Testing Seventh");

            return messageLog;
        }

        public Window CreateMapWindow(int width, int height, string title)
        {
            int consoleWidth = width - 2;
            int consoleHeight = height - 2;

            Window mapWindow = new Window(width, height)
            {
                CanDrag = true,
                Title = title.Align(HorizontalAlignment.Center, consoleWidth)
            };

            //TODO: make click do something
            Button closeButton = new Button(3, 1)
            {
                Position = new Point(width - 3, 0),
                Text = "X"
            };
            mapWindow.Add(closeButton);

            MapConsole = new ScrollingConsole(
                MapWidth, MapHeight,
                Global.FontDefault, new Rectangle(0, 0, WindowWidth, WindowHeight),
                Map.Tiles);
            log.Debug("Created map console.");

            // Fit the MapConsole inside the border
            MapConsole.ViewPort = new Rectangle(0, 0, consoleWidth, consoleHeight);
            MapConsole.Position = new Point(1, 1);

            log.Debug("Created map window.");
            return mapWindow;
        }

        private CbEntity CreatePlayer(Point playerLocation)
        {
            var player = new Player(Color.AntiqueWhite, Color.Transparent);
            player.Animation.CurrentFrame[0].Glyph = '@';
            player.Animation.CurrentFrame[0].Foreground = Color.AntiqueWhite;
            player.Position = playerLocation;
            player.Components.Add(new EntityViewSyncComponent());

            log.Debug("Created player entity.");
            return player;
        }

        //  End Init



        public override void Update(TimeSpan timeElapsed)
        {
            CheckKeyboard();

            QueueInput();
            ActOnMode();


            base.Update(timeElapsed);
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

        public void QueueInput()
        {
            //  For now, only checking the keyboard for input

            foreach (var key in Kbd.KeysPressed)
            {
                InputQueue.Enqueue(key);
            }
        }

        #region Mode mechanics
        private Stack<EngineMode> ModeStack = new Stack<EngineMode>();
        private Stack<Func<bool>> CallbackStack = new Stack<Func<bool>>();
        internal EngineMode CurrentMode { get => ModeStack.Peek(); }
        internal Func<bool> CurrentCallback { get => CallbackStack.Peek(); }

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

            //  A game menu will block even pending messages 
            case EngineMode.MenuOpen:
                HandleMenus();
                break;

            //  The large message pane overlays most of the game
            case EngineMode.LargeMessagePending:
                HandleLargeMessage();
                break;

            //  Messages waiting for the player block player input and scheduled events
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

            //  When the player has committed to a slow action, time passes
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
                //var nextAction = Schedule.GetNextAction();
                //nextAction?.Invoke(Dispatcher);
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
