using System;
using System.Collections.Generic;
using System.Linq;
using Size = System.Drawing.Size;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using log4net;
using SadConsole;
using SadConsole.Input;
using SadConsole.Components;
using SadConState = SadConsole.Global;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;

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
        private Being Player;

        private GameState GameState;
        private Schedule Schedule;
        private CommandDispatcher Dispatcher;
        private IDGenerator IDGenerator;

        #region Init
        public Engine(int gameWidth, int gameHeight)
            : base()
        {
            log = LogManager.GetLogger("CB", "CB.Engine");

            IsVisible = true;
            IsFocused = true;

            GameSize = new Size(gameWidth, gameHeight);
            MapWindowSize = new Size(GameSize.Width * 2 / 3, GameSize.Height - 8);

            Parent = SadConState.CurrentScreen;
            Kbd = SadConState.KeyboardState;

            InputQueue = new Queue<AsciiKey>();
            ModeStack = new Stack<EngineMode>();
            CallbackStack = new Stack<Func<bool>>();

            Init();
        }

        private ICompoundMap FullMap;
        public void Init()
        {
            PushEngineMode(EngineMode.StartUp, null);
            PrepareIDGeneration();
            PreparePlants();

            var loader = new MapLoader();
            FullMap = loader.FarmMap();
            log.Debug("Generated map");

            Describer describer = new Describer();
            Schedule = new Schedule();
            Player = CreatePlayer(FullMap.SpaceMap.PlayerStartPoint);
            Schedule.AddAgent(Player, 12);

            var builder = new UIBuilder(GameSize);
            (MapConsole, MapWindow) = builder.CreateMapWindow(MapWindowSize, "Game Map", FullMap);
            Children.Add(MapWindow);
            MapConsole.Children.Add(Player);
            FullMap.SetInitialConsoleCells(MapConsole, FullMap.SpaceMap);
            FullMap.FOV = new FOV(FullMap.GetView_CanSeeThrough());
            FullMap.UpdateFOV(MapConsole, Player.Position);
            MapWindow.Show();

            MessageLog = builder.CreateMessageLog();
            Children.Add(MessageLog);
            MessageLog.Show();

            GameState = new GameState
            {
                Player = Player,
                Map = FullMap
            };

            Dispatcher = new CommandDispatcher(Schedule, GameState, describer, MessageLog)
            {
                PushEngineMode = PushEngineMode,

                IsInputReady = () => InputQueue.Count > 0,
                GetNextInput = InputQueue.Dequeue,
                ClearPendingInput = InputQueue.Clear,
                AddMessage = MessageLog.Add,
            };

            Player.CommandSource = new InputCommandSource(describer, GameState, Dispatcher);

            MapConsole.CenterViewPortOnPoint(Player.Position);

            PushEngineMode(EngineMode.Schedule, null);
        }

        private void PrepareIDGeneration()
        {
            // On this path, each new IDed type needs addition here,
            // yet no IDs will ever clash.  I like this tradeoff.
            IDGenerator = new IDGenerator();

            CbEntity.IDGenerator = IDGenerator;
            Item.IDGenerator = IDGenerator;
            Space.IDGenerator = IDGenerator;
            AreaBlight.IDGenerator = IDGenerator;
        }

        private Dictionary<string, PlantDetails> PlantsByName;
        private Dictionary<uint, PlantDetails> PlantsByID;
        private void PreparePlants()
        {
            PlantsByName = new Dictionary<string, PlantDetails>();
            PlantsByID = new Dictionary<uint, PlantDetails>();

            AddPlantDetails( new PlantDetails {
                ID = 1,
                MainName = "Boomer",
                GrowthTime = 400
            });
            AddPlantDetails(new PlantDetails
            {
                ID = 2,
                MainName = "Healer",
                GrowthTime = 400
            });
            AddPlantDetails(new PlantDetails
            {
                ID = 3,
                MainName = "Thornfriend",
                GrowthTime = 400
            });

            Seed.PlantByID = PlantsByID;
            Seed.PlantByName = PlantsByName;
            Fruit.PlantByID = PlantsByID;
            Fruit.PlantByName = PlantsByName;
            Describer.PlantByID = PlantsByID;
            Describer.PlantByName = PlantsByName;
        }

        private void AddPlantDetails(PlantDetails plant)
        {
            PlantsByID[plant.ID] = plant;
            PlantsByName[plant.MainName] = plant;
        }

        private Being CreatePlayer(Coord playerLocation)
        {
            var player = new Player(Color.AntiqueWhite, Color.Transparent)
            {
                Name = "Suvail",
                Position = playerLocation
            };
            player.Animation.CurrentFrame[0].Glyph = '@';
            player.Animation.CurrentFrame[0].Foreground = Color.AntiqueWhite;
            player.Components.Add(new EntityViewSyncComponent());
            player.AddToInventory(new Hoe((0,0)));
            player.AddToInventory(new Seed((0,0), 2, PlantsByName["Healer"].ID));

            log.Debug("Created player.");
            return player;
        }
        #endregion

        public override void Update(TimeSpan timeElapsed)
        {
            QueueInput();
            ActOnMode();
            SyncMapChanges();

            AnimateBackground(timeElapsed);

            base.Update(timeElapsed);
        }

        private void AnimateBackground(TimeSpan timeElapsed)
        {
            FullMap.EffectsManager.UpdateEffects(timeElapsed.TotalSeconds);
        }

        public void QueueInput()
        {
            //  0.5, later other sources of input
            foreach (var key in Kbd.KeysPressed)
            {
                InputQueue.Enqueue(key);
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

            if (oldMode == EngineMode.Schedule && CurrentMode == EngineMode.InputBound) return;
            log.Debug($"Pushed mode {oldMode} down, now in {CurrentMode}.");
        }
        internal void PopEngineMode()
        {
            var oldMode = ModeStack.Pop();
            CallbackStack.Pop();

            if (oldMode == EngineMode.InputBound && CurrentMode == EngineMode.Schedule) return;
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
                    AddMessage(nextMessage);
                    ShownMessages++;
                }
                else
                {
                    AddMessage("-- more --");
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


        public void SyncMapChanges()
        {
            //  If any spaces changed whether they can be seen through, rebuild FOV
            if (FullMap.VisibilityChanged)
            {
                FullMap.FOV = new FOV(FullMap.GetView_CanSeeThrough());
                FullMap.VisibilityChanged = false;
                Dispatcher.PlayerMoved = true;
            }

            if (Dispatcher.PlayerMoved)
            {
                //TODO:  Events at locations on map:  CheckActorAtCoordEvent(actor, tile);

                FullMap.UpdateFOV(MapConsole, Player.Position);
                MapConsole.CenterViewPortOnPoint(Player.Position);
                Dispatcher.PlayerMoved = false;
            }

            if (!FullMap.CoordsWithChanges.Any()) return;
            var changedAndInFOV = FullMap.CoordsWithChanges
                .Intersect(FullMap.FOV.CurrentFOV)
                .Where(c => MapConsole.ViewPort.Contains(c));
            FullMap.UpdateViewOfCoords(MapConsole, changedAndInFOV);
            FullMap.CoordsWithChanges.Clear();
        }

        #region LargeMessages and Menus, currently empty
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
        #endregion

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
