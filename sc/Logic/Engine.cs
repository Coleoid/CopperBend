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
using SadConsole.Ansi;

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

        private IBeing Player { get; set; }

        private bool GameInProgress { get; set; }
        private int TickWhenGameLastSaved { get; set; }
        private string TopSeed { get; set; }
        private ICompoundMap FullMap { get; set; }
        private readonly bool jumpToDebugger = true;


        // ===========  Constructor args
        //NEXT, these get folded into ServicePanel
        private ISadConEntityFactory SadConEntityFactory { get; set; }
        private Keyboard Keyboard { get; set; }
        private IUIBuilder UIBuilder { get; set; }
        private IGameState GameState { get; set; }
        private IControlPanel Dispatcher { get; set; }

        private IServicePanel ServicePanel { get; set; }
        //private IDescriber Describer { get => ServicePanel.Describer; }
        private IMessager Messager { get => ServicePanel.Messager; }
        private ILog Log { get => ServicePanel.Log; }
        private IGameMode GameMode { get => ServicePanel.GameMode; }
        private ISchedule Schedule { get => ServicePanel.Schedule; }


        #region Init
        public Engine(
                IServicePanel servicePanel,
                ISadConEntityFactory scef,
                Keyboard keyboard,
                Size gameSize,
                IUIBuilder uiBuilder,
                IGameState gameState,
                IControlPanel dispatcher
            )
            : base()
        {
            ServicePanel = servicePanel;
            SadConEntityFactory = scef;
            Keyboard = keyboard;
            GameSize = gameSize;
            UIBuilder = uiBuilder;
            GameState = gameState;
            Dispatcher = dispatcher;

            Parent = SadGlobal.CurrentScreen;
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
            GameMode.PushEngineMode(EngineMode.NoGameRunning, null);

            MapWindowSize = new Size(GameSize.Width * 2 / 3, GameSize.Height - 8);
            MenuWindowSize = new Size(GameSize.Width - 20, GameSize.Height / 4);

            //0.2.MAP: Put map name in YAML -> CompoundMap -> CreateMapWindow

            (MenuConsole, MenuWindow) = UIBuilder.CreateM2Window(MenuWindowSize, "Game Menu");
            Children.Add(MenuWindow);

            MessageWindow = UIBuilder.CreateMessageLog();
            this.ServicePanel.Messager.MessageWindow = MessageWindow;
            Children.Add((SadConsole.Console)MessageWindow);
            MessageWindow.Show();

            OpenGameMenu();
        }
        #endregion

        public void OpenGameMenu()
        {
            Guard.Against(GameMode.CurrentMode == EngineMode.MenuOpen);

            MenuConsole.Clear();
            MenuConsole.Print(2, 4, GameInProgress ? "R) Return to game" : "B) Begin new game");
            MenuConsole.Print(2, 6, "Q) Quit");

            MenuWindow.Show();
            GameMode.PushEngineMode(EngineMode.MenuOpen, null);
        }

        #region Event Loop
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

        public void QueueInput()
        {
            bool ShouldClear()
            {
                MenuWindow.Show();
                GameMode.PushEngineMode(EngineMode.MenuOpen, () => { });
                return true;
            }
            Messager.ShouldClearQueueOnEscape = ShouldClear;
            Messager.QueueInput(Keyboard.KeysPressed);
            //1.+: Capturing mouse events can wait until post 1.0.
        }

        internal void ActOnMode()
        {
            switch (GameMode.CurrentMode)
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
                GameMode.CurrentCallback();
                Messager.ResetMessagesSentSincePause();
                break;

            //  When enough small messages have been printed that the
            //  UI is waiting with a '- more -' style prompt
            case EngineMode.MessagesPendingUserInput:
                GameMode.CurrentCallback();
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
                    throw new Exception($"Hit the game loop in {GameMode.CurrentMode} mode.");
                }
                break;
            }
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
                        GameMode.PopEngineMode();
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
                        GameMode.PopEngineMode();
                        MenuWindow.Hide();
                        BeginNewGame();
                        return;
                    }

                    //0.0: Load saved game and resume play
                    if (press.Key == Keys.L)
                    {
                        GameMode.PopEngineMode();
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
            while (GameMode.CurrentMode == EngineMode.WorldTurns)
            {
                var nextAction = Schedule.GetNextAction();
                Dispatcher.Dispatch(nextAction);
            }
        }
        #endregion

        public void BeginNewGame()
        {
            if (TopSeed == null)
                TopSeed = GenerateSimpleTopSeed();
            Log.InfoFormat("Beginning new game with Top Seed [{0}]", TopSeed);
            Cosmogenesis(TopSeed, SadConEntityFactory);
            TopSeed = null;

            ServicePanel.Notify_NewGame_Startup(new GameDataEventArgs(Compendium.TomeOfChaos, Compendium.Herbal));

            var loader = new Persist.MapLoader(Log, Engine.Compendium.Atlas);
            FullMap = loader.FarmMap();

            Player = Compendium.SocialRegister.FindBeing("Suvail");
            Player.MoveTo(FullMap.BeingMap);
            Player.MoveTo(FullMap.SpaceMap.PlayerStartPoint);
            Log.Debug("Created player character.");

            Schedule.AddAgent(Player, 12);

            (MapConsole, MapWindow) = UIBuilder.CreateMapWindow(MapWindowSize, "A Farmyard", FullMap);
            Children.Add(MapWindow);
            MapConsole.Children.Add(Player.Console);
            FullMap.SetInitialConsoleCells(MapConsole, FullMap.SpaceMap);
            FullMap.FOV = new FOV(FullMap.GetView_CanSeeThrough());
            FullMap.UpdateFOV(MapConsole, Player.GetPosition());
            MapWindow.Show();

            GameState = new GameState
            {
                Map = FullMap,
                Story = Engine.Compendium.Dramaticon,
            };
            Dispatcher.GameState = GameState;
            Dispatcher.Compendium = Compendium;

            Player.CommandSource = new InputCommandSource(ServicePanel, GameState, Dispatcher);
            MapConsole.CenterViewPortOnPoint(Player.GetPosition());

            GameMode.PushEngineMode(EngineMode.WorldTurns, null);
            GameInProgress = true;
            Log.Info("Began new game");
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

            GameMode.PopEngineMode();
            GameInProgress = false;

            ShutDownGame();
            OpenGameMenu();
        }

        public void WriteGameOverReport(PlayerDiedException pde) { } //0.0: WriteGameOverReport
        public void ClearStats() { } //0.0: ClearStats

        private void ShutDownGame()
        {
            // Start by simply undoing many NewGame steps in reverse order
            Log.Info("Shutting down game");

            Player.CommandSource = null;

            // ?!?? Dispatcher.AttackSystem = null;
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

            Log.Info("Shut down game");
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

                FullMap.UpdateFOV(MapConsole, Player.GetPosition());
                MapConsole.CenterViewPortOnPoint(Player.GetPosition());
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
