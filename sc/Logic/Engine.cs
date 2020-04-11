using System;
using System.Diagnostics;
using System.Linq;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Size = System.Drawing.Size;
using log4net;
using SadConsole;
using SadConsole.Input;
using SadGlobal = SadConsole.Global;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;

namespace CopperBend.Logic
{
    /// <summary> Main game mechanisms. </summary>
    public partial class Engine : ContainerConsole
    {
        public Size GameSize { get; set; }
        public Size MapSize { get; set; }
        public Size MapWindowSize { get; set; }
        public Size MenuWindowSize { get; set; }

        private ScrollingConsole MapConsole { get; set; }
        public Window MapWindow { get; set; }
        public IMessageLogWindow MessageWindow { get; set; }
        private Window MenuWindow { get; set; }
        private ControlsConsole MenuConsole { get; set; }

        //public Queue<AsciiKey> InputQueue { get; private set; }
        private Being Player { get; set; }

        private bool GameInProgress { get; set; }
        private int TickWhenGameLastSaved { get; set; }
        private string TopSeed { get; set; }
        private ICompoundMap FullMap { get; set; }
        private readonly bool jumpToDebugger = true;


        // ===========  Constructor args
        private readonly ILog log;
        private ISchedule Schedule { get; set; }
        private ISadConEntityFactory SadConEntityFactory { get; set; }
        private Keyboard Keyboard { get; set; }
        private IUIBuilder UIBuilder { get; set; }
        private IDescriber Describer { get; set; }
        private IGameState GameState { get; set; }
        private IControlPanel Dispatcher { get; set; }
        private ModeNode ModeNode { get; set; }
        private IMessager Messager { get; set; }

        #region Init
        public Engine(
                ILog logger,
                ISchedule schedule,
                ISadConEntityFactory scef,
                Keyboard keyboard,
                Size gameSize,
                IUIBuilder uiBuilder,
                IDescriber describer,
                IGameState gameState,
                IControlPanel dispatcher,
                ModeNode modeNode,
                IMessager messager
            )
            : base()
        {
            log = logger;
            Parent = SadGlobal.CurrentScreen;
            Schedule = schedule;
            SadConEntityFactory = scef;
            Keyboard = keyboard;
            GameSize = gameSize;
            UIBuilder = uiBuilder;
            Describer = describer;  // (must be attached to Herbal &c per-game)
            GameState = gameState;
            Dispatcher = dispatcher;
            ModeNode = modeNode;
            Messager = messager;
        }

        //0.1: Tease apart distinct start-up goals:
        //  Loading a saved game,
        //  Loading debug "saved game" configurations
        /// <summary> Initialize more complex game systems </summary>
        public void Init(string topSeed = null)
        {
            TopSeed = topSeed;

            IsVisible = true;
            IsFocused = true;

            GameInProgress = false;
            ModeNode.PushEngineMode(EngineMode.NoGameRunning, null);

            MapWindowSize = new Size(GameSize.Width * 2 / 3, GameSize.Height - 8);
            MenuWindowSize = new Size(GameSize.Width - 20, GameSize.Height / 4);

            //0.2.MAP: Put map name in YAML -> CompoundMap -> CreateMapWindow

            (MenuConsole, MenuWindow) = UIBuilder.CreateM2Window(MenuWindowSize, "Game Menu");
            Children.Add(MenuWindow);

            MessageWindow = UIBuilder.CreateMessageLog();
            Children.Add((SadConsole.Console)MessageWindow);
            MessageWindow.Show();

            OpenGameMenu();
        }

        public void OpenGameMenu()
        {
            Guard.Against(ModeNode.CurrentMode == EngineMode.MenuOpen);

            MenuConsole.Clear();
            MenuConsole.Print(2, 4, GameInProgress ? "R) Return to game" : "B) Begin new game");
            MenuConsole.Print(2, 6, "Q) Quit");

            MenuWindow.Show();
            ModeNode.PushEngineMode(EngineMode.MenuOpen, null);
        }

        public void BeginNewGame()
        {
            if (TopSeed == null)
                TopSeed = GenerateSimpleTopSeed();
            log.InfoFormat("Beginning new game with Top Seed [{0}]", TopSeed);
            Cosmogenesis(TopSeed, SadConEntityFactory);
            TopSeed = null;
            Equipper.Herbal = Compendium.Herbal;
            Describer.Scramble();

            var loader = new Persist.MapLoader(log);
            FullMap = loader.FarmMap();

            Player = Engine.Compendium.SocialRegister.CreatePlayer(FullMap.SpaceMap.PlayerStartPoint);
            log.Debug("Created player character.");

            Schedule.AddAgent(Player, 12);

            (MapConsole, MapWindow) = UIBuilder.CreateMapWindow(MapWindowSize, "A Farmyard", FullMap);
            Children.Add(MapWindow);
            MapConsole.Children.Add(Player.Console);
            FullMap.SetInitialConsoleCells(MapConsole, FullMap.SpaceMap);
            FullMap.FOV = new FOV(FullMap.GetView_CanSeeThrough());
            FullMap.UpdateFOV(MapConsole, Player.Position);
            MapWindow.Show();

            GameState = new GameState
            {
                Player = Player,
                Map = FullMap,
                Story = Engine.Compendium.Dramaticon,
            };
            Dispatcher.GameState = GameState;
            Dispatcher.AttackSystem.SetRotMap(GameState.Map.RotMap);

            Player.CommandSource = new InputCommandSource(log, Describer, GameState, Dispatcher, ModeNode, Messager);
            MapConsole.CenterViewPortOnPoint(Player.Position);

            ModeNode.PushEngineMode(EngineMode.WorldTurns, null);
            GameInProgress = true;
            log.Info("Began new game");
        }

        /// <summary> I hope you had fun! </summary>
        public void GameOver(IBeing player, PlayerDiedException pde)
        {
            //Dispatcher.WriteLine("After you're dead, the town of Copper Bend in Kulkecharra Valley is");
            //Dispatcher.WriteLine("overrun by the Rot.  Every creature flees, is absorbed");
            //Dispatcher.WriteLine("for energy, or suffers a brief quasi-life as the Rot");
            //Dispatcher.WriteLine("strives to understand and replace life.  What will stand in its way?");

            WriteGameOverReport(pde);
            ClearStats();

            ModeNode.PopEngineMode();
            GameInProgress = false;

            ShutDownGame();
            OpenGameMenu();
        }

        public void WriteGameOverReport(PlayerDiedException pde) { } //0.0: WriteGameOverReport
        public void ClearStats() { } //0.0: ClearStats

        private void ShutDownGame()
        {
            // Start by simply undoing many NewGame steps in reverse order
            log.Info("Shutting down game");

            Player.CommandSource = null;

            Dispatcher.AttackSystem.RotMap = null;
            Dispatcher.GameState = null;

            GameState = null;

            FullMap.FOV = null;

            MapConsole.Children.Remove(Player.Console);
            Children.Remove(MapWindow);
            MapConsole = null;
            MapWindow = null;

            Schedule.Clear();
            Player = null;
            FullMap = null;

            log.Info("Shut down game");
        }
        #endregion

        public override void Update(TimeSpan timeElapsed)
        {
            QueueInput();

            try
            {
                ActOnMode();  // <<==
            }
            catch (PlayerDiedException pde)
            {
                GameOver(Player, pde);
            }

            if (GameInProgress)
            {
                SyncMapChanges();
                AnimateBackground(timeElapsed);
            }

            base.Update(timeElapsed);
        }


        internal void ActOnMode()
        {
            switch (ModeNode.CurrentMode)
            {
            //  The game menu being open has priority
            case EngineMode.MenuOpen:
                HandleGameMenu();
                break;

            //  When the player is busy or incapacitated,
            //  time passes in the game world, events occur.
            case EngineMode.WorldTurns:
                HandleScheduledEvents();
                break;

            //  When the player is choosing their action
            case EngineMode.PlayerTurn:
                ModeNode.CurrentCallback();
                Messager.ResetMessagesSentSincePause();
                break;

            //  When enough small messages have been printed that the
            //  UI is waiting with a '- more -' style prompt
            case EngineMode.MessagesPendingUserInput:
                ModeNode.CurrentCallback();
                break;

            //  The large message pane (inventory, log, ...) overlays most of the game
            case EngineMode.LargeMessagePending:
                Messager.HandleLargeMessage();
                break;

            //  Reaching this branch is always developer error
            case EngineMode.Unknown:
            case EngineMode.NoGameRunning:
            default:
                // JumpToDebugger compiles to false for production builds
                if (jumpToDebugger)
                {
                    if (!Debugger.IsAttached) Debugger.Launch();
                    else Debugger.Break();
                }
                else
                {
                    throw new Exception($"Hit the game loop in {ModeNode.CurrentMode} mode.");
                }
                break;
            }
        }

        public void QueueInput()
        {
            bool ShouldClear()
            {
                MenuWindow.Show();
                ModeNode.PushEngineMode(EngineMode.MenuOpen, () => { });
                return true;
            }
            Messager.ShouldClearQueueOnEscape = ShouldClear;
            Messager.QueueInput(Keyboard.KeysPressed);
            //1.+: Capturing mouse events can wait until post 1.0.
        }

        private void HandleGameMenu()
        {
            for (AsciiKey press = Messager.GetNextKeyPress(); press.Key != Keys.None; press = Messager.GetNextKeyPress())
            {
                // Quit
                if (press.Key == Keys.Q)
                {
                    if (GameInProgress && Schedule.CurrentTick > TickWhenGameLastSaved)
                    {
                        //0.0: verify with player "Quit with unsaved changes?"
                    }
                    Game.Instance.Exit();
                }

                if (GameInProgress)
                {
                    // Return to game
                    if (press.Key == Keys.R || press.Key == Keys.Escape)
                    {
                        ModeNode.PopEngineMode();
                        MenuWindow.Hide();
                        return;
                    }

                    //0.0: Save and unload current game
                    if (press.Key == Keys.S)
                    {
                        TickWhenGameLastSaved = Schedule.CurrentTick;
                        // ActuallySaveTheGame();
                        GameInProgress = false;
                        // UnloadManyFriends();
                        // redraw menu options
                    }
                }
                else
                {
                    // Begin new game
                    if (press.Key == Keys.B)
                    {
                        ModeNode.PopEngineMode();
                        MenuWindow.Hide();
                        BeginNewGame();
                        return;
                    }

                    //0.0: Load saved game and resume play
                    if (press.Key == Keys.L)
                    {
                        ModeNode.PopEngineMode();
                        MenuWindow.Hide();
                        //PickAndResumeSavedGame();
                        TickWhenGameLastSaved = Schedule.CurrentTick;
                        return;
                    }

                    //0.0: Generate new seed, or enter new seed
                }
            }
        }

        public void HandleScheduledEvents()
        {
            while (ModeNode.CurrentMode == EngineMode.WorldTurns)
            {
                var nextAction = Schedule.GetNextAction();
                Dispatcher.Dispatch(nextAction);
            }
        }

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
                .Intersect(FullMap.FOV.CurrentFOV);
            FullMap.UpdateViewOfCoords(MapConsole, changedAndInFOV);
            FullMap.CoordsWithChanges.Clear();
        }

        private void AnimateBackground(TimeSpan timeElapsed)
        {
            FullMap.EffectsManager.UpdateEffects(timeElapsed.TotalSeconds);
        }

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
