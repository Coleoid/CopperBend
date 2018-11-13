using System;
using System.Collections.Generic;
using CopperBend.App.Model;
using CopperBend.MapUtil;
using RLNET;

namespace CopperBend.App
{
    public class GameEngine : IGameState
    {
        private RLRootConsole GameConsole;
        private Queue<RLKeyPress> InputQueue;
        private Queue<GameCommand> CommandQueue;
        private Scheduler Scheduler;
        private CommandDispatcher Dispatcher;
        private Messenger Messenger;
        private MapLoader MapLoader;

        public IAreaMap Map { get; private set; }
        public IActor Player { get; private set; }

        public GameEngine(RLRootConsole console, Actor player)
        {
            GameConsole = console;
            Player = player;
            CommandQueue = new Queue<GameCommand>();
        }

        public void StartNewGame()
        {
            InputQueue = new Queue<RLKeyPress>();
            Scheduler = new Scheduler();
            Dispatcher = new CommandDispatcher(InputQueue, Scheduler);
            Messenger = new Messenger(Dispatcher);
            MapLoader = new MapLoader();

            LoadMap("Farm");
            Mode = GameMode.PlayerReady;
        }

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
            Messenger.ClearMessagePanel();
        }

        public void Run()
        {
            //later these will be untrue, with load game and create new game
            if (Player == null) throw new Exception("Must have Player before starting engine.");
            if (Map == null) throw new Exception("Must have Map before starting engine.");

            Dispatcher.Init(this);

            GameConsole.Update += onUpdate;
            GameConsole.Render += onRender;
            GameConsole.Run();
        }

        private bool MapLoaded = false;

        private void onRender(object sender, UpdateEventArgs e)
        {
            //larval form of map.FirstLoaded event
            if (!MapLoaded)
            {
                MapLoaded = true;
                foreach (var text in Map.FirstSightMessages)
                {
                    Messenger.Message(text);
                }
            }

            //FUTURE:  real-time (background) animation around here

            //  If the map hasn't changed, why render?
            if (!Map.DisplayDirty) return;

            GameConsole.Clear();
            Map.DrawMap(GameConsole);
            GameConsole.Draw();
            Map.DisplayDirty = false;


            RLConsole.Blit(_mapConsole, 0, 0, _mapWidth, _mapHeight, _rootConsole, 0, _inventoryHeight);
            RLConsole.Blit(_statConsole, 0, 0, _statWidth, _statHeight, _rootConsole, _mapWidth, 0);
            RLConsole.Blit(_messageConsole, 0, 0, _messageWidth, _messageHeight, _rootConsole, 0, _screenHeight - _messageHeight);
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
            RLKeyPress key = GameConsole.Keyboard.GetKeyPress();
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
                    Player.MoveTo(new Point(2, 2));
                    break;

                case GameCommand.Unset:
                    throw new Exception("Command unset--preparation missed.");

                default:
                    throw new Exception($"Haven't coded case [{command}] yet.");
                }
            }
        }

        private void QuitGame()
        {
            //0.1, later verify, offer save
            GameConsole.Close();
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

            //  Messages waiting for the player block player input and scheduled events
            case GameMode.MessagesPending:
                Messenger.HandlePendingMessages();
                break;

            //  Waiting for player actions blocks Scheduler
            case GameMode.PlayerReady:
                Messenger.ClearMessagePanel();
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

        public void QueueCommand(GameCommand command)
        {
            CommandQueue.Enqueue(command);
        }
    }
}
