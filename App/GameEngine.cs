using System;
using System.Collections.Generic;
using CopperBend.App.Model;
using CopperBend.MapUtil;
using RLNET;

namespace CopperBend.App
{
    public class GameEngine
    {
        private IGameWindow GameWindow;
        private Queue<GameCommand> CommandQueue;
        private Queue<RLKeyPress> InputQueue;
        private Schedule _schedule;
        private CommandDispatcher Dispatcher;
        private MapLoader MapLoader;
        private GameState GameState;
        private readonly EventBus Bus;

        #region Initialization
        //  The division of responsibilities among these methods works for now
        //  When load/save comes in, they'll need reworking
        public GameEngine(
            EventBus bus, 
            Schedule schedule, 
            IGameWindow gameWindow, 
            Queue<GameCommand> commandQueue, 
            Queue<RLKeyPress> inputQueue, 
            MapLoader mapLoader, 
            IGameState gameState, 
            CommandDispatcher commandDispatcher)
        {
            Bus = bus;
            Bus.AllMessagesSentSubscribers += AllMessagesSent;
            Bus.MessagePanelFullSubscribers += MessagePanelFull;

            _schedule = schedule;
            GameWindow = gameWindow;
            Dispatcher = commandDispatcher;

            CommandQueue = commandQueue;
            InputQueue = inputQueue;
            MapLoader = mapLoader;
            GameState = (GameState)gameState;  // not great, but working...
        }

        public void Run()
        {
            LoadNewGame();
            GameWindow.Run(onUpdate, onRender);
        }

        public void LoadNewGame()
        {
            // recreate or clear existing game objects?
            GameState.Player = InitPlayer();
            LoadMap("Farm");
            Mode = GameMode.PlayerReady;
        }


        private static Actor InitPlayer()
        {
            var player = new Actor(At(0, 0))
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

        private void LoadMap(string mapName)
        {
            IAreaMap map;
            if (mapName == "Farm")
                map = MapLoader.FarmMap();
            else if (mapName == "Farmhouse")
                map = MapLoader.FarmhouseMap();
            else
                map = MapLoader.DemoMap();

            LoadMap(map);
        }

        public void LoadMap(IAreaMap map)
        {
            UnloadCurrentMap();

            foreach (var actor in map.Actors)
            {
                _schedule.Add(new ScheduleEntry(12, null, actor));
            }

            map.ViewpointActor = GameState.Player;
            map.Actors.Add(GameState.Player);
            GameState.Player.MoveTo(map.PlayerStartsAt);
            map.UpdatePlayerFieldOfView(GameState.Player);
            Bus.SendLargeMessage(this, map.FirstSightMessage);
            GameState.Map = map;
        }

        public void UnloadCurrentMap()
        {
            //TODO: persist map content/changes

            //TODO:  Keep some things scheduled
            //  so plants keep growing, et c...
            _schedule.Clear();
            GameWindow.ResetWait();
        }

        public void QueueCommand(GameCommand command)
        {
            CommandQueue.Enqueue(command);
        }

        //0.1
        private bool MapLoaded { get; set; } = false;

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
            ReadInput();
            WorkCommandQueue();
            ActOnMode();
        }

        private void ReadInput()
        {
            //  For now, only checking the keyboard for input
            RLKeyPress key = GameWindow.RootConsole.Keyboard.GetKeyPress();
            if (key != null)
            {
                if (key.Alt && key.Key == RLKey.F4)
                {
                    CommandQueue.Enqueue(GameCommand.Quit);
                    return;
                }

                InputQueue.Enqueue(key);
            }
        }

        private void WorkCommandQueue()
        {
            while (CommandQueue.Count > 0)
            {
                var command = CommandQueue.Dequeue();
                switch (command)
                {
                case GameCommand.Quit:
                    QuitGame();
                    break;

                //0.1
                case GameCommand.GoToFarmhouse:
                    LoadMap("Farmhouse");
                    break;

                //0.1
                case GameCommand.NotReadyToLeave:
                    GameState.Player.MoveTo(new Point(7, 17));
                    GameWindow.AddMessage("At the gate... and... I don't want to leave.  Some important jobs here, first.");
                    break;

                case GameCommand.Unset:
                    throw new Exception("Dev error:  Unset command on the queue--preparation missed.");

                default:
                    throw new Exception($"Dev error:  Don't know how to work the [{command}] command yet.");
                }
            }
        }

        public GameMode Mode { get; set; }
        private void ActOnMode()
        {
            switch (Mode)
            {
            //  A game menu will block even pending messages 
            case GameMode.MenuOpen:
                HandleMenus();
                break;

            //  The large message pane overlays most of the game
            case GameMode.LargeMessagePending:
                GameWindow.HandleLargeMessage();
                break;

            //  Messages waiting for the player block player input and scheduled events
            case GameMode.MessagesPending:
                GameWindow.HandlePendingMessages();
                break;

            //  Waiting for player input blocks Schedule
            case GameMode.PlayerReady:
                GameWindow.ResetWait();
                Dispatcher.HandlePlayerCommands();
                break;

            //  When the player has committed to a slow action, time passes
            case GameMode.Schedule:
                _schedule.DoNext(Dispatcher);
                break;

            case GameMode.Unknown:
                throw new Exception("Game mode unknown, perhaps Init() was missed.");

            default:
                throw new Exception($"Game mode [{Mode}] not written yet.");
            }
        }

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
            GameWindow.RootConsole.Close();
        }


        #region Event handlers

        private void MessagePanelFull(object sender, EventArgs args)
        {
            Mode = GameMode.MessagesPending;
        }

        private void AllMessagesSent(object sender, EventArgs args)
        {
            Mode = Dispatcher.IsPlayerScheduled ?
                GameMode.Schedule : GameMode.PlayerReady;
        }

        #endregion
    }
}
