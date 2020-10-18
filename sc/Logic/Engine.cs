using System;
using System.Diagnostics;
using System.IO;
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
using CopperBend.Creation;

namespace CopperBend.Logic
{
    /// <summary> Main game mechanisms. </summary>
    public partial class Engine
    {
        [InjectProperty] private ILog Log { get; set; }
        [InjectProperty] private ISadConEntityFactory SadConEntityFactory { get; set; }
        [InjectProperty] private Keyboard Keyboard { get; set; }
        [InjectProperty] private IGameState GameState { get; set; }
        [InjectProperty] private IControlPanel ControlPanel { get; set; }
        [InjectProperty] private ITriggerPuller Puller { get; set; }

        [InjectProperty] private IDescriber Describer { get; set; }
        [InjectProperty] private IMessager Messager { get; set; }
        [InjectProperty] private IGameMode GameMode { get; set; }
        [InjectProperty] private ISchedule Schedule { get; set; }
        [InjectProperty] private MapLoader MapLoader { get; set; }
        [InjectProperty] private SocialRegister SocialRegister { get; set; }
        private TomeOfChaos TomeOfChaos { get; set; }

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


        private IUIBuilder UIBuilder { get; set; }
        private TopConsole Top { get; set; }


        #region Init
        public Engine(IUIBuilder uiBuilder)
        {
            UIBuilder = uiBuilder;
            GameSize = uiBuilder.GameSize;
        }

        //0.1: Tease apart distinct start-up goals:
        //  Loading a saved game,
        //  Loading debug "saved game" configurations
        /// <summary> Initialize more complex game systems </summary>
        public void Init(string topSeed = null)
        {
            TopSeed = topSeed;

            GameInProgress = false;
            GameMode.PushEngineMode(EngineMode.NoGameRunning, null);

            MapWindowSize = new Size(GameSize.Width * 2 / 3, GameSize.Height - 8);
            MenuWindowSize = new Size(GameSize.Width - 20, GameSize.Height / 4);

            //0.2.MAP: Put map name in YAML -> CompoundMap -> CreateMapWindow

            Top = new TopConsole();
            Top.EngineUpdate = Update;
            Top.Parent = SadGlobal.CurrentScreen;
            Top.IsVisible = true;
            Top.IsFocused = true;

            (MenuConsole, MenuWindow) = UIBuilder.CreateM2Window(MenuWindowSize, "Game Menu");
            Top.Children.Add(MenuWindow);

            MessageWindow = UIBuilder.CreateMessageLog();
            Messager.MessageWindow = MessageWindow;
            Top.Children.Add((SadConsole.Console)MessageWindow);
            MessageWindow.Show();

            OpenGameMenu();
        }
        #endregion

        public void OpenGameMenu()
        {
            Guard.Against(GameMode.CurrentMode == EngineMode.MenuOpen);

            MenuConsole.Clear();
            MenuConsole.Print(2,  4, GameInProgress ? "R) Return to game" : "B) Begin new game");
            MenuConsole.Print(2,  6, "S) Save game");
            MenuConsole.Print(2,  8, "L) Load game");
            MenuConsole.Print(2, 10, "Q) Quit");

            MenuWindow.Show();
            GameMode.PushEngineMode(EngineMode.MenuOpen, null);
        }

        #region Event Loop
        public void Update(TimeSpan timeElapsed)
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
            case EngineMode.Unset:
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
                string readable = Readable(press);
                Log.Info($"At menu, pressed [{readable}]");

                // Quit
                if (press.Key == Keys.Q)
                {
                    if (ConfirmExit())
                    {
                        Game.Instance.Exit();
                    }
                }

                //0.0: Load saved game and resume play
                if (press.Key == Keys.L)
                {
                    GameMode.PopEngineMode();
                    MenuWindow.Hide();
                    LoadSavedGame();

                    // 1.+.SAVE: Hardcore/roguelike load game mode:
                    // delete save file

                    return;
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

                    //0.0: Save current game
                    if (press.Key == Keys.S)
                    {
                        SaveTheGame();

                        // 1.+.SAVE: Hardcore/roguelike save game mode:
                        // GameInProgress = false;
                        // UnloadGane();
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

                    //0.0: Generate new seed, or enter new seed
                }
            }
        }

        public string Readable(AsciiKey press)
        {
            if (32 <= press.Character && press.Character <= 126)
                return press.Character.ToString();

            if (1 <= press.Character && press.Character <= 26)
                return $"Ctrl-{(char)(press.Character + 64)}";

            return press.Key switch
            {
                Keys.Escape => "Esc",
                _ => "[NP]"
            };
        }

        private bool ConfirmExit()
        {
            bool okay = !GameInProgress || Schedule.CurrentTick == TickWhenGameLastSaved;
            if (!okay)
            {
                //0.0: verify with player "Quit with unsaved changes?"
                //okay = YesNoDialog("Quit with unsaved changes?");
                okay = true;  // until I make the above happen
            }

            return okay;
        }

        private void SaveTheGame()
        {
            string fileName = "cb_1.save";
            Log.Info($"Saving game file at {Path.Combine(Directory.GetCurrentDirectory(), fileName)}");

            string saveYaml = MapLoader.YAMLFromMap(GameState.Map);
            File.WriteAllText(fileName, saveYaml);

            TickWhenGameLastSaved = Schedule.CurrentTick;
        }

        private void LoadSavedGame()
        {
            string fileName = "cb_1.save";
            Log.Info($"Loading game file at {Path.Combine(Directory.GetCurrentDirectory(), fileName)}");

            string loadYaml = File.ReadAllText(fileName);
            ((GameState)GameState).Map = MapLoader.MapFromYAML(loadYaml);

            TickWhenGameLastSaved = Schedule.CurrentTick;
        }

        public void HandleScheduledEvents()
        {
            while (GameMode.CurrentMode == EngineMode.WorldTurns)
            {
                var nextAction = Schedule.GetNextAction();
                ControlPanel.Dispatch(nextAction);
            }
        }
        #endregion

        public void BeginNewGame()
        {
            if (TopSeed == null)
                TopSeed = GenerateSimpleTopSeed();
            Log.InfoFormat("Beginning new game with Top Seed [{0}]", TopSeed);
            TomeOfChaos = new TomeOfChaos(TopSeed);
            //TomeOfChaos.SetTopSeed(TopSeed);
            TopSeed = null;

            // ServicePanel.Notify_NewGame_Startup(new GameDataEventArgs(Compendium.TomeOfChaos, Compendium.Herbal));

            FullMap = MapLoader.FarmMap();
            GameState.Map = FullMap;

            Player = SocialRegister.CreatePlayer();
            SocialRegister.LoadRegister(Player);
            Log.Debug("Created player character.");
            GameState.Player = Player;

            Puller.AddTriggerHolderToScope(FullMap);

            Player.MoveTo(FullMap.BeingMap);
            //Player.MoveTo(FullMap.SpaceMap.PlayerStartPoint);

            Puller.Check(TriggerCategories.MapChanged);

            Schedule.AddAgent(Player, 12);

            (MapConsole, MapWindow) = UIBuilder.CreateMapWindow(MapWindowSize, "A Farmyard", FullMap);
            Top.Children.Add(MapWindow);
            //INPROG: Somewhere near here, pull triggers for MapEnter

            MapConsole.Children.Add(Player.Console);
            FullMap.SetInitialConsoleCells(MapConsole, FullMap.SpaceMap);
            FullMap.FOV = new FOV(FullMap.GetView_CanSeeThrough());
            FullMap.UpdateFOV(MapConsole, Player.GetPosition());
            MapWindow.Show();

            GameState = new GameState
            {
                Map = FullMap,
            };
            ControlPanel.GameState = GameState;

            Player.StrategyStyle = StrategyStyle.UserInput;
            MapConsole.CenterViewPortOnPoint(Player.GetPosition());

            GameMode.PushEngineMode(EngineMode.WorldTurns, null);
            GameInProgress = true;
            Log.Info("Began new game");
        }

        /// <summary> I hope you had fun! </summary>
        public void GameOver(IBeing player, PlayerDiedException pde)
        {
            //TODO: Message on player death
            //Dispatcher.WriteLine("Shortly after you die, the town of Copper Bend is");
            //Dispatcher.WriteLine("overrun by the Rot.  All creatures flee, die,");
            //Dispatcher.WriteLine("or suffer brief quasi-lives when the Rot");
            //Dispatcher.WriteLine("catches and animates them, to extend its spread.");
            //Dispatcher.WriteLine("What will stand in its way?");
            // Losing is Fun?

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

            //Player.Strategy = null;

            // ?!?? Dispatcher.AttackSystem = null;
            ControlPanel.GameState = null;

            GameState = null;

            FullMap.FOV = null;

            MapConsole.Children.Remove(Player.Console);
            Top.Children.Remove(MapWindow);
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
                ControlPanel.PlayerMoved = true;

                FullMap.VisibilityChanged = false;
            }

            if (ControlPanel.PlayerMoved)
            {
                //TODO:  Events at locations on map:  CheckActorAtCoordEvent(actor, tile);

                FullMap.UpdateFOV(MapConsole, Player.GetPosition());
                MapConsole.CenterViewPortOnPoint(Player.GetPosition());
                Puller.Check(TriggerCategories.PlayerLineOfSight);

                ControlPanel.PlayerMoved = false;
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
