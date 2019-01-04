using System;
using System.Collections.Generic;
using CopperBend.App.Model;
using CopperBend.MapUtil;
using RLNET;

namespace CopperBend.App
{
    public class GameEngine : IGameState
    {
        private RLRootConsole RootConsole;

        private RLConsole MapPane;
        private int MapWidth = 60;
        private int MapHeight = 60;

        private RLConsole StatPane;
        private int StatWidth = 20;
        private int StatHeight = 60;

        private RLConsole TextPane;
        private int TextWidth = 80;
        private int TextHeight = 20;

        private RLConsole LargePane;
        private int LargeWidth = 60;
        private int LargeHeight = 60;
        private bool LargePaneVisible;

        private Queue<RLKeyPress> InputQueue;
        private Queue<GameCommand> CommandQueue;
        private Scheduler Scheduler;
        private CommandDispatcher Dispatcher;
        private Messenger Messenger;
        private MapLoader MapLoader;

        private readonly EventBus Bus;
        public IAreaMap Map { get; private set; }
        public IActor Player { get; private set; }

        #region Initialization
        //  The division of responsibilities among these methods works for now
        //  When load/save comes in, they'll need reworking
        public GameEngine(RLRootConsole console, Actor player)
        {
            RootConsole = console;
            MapPane = new RLConsole(MapWidth, MapHeight);
            StatPane = new RLConsole(StatWidth, StatHeight);
            TextPane = new RLConsole(TextWidth, TextHeight);
            LargePane = new RLConsole(LargeWidth, LargeHeight);
            LargePaneVisible = false;

            Player = player;
            CommandQueue = new Queue<GameCommand>();
            Bus = EventBus.OurBus;
            Bus.AllMessagesSentSubscribers += AllMessagesSent;
            Bus.MessagePanelFullSubscribers += MessagePanelFull;
        }

        public void Run()
        {
            Guard.AgainstNullArgument(Player, "Currently need Player before starting engine.");
            Guard.AgainstNullArgument(Map, "Currently need Map before starting engine.");

            Dispatcher.Init(this);

            RootConsole.Update += onUpdate;
            RootConsole.Render += onRender;
            RootConsole.Run();
        }

        public void StartNewGame()
        {
            InputQueue = new Queue<RLKeyPress>();
            Scheduler = new Scheduler();
            Messenger = new Messenger(InputQueue, TextPane, LargePane);
            Dispatcher = new CommandDispatcher(Scheduler, Messenger);
            MapLoader = new MapLoader();

            LoadMap("Farm");
            Mode = GameMode.PlayerReady;
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
            Messenger.ResetWait();
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
                    Messenger.AddMessage(text);
                }
            }

            //FUTURE:  real-time (background) animation around here

            bool rootDirty = false;

            if (Map.DisplayDirty)
            {
                Map.DrawMap(MapPane);
                RLConsole.Blit(MapPane, 0, 0, MapWidth, MapHeight, RootConsole, 0, 0);
                Map.DisplayDirty = false;
                rootDirty = true;
            }

            //  I haven't even begun to code status reporting
            //  ...I've barely thought about it.
            //  Health reporting should be vague, perhaps just to begin with
            //  Magical energy is called...  something that's not greek.
            //  Perhaps two fatigue/physical energy pools?  Wind and Vitality?
            //  I'm not gathering experience from anywhere yet
            //  No status effects occurring yet (haste, confusion, ...)
            //if (Stats.DisplayDirty)
            //{
            //    Stats.Report(StatConsole);
            //    RLConsole.Blit(MapConsole, 0, 0, StatWidth, StatHeight, RootConsole, MapWidth, 0);
            //    Stats.DisplayDirty = false;
            //    rootDirty = true;
            //}

            //if (Messenger.DisplayDirty)
            //{
            RLConsole.Blit(TextPane, 0, 0, TextWidth, TextHeight, RootConsole, 0, MapHeight);
            rootDirty = true;
            //Messenger.DisplayDirty = false;
            //}

            //  Large messages blitted last since they overlay the rest of the panes
            if (LargePaneVisible)
            {
                RLConsole.Blit(LargePane, 0, 0, LargeWidth, LargeHeight, RootConsole, 10, 10);
            }

            if (rootDirty)
            {
                //RootConsole.Clear();  //  por que?
                RootConsole.Draw();
                rootDirty = false;
            }
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
            RLKeyPress key = RootConsole.Keyboard.GetKeyPress();
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
                    Messenger.AddMessage("At the gate... and... I don't want to leave.  Some important jobs here, first.");
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
                Messenger.HandleLargeMessage();
                break;

            //  Messages waiting for the player block player input and scheduled events
            case GameMode.MessagesPending:
                Messenger.HandlePendingMessages();
                break;

            //  Waiting for player actions blocks Scheduler
            case GameMode.PlayerReady:
                Messenger.ResetWait();
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
            RootConsole.Close();
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
