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
            Queue<GameCommand> commandQueue, 
            Queue<RLKeyPress> inputQueue, 
            MapLoader mapLoader, 
            IGameState gameState, 
            CommandDispatcher commandDispatcher)
        {
            Bus = bus;
            Bus.AllMessagesSentSubscribers += AllMessagesSent;
            Bus.MessagePanelFullSubscribers += MessagePanelFull;

            Schedule = schedule;
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
            LoadMap("Farm");

            var player = InitPlayer();
            GameState.Player = player;

            var ics = new InputCommandSource(InputQueue, new Describer(), GameWindow);
            player.CommandSource = ics;
            ics.SetActor(player);

            BindInputToFunc(ics.InputUntilCommandGenerated);
            //EnterMode(EngineMode.InputBound);
        }


        private Actor InitPlayer()
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
            RLKeyPress key = GameWindow.GetKeyPress();
            while (key != null)
            {
                if (key.Alt && key.Key == RLKey.F4)
                {
                    //CommandQueue.Enqueue(GameCommand.Quit);
                    QuitGame();
                    return;
                }

                InputQueue.Enqueue(key);
                key = GameWindow.GetKeyPress();
            }
        }

        private Stack<EngineMode> ModeStack = new Stack<EngineMode>();
        public EngineMode Mode { get => ModeStack.Peek(); }
        public void EnterMode(EngineMode newMode)
        {
            ModeStack.Push(newMode);
        }
        public void LeaveMode()
        {
            ModeStack.Pop();
        }

        private Func<IControlPanel, bool> InputUsingCall { get; set; }
        public void BindInputToFunc(Func<IControlPanel, bool> func)
        {
            InputUsingCall = func;
            EnterMode(EngineMode.InputBound);
        }

        private void ActOnMode()
        {
            switch (Mode)
            {
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
                bool stillInputBound = InputUsingCall((IControlPanel) Dispatcher);
                if (!stillInputBound)
                {
                    LeaveMode();
                    InputUsingCall = null;
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
            var nextAction = Schedule.GetNextAction();
            nextAction?.Invoke(Dispatcher);
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
        }

        public void LoadMap(IAreaMap map)
        {
            UnloadCurrentMap();

            foreach (var actor in map.Actors)
            {
                Schedule.Add(actor.NextAction, 12);
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
            Schedule.Clear();
            GameWindow.ClearMessagePause();
        }

        //0.1
        private bool MapLoaded { get; set; } = false;


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
            EnterMode(EngineMode.MessagesPending);
        }

        private void AllMessagesSent(object sender, EventArgs args)
        {
            ModeStack.Pop();
        }

        #endregion
    }
}
