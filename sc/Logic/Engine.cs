using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Size = System.Drawing.Size;
using log4net;
using SadConsole;
using SadConsole.Input;
using SadConsole.Components;
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
        private const int messageLimitBeforePause = 3;  //0.1: artificially low for short-term testing
        private readonly ILog log;

        public Size GameSize { get; set; }
        public Size MapSize { get; set; }
        public Size MapWindowSize { get; set; }
        public Size MenuWindowSize { get; set; }

        private ScrollingConsole MapConsole { get; set; }
        public Window MapWindow { get; set; }
        public IMessageLogWindow MessageWindow { get; set; }
        private Window MenuWindow { get; set; }
        private ControlsConsole MenuConsole { get; set; }

        private Keyboard Kbd { get; }
        public Queue<AsciiKey> InputQueue { get; }
        private Being Player { get; set; }

        private GameState GameState { get; set; }
        private Schedule Schedule { get; set; }
        private CommandDispatcher Dispatcher { get; set; }
        private Describer Describer { get; set; }
        private UIBuilder UIBuilder { get; set; }
        public ISadConEntityFactory SadConEntityFactory { get; private set; }
        private bool GameInProgress { get; set; }
        private int TickWhenGameLastSaved { get; set; }
        private string TopSeed { get; set; }
        private ICompoundMap FullMap { get; set; }
        private readonly bool jumpToDebugger = true;

        #region Init
        public Engine(int gameWidth, int gameHeight, ILog logger, string topSeed = null)
            : base()
        {
            log = logger;
            TopSeed = topSeed;

            IsVisible = true;
            IsFocused = true;

            GameSize = new Size(gameWidth, gameHeight);

            Parent = SadGlobal.CurrentScreen;
            Kbd = SadGlobal.KeyboardState;

            InputQueue = new Queue<AsciiKey>();
            modeStack = new Stack<EngineMode>();
            callbackStack = new Stack<Action>();

            GameSize = new Size(gameWidth, gameHeight);
            MapWindowSize = new Size(GameSize.Width * 2 / 3, GameSize.Height - 8);
            MenuWindowSize = new Size(GameSize.Width - 20, GameSize.Height / 4);

            Init();
        }

        //0.1: Tease apart distinct start-up goals:
        //  Loading a saved game,
        //  Loading debug "saved game" configurations
        /// <summary> Initialize more complex game systems </summary>
        public void Init()
        {
            var mapFontMaster = SadGlobal.LoadFont("Cheepicus_14x14.font");
            UIBuilder = new UIBuilder(GameSize, mapFontMaster, log);

            GameInProgress = false;
            PushEngineMode(EngineMode.NoGameRunning, null);

            UIBuilder = new UIBuilder(GameSize, mapFontMaster, log);

            SadConEntityFactory = new SadConEntityFactory(mapFontMaster);
            Schedule = new Schedule(log);

            MapWindowSize = new Size(GameSize.Width * 1 / 3, GameSize.Height - 8);
            MenuWindowSize = new Size(GameSize.Width - 20, GameSize.Height / 4);

            //0.2.MAP: Put map name in YAML -> CompoundMap -> CreateMapWindow

            (MenuConsole, MenuWindow) = UIBuilder.CreateM2Window(MenuWindowSize, "Game Menu");
            Children.Add(MenuWindow);

            MessageWindow = UIBuilder.CreateMessageLog();
            Children.Add((SadConsole.Console)MessageWindow);
            MessageWindow.Show();

            Describer = new Describer();  // (must be attached to Herbal &c per-game)
            Dispatcher = new CommandDispatcher(Schedule, GameState, Describer, MessageWindow, log)
            {
                PushEngineMode = PushEngineMode,
                PopEngineMode = PopEngineMode,
                IsInputReady = () => InputQueue.Count > 0,
                GetNextInput = InputQueue.Dequeue,
                ClearPendingInput = InputQueue.Clear,
                WriteLine = AddMessage,
                Prompt = MessageWindow.Prompt,
                More = this.PromptUserForMoreAndPend,
            };

            OpenGameMenu();
        }

        public void OpenGameMenu()
        {
            Guard.Against(CurrentMode == EngineMode.MenuOpen);

            MenuConsole.Clear();
            MenuConsole.Print(2, 4, GameInProgress ? "R) Return to game" : "B) Begin new game");
            MenuConsole.Print(2, 6, "Q) Quit");

            MenuWindow.Show();
            PushEngineMode(EngineMode.MenuOpen, null);
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

            Player = CreatePlayer(FullMap.SpaceMap.PlayerStartPoint);
            Schedule.AddAgent(Player, 12);

            (MapConsole, MapWindow) = UIBuilder.CreateMapWindow(MapWindowSize, "A Farmyard", FullMap);
            Children.Add(MapWindow);
            MapConsole.Children.Add(Player.Console);
            FullMap.SetInitialConsoleCells(MapConsole, FullMap.SpaceMap);
            FullMap.FOV = new FOV(FullMap.GetView_CanSeeThrough());
            FullMap.UpdateFOV(MapConsole, Player.Position);
            MapWindow.Show();
            messageQueue = new Queue<string>();

            GameState = new GameState
            {
                Player = Player,
                Map = FullMap,
                Story = Engine.Compendium.Dramaticon,
            };
            Dispatcher.GameState = GameState;
            Dispatcher.AttackSystem.RotMap = GameState.Map.RotMap;

            Player.CommandSource = new InputCommandSource(Describer, GameState, Dispatcher, log);
            MapConsole.CenterViewPortOnPoint(Player.Position);

            PushEngineMode(EngineMode.WorldTurns, null);
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

            PopEngineMode();
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

        private Being CreatePlayer(Coord playerLocation)
        {
            var player = BeingCreator.CreateBeing("player");
            player.AddComponent(new EntityViewSyncComponent());
            player.Position = playerLocation;
            player.Console.Position = playerLocation;

            //0.2: remove these pre-equipped items
            player.AddToInventory(Equipper.BuildItem("hoe"));
            player.AddToInventory(Equipper.BuildItem("seed:Healer", 2));

            log.Debug("Created player.");
            return player;
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

        #region Handle input
        public void QueueInput()
        {
            //0.K
            foreach (var key in Kbd.KeysPressed)
            {
                // Escape key processing skips the normal input queue,
                // to make 'Quit Game' as reliably available as possible.
                if (key == Keys.Escape && CurrentMode != EngineMode.MenuOpen)
                {
                    InputQueue.Clear();
                    MenuWindow.Show();
                    PushEngineMode(EngineMode.MenuOpen, () => { });
                    return;
                }

                InputQueue.Enqueue(key);
            }

            //1.+: Capturing mouse events can wait until post 1.0.
        }

        private AsciiKey GetNextKeyPress()
        {
            if (InputQueue.Count == 0) return new AsciiKey { Key = Keys.None };
            return InputQueue.Dequeue();
        }
        #endregion

        #region Handle Game Modes
        #region Mode Mechanisms
        //  We can stack modes of the game to any level.
        //  Say the schedule reaches the player, who enters a command
        //  to inspect their quest/job list, the stack is:
        //    Large, Input, Schedule, Start
        //  Each mode on the stack has its own callback, so if we then
        //  look into details of a quest, the states could be:
        //    Large, Large, Input, Schedule, Start
        //  ...and we can later leave the quest details without confusion
        //  about what we're doing.

        private readonly Stack<EngineMode> modeStack;
        private readonly Stack<Action> callbackStack;

        internal EngineMode CurrentMode { get; set; } = EngineMode.Unknown;

        internal Action CurrentCallback { get; set; } = () => { };

        internal void PushEngineMode(EngineMode newMode, Action callback)
        {
            if (newMode == EngineMode.Unknown)
                throw new Exception($"Should never EnterMode({newMode}).");

            var oldMode = CurrentMode;
            modeStack.Push(CurrentMode);
            CurrentMode = newMode;
            callbackStack.Push(CurrentCallback);
            CurrentCallback = callback;

            // fires when restarting game 12 nov 19
            //if (oldMode == CurrentMode)
            //    if (!Debugger.IsAttached) Debugger.Launch();

            // Don't log mode shifts from world's turn to player's turn.
            if (oldMode == EngineMode.WorldTurns && CurrentMode == EngineMode.PlayerTurn) return;

            log.Debug($"Enter mode {CurrentMode}, push down mode {oldMode}.");
        }

        internal void PopEngineMode()
        {
            var oldMode = CurrentMode;
            CurrentMode = modeStack.Pop();
            CurrentCallback = callbackStack.Pop();

            // Don't log mode shifts from player's turn to world's turn.
            if (oldMode == EngineMode.PlayerTurn && CurrentMode == EngineMode.WorldTurns) return;

            log.Debug($"Pop mode {oldMode} off, enter mode {CurrentMode}.");
        }
        #endregion

        internal void ActOnMode()
        {
            switch (CurrentMode)
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
                CurrentCallback();
                ResetMessagesSentSincePause();
                break;

            //  When enough small messages have been printed that the
            //  UI is waiting with a '- more -' style prompt
            case EngineMode.MessagesPendingUserInput:
                CurrentCallback();
                break;

            //  The large message pane (inventory, log, ...) overlays most of the game
            case EngineMode.LargeMessagePending:
                HandleLargeMessage();
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
                    throw new Exception($"Hit the game loop in {CurrentMode} mode.");
                }
                break;
            }
        }

        private void HandleGameMenu()
        {
            for (AsciiKey press = GetNextKeyPress(); press.Key != Keys.None; press = GetNextKeyPress())
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
                        PopEngineMode();
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
                        PopEngineMode();
                        MenuWindow.Hide();
                        BeginNewGame();
                        return;
                    }

                    //0.0: Load saved game and resume play
                    if (press.Key == Keys.L)
                    {
                        PopEngineMode();
                        MenuWindow.Hide();
                        //PickAndResumeSavedGame();
                        TickWhenGameLastSaved = Schedule.CurrentTick;
                        return;
                    }

                    //0.0: Generate new seed, or enter new seed
                }
            }
        }

        #endregion

        public void HandleScheduledEvents()
        {
            while (CurrentMode == EngineMode.WorldTurns)
            {
                var nextAction = Schedule.GetNextAction();
                Dispatcher.Dispatch(nextAction);
            }
        }

        #region Short Message log
        private Queue<string> messageQueue;
        private int messagesSentSincePause = 0;

        public void AddMessage(string newMessage)
        {
            messageQueue.Enqueue(newMessage);
            ShowMessages();
        }

        //0.1: ResetMessagesSentSincePause() needs to be called all through the ICS.
        public void ResetMessagesSentSincePause() => messagesSentSincePause = 0;

        private void ShowMessages()
        {
            //0.2: There are probably other modes where we want to suspend messages.
            while (CurrentMode != EngineMode.MessagesPendingUserInput && messageQueue.Any())
            {
                if (messagesSentSincePause >= messageLimitBeforePause)
                {
                    PromptUserForMoreAndPend();
                    return;
                }

                var nextMessage = messageQueue.Dequeue();
                MessageWindow.WriteLine(nextMessage);
                messagesSentSincePause++;
            }
        }

        private void PromptUserForMoreAndPend()
        {
            MessageWindow.WriteLine("-- more --");
            PushEngineMode(EngineMode.MessagesPendingUserInput, HandleMessagesPending);
        }

        private void HandleMessagesPending()
        {
            for (AsciiKey k = GetNextKeyPress(); k.Key != Keys.None; k = GetNextKeyPress())
            {
                if (k.Key == Keys.Space)
                {
                    ResetMessagesSentSincePause();
                    PopEngineMode();
                    ShowMessages();  // ...which may have enough messages to pend us again.
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
                .Intersect(FullMap.FOV.CurrentFOV);
            FullMap.UpdateViewOfCoords(MapConsole, changedAndInFOV);
            FullMap.CoordsWithChanges.Clear();
        }

        private void AnimateBackground(TimeSpan timeElapsed)
        {
            FullMap.EffectsManager.UpdateEffects(timeElapsed.TotalSeconds);
        }

        #region LargeMessages, currently empty
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
