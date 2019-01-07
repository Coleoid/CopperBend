using System;
using System.Collections.Generic;
using CopperBend.App.Model;
using CopperBend.MapUtil;
using RLNET;

namespace CopperBend.App
{
    public class GameEngine : IGameState
    {
        private GameWindow GameWindow;

        private Queue<RLKeyPress> InputQueue;
        private Queue<GameCommand> CommandQueue;
        private Scheduler Scheduler;
        private CommandDispatcher Dispatcher;
        private MapLoader MapLoader;

        private readonly EventBus Bus;
        public IAreaMap Map { get; private set; }
        public IActor Player { get; private set; }

        #region Initialization
        //  The division of responsibilities among these methods works for now
        //  When load/save comes in, they'll need reworking
        public GameEngine()
        {
            InputQueue = new Queue<RLKeyPress>();
            GameWindow = new GameWindow(InputQueue);

            CommandQueue = new Queue<GameCommand>();
            Bus = EventBus.OurBus;
            Bus.AllMessagesSentSubscribers += AllMessagesSent;
            Bus.MessagePanelFullSubscribers += MessagePanelFull;

            Scheduler = new Scheduler();
            Dispatcher = new CommandDispatcher(Scheduler, GameWindow);
            MapLoader = new MapLoader();
        }

        public void Run()
        {
            Guard.AgainstNullArgument(Player, "Currently need Player before starting engine.");
            Guard.AgainstNullArgument(Map, "Currently need Map before starting engine.");

            Dispatcher.Init(this);

            GameWindow.Run(onUpdate, onRender);
        }

        public void StartNewGame()
        {
            // recreate or clear existing game objects?
            Player = InitPlayer();
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
            Player.MoveTo(Map.PlayerStartsAt);
            Map.UpdatePlayerFieldOfView(Player);
            Bus.SendLargeMessage(this, Map.FirstSightMessage);
        }

        public void LoadMap(IAreaMap map)
        {
            UnloadCurrentMap();

            Map = map;
            foreach (var actor in map.Actors)
            {
                Scheduler.Add(new ScheduleEntry(12, null, actor));
            }

            map.ViewpointActor = Player;
            map.Actors.Add(Player);
        }

        public void UnloadCurrentMap()
        {
            //later persist map changes off
            //later I want to leave some (all?) things scheduled,
            //so plants keep growing, et c...
            Scheduler.Clear();
            GameWindow.ResetWait();
        }

        public void QueueCommand(GameCommand command)
        {
            CommandQueue.Enqueue(command);
        }

        //0.1
        private bool MapLoaded = false;

        private void onRender(object sender, UpdateEventArgs e)
        {
            //0.1
            if (!MapLoaded)
            {
                MapLoaded = true;
                foreach (var text in Map.FirstSightMessage)
                {
                    GameWindow.AddMessage(text);
                }
            }

            GameWindow.Render(Map);
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

                case GameCommand.GoToFarmhouse:
                    LoadMap("Farmhouse");
                    break;

                case GameCommand.NotReadyToLeave:
                    Player.MoveTo(new Point(7, 17));
                    GameWindow.AddMessage("At the gate... and... I don't want to leave.  Some important jobs here, first.");
                    break;

                case GameCommand.Unset:
                    throw new Exception("Command unset--preparation missed.");

                default:
                    throw new Exception($"Haven't coded case [{command}] yet.");
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

            //  Waiting for player actions blocks Scheduler
            case GameMode.PlayerReady:
                GameWindow.ResetWait();
                Dispatcher.HandlePlayerCommands();
                break;

            //  When the player has committed to a slow action, time passes
            case GameMode.Schedule:
                Scheduler.DoNext(Dispatcher);
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
