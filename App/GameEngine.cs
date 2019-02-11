using System;
using System.Collections.Generic;
using CopperBend.App.Model;
using CopperBend.MapUtil;
using RLNET;

namespace CopperBend.App
{
    public class GameEngine
    {
        public bool Config_AutoLoad_new_game { get; set; }

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
            Bus = bus;
            Bus.AllMessagesSentSubscribers += AllMessagesSent;
            Bus.MessagePanelFullSubscribers += MessagePanelFull;
            Bus.EnterEngineModeSubscribers += (s, a) => EnterMode(a);  //0.1

            Schedule = schedule;
            GameWindow = gameWindow;
            Dispatcher = commandDispatcher;

            InputQueue = inputQueue;
            MapLoader = mapLoader;
            GameState = (GameState)gameState;  // not great, but working...
        }

        public void Run()
        {
            if (Config_AutoLoad_new_game) LoadNewGame();
            GameWindow.Run(onUpdate, onRender);
        }

        public void LoadNewGame()
        {
            // recreate or clear existing game objects?
            LoadMap("Farm");

            var player = InitPlayer();
            GameState.Player = player;

            var ics = new InputCommandSource(InputQueue, new Describer(), GameWindow, Bus);
            player.CommandSource = ics;

            //BindInputToCallback(ics.InputUntilCommandGenerated);
            //EnterMode(EngineMode.InputBound);
        }

        public void LoadMap(string mapName)
        {
            UnloadCurrentMap();

            var map = MapLoader.LoadDevMap(mapName, GameState);

            foreach (var actor in map.Actors)
            {
                Schedule.Add(actor.NextAction, 12);
            }

            Bus.SendLargeMessage(this, map.FirstSightMessage);
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
            //WorkCommandQueue();
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
                    //CommandQueue.Enqueue(GameCommand.Quit);
                    QuitGame();
                    return;
                }

                InputQueue.Enqueue(press);
                press = GameWindow.GetKeyPress();
            }
        }

        private Stack<EngineMode> ModeStack = new Stack<EngineMode>();
        private Stack<Func<IControlPanel, bool>> CallbackStack = new Stack<Func<IControlPanel, bool>>();
        public EngineMode Mode { get => ModeStack.Peek(); }

        public void EnterMode(EnterModeEventArgs args)
        {
            EnterMode(args.Mode, args.Callback);
        }

        public void EnterMode(EngineMode newMode, Func<IControlPanel, bool> callback)
        {
            switch (newMode)
            {
            case EngineMode.InputBound:
            case EngineMode.Pause:
                ModeCallback = callback;
                ModeStack.Push(newMode);
                CallbackStack.Push(callback);
                break;

            case EngineMode.Schedule:
                ModeCallback = null;
                ModeStack.Push(newMode);
                CallbackStack.Push(null);
                break;

            case EngineMode.Unknown:
                throw new Exception($"Should never EnterMode({newMode}).");
            default:
                throw new Exception($"Received EnterMode({newMode}) event, need code.");
            }
        }
        public void LeaveMode()
        {
            ModeStack.Pop();
        }

        internal Func<IControlPanel, bool> ModeCallback { get; set; }

        internal void ActOnMode()
        {
            switch (Mode)
            {
            //  Update does nothing when paused
            case EngineMode.Pause:
                bool unPause = ModeCallback(Dispatcher);
                if (unPause)
                {
                    LeaveMode();
                    ModeCallback = null;
                }
                return;

            //  A game menu will block even pending messages 
            case EngineMode.MenuOpen:
                HandleMenus();
                break;

            //  The large message pane overlays most of the game
            case EngineMode.LargeMessagePending:
                GameWindow.HandleLargeMessage();
                break;

            //  Messages waiting for the player block player input and scheduled events
            case EngineMode.MessagesPending:
                GameWindow.HandlePendingMessages();
                break;

            //MAYBE:  A "cinematic" mode for story scenes
            // Player input not routed as normal commands, though it
            //   still handles message pauses, escape-to-menu, or choices
            //   presented by the scene.
            // NPC/Creature/world effects advance at dramatic (slow) pacing.
            //case EngineMode.Cinematic:
            //    GameWindow.MuchScene();
            //    break;

            //  Waiting for player input blocks Schedule
            case EngineMode.InputBound:
                bool unbindInput = ModeCallback(Dispatcher);
                if (unbindInput)
                {
                    LeaveMode();
                    ModeCallback = null;
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
                throw new Exception($"Game mode [{Mode}] not written yet.");
            }
        }

        public void DoNextScheduled()
        {
            while (Mode == EngineMode.Schedule)
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
            Schedule.Clear();
            GameWindow.ClearMessagePause();
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
            EnterMode(EngineMode.MessagesPending, null);
        }

        private void AllMessagesSent(object sender, EventArgs args)
        {
            ModeStack.Pop();
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
