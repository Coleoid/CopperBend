using System;
using System.Collections.Generic;
using CopperBend.App.Model;
using CopperBend.MapUtil;
using log4net;
using RLNET;

namespace CopperBend.App
{
    public class GameEngine
    {
        private readonly ILog log;
        public bool Config_AutoLoad_new_game { get; set; } = true;

        public IGameWindow GameWindow;
        private Queue<RLKeyPress> InputQueue;
        private Schedule Schedule;
        private CommandDispatcher Dispatcher;
        private MapLoader MapLoader;
        private GameState GameState;
        private readonly EventBus Bus;

        #region Initialization
        public GameEngine(
            EventBus bus, 
            Schedule schedule, 
            IGameWindow gameWindow, 
            Queue<RLKeyPress> inputQueue, 
            MapLoader mapLoader, 
            IGameState gameState, 
            CommandDispatcher commandDispatcher)
        {
            log = LogManager.GetLogger("CB.Engine");

            Bus = bus;
            Bus.AllMessagesSentSubscribers += AllMessagesSent;
            Bus.MessagePanelFullSubscribers += MessagePanelFull;
            Bus.EnterEngineModeSubscribers += (s, a) => PushMode(a.Mode, a.Callback);  //0.1
            Bus.ClearPendingInputSubscribers += (s, a) => InputQueue.Clear();//0.1

            Schedule = schedule;
            GameWindow = gameWindow;
            Dispatcher = commandDispatcher;

            InputQueue = inputQueue;
            MapLoader = mapLoader;
            GameState = (GameState)gameState;  // not great, but working...

            ModeStack.Push(EngineMode.Unknown);
            CallbackStack.Push(null);
            PushMode(EngineMode.Schedule, null);
        }

        public void Run()
        {
            if (Config_AutoLoad_new_game) LoadNewGame();
            //else throw new Exception("Time to develop some menu flow");

            GameWindow.Run(onUpdate, onRender);
        }

        public void LoadNewGame()
        {
            var player = InitPlayer();
            player.IsPlayer = true;
            GameState.Player = player; // leaving?

            LoadDevMap("Farm");  //0.1

            var ics = new InputCommandSource(InputQueue, new Describer(), GameWindow, Bus, Dispatcher);
            player.CommandSource = ics;
            player.Controls = Dispatcher;
        }

        public void LoadDevMap(string mapName)
        {
            UnloadCurrentMap();

            GameState.Map = MapLoader.LoadDevMap(mapName, GameState);

            foreach (var actor in GameState.Map.Actors)
            {
                Schedule.AddActor(actor, 12);
            }

            Bus.SendLargeMessage(this, GameState.Map.FirstSightMessage);
        }

        #endregion

        private void onRender(object sender, UpdateEventArgs e)
        {
            //0.1
            if (!MapLoaded)
            {
                MapLoaded = true;
                foreach (var text in GameState.Map.FirstSightMessage)
                {
                    GameWindow.AddMessage(text);
                }
            }

            GameWindow.Render(GameState.Map);
        }

        private void onUpdate(object sender, UpdateEventArgs e)
        {
            QueueInput();
            ActOnMode();
        }

        private void QueueInput()
        {
            //  For now, only checking the keyboard for input
            RLKeyPress press = GameWindow.GetKeyPress();
            while (press != null)
            {
                if (press.Alt && press.Key == RLKey.F4)
                {
                    QuitGame();
                    return;
                }

                InputQueue.Enqueue(press);
                press = GameWindow.GetKeyPress();
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
                GameWindow.HandlePendingMessages();
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

        //  The engine calls here when we're in EngineMode.LargeMessagePending
        public void HandleLargeMessage()
        {
            RLKeyPress press =  GameWindow.GetNextKeyPress();
            while (press != null 
                && press.Key != RLKey.Escape
                && press.Key != RLKey.Enter
                && press.Key != RLKey.KeypadEnter
                && press.Key != RLKey.Space
            )
            {
                press = GameWindow.GetNextKeyPress();
            }

            if (press == null) return;

            //EventBus.ClearLargeMessage(this, new EventArgs());
            HideLargeMessage();
        }

        private void HideLargeMessage()
        {
            throw new NotImplementedException();
        }

        public void DoNextScheduled()
        {
            while (CurrentMode == EngineMode.Schedule)
            {
                var nextAction = Schedule.GetNextAction();
                nextAction?.Invoke(Dispatcher);
            }
        }


        public void UnloadCurrentMap()
        {
            //TODO: persist map content/changes

            //TODO:  Keep some things scheduled
            //  so plants keep growing, et c...
            //Schedule.Clear();
            //GameWindow.ClearMessagePause();
        }

        //0.1
        private bool MapLoaded { get; set; } = false;

        private void HandleMenus()
        {
            //TODO:  All these:
            //  Start new game
            //  Load game
            //  Save and Quit
        }

        private void QuitGame()
        {
            //0.1, later verify, offer save
            GameWindow.Close();
        }


        #region Event handlers

        private void MessagePanelFull(object sender, EventArgs args)
        {
            PushMode(EngineMode.MessagesPending, null);
        }

        private void AllMessagesSent(object sender, EventArgs args)
        {
            PopMode();
        }

        #endregion

        #region Crappy first draft leftovers
        private Actor InitPlayer()
        {
            var player = new Actor()
            {
                Name = "Our Dude",
                Symbol = '@',
                ColorForeground = Palette.DbLight,
                Awareness = 222,
                Health = 23
            };

            var hoe = new Hoe(At(0, 0))
            {
                Name = "hoe",
                ColorForeground = RLColor.Brown,
                Symbol = '/',
                IsUsable = true,
            };

            var rock = new Item(At(0, 0))
            {
                Name = "rock",
                ColorForeground = RLColor.Gray,
                Symbol = '*'
            };

            var seed = new Seed(At(0, 0), 1, PlantType.Boomer)
            {
                Name = "seed",
                ColorForeground = RLColor.LightGreen,
                Symbol = '.'
            };

            var seed_2 = new HealerSeed(At(0, 0), 2)
            {
                Name = "seed",
                ColorForeground = RLColor.LightGreen,
                Symbol = '.'
            };

            player.AddToInventory(hoe);
            player.AddToInventory(rock);
            player.AddToInventory(seed);
            player.AddToInventory(seed_2);
            player.WieldedTool = hoe;

            return player;
        }

        private static Point At(int x, int y)
        {
            return new Point(x, y);
        }
        #endregion
    }
}
