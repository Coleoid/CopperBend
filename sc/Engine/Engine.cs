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
using System.Diagnostics;

namespace CopperBend.Engine
{
    public partial class Engine : ContainerConsole
    {
        private readonly ILog log;

        public Size GameSize;
        public Size MapSize;
        public Size MapWindowSize;
        public Size MenuWindowSize;

        private ScrollingConsole MapConsole { get; set; }
        public Window MapWindow;
        public MessageLogWindow MessageWindow;
        private Window MenuWindow;
        private ControlsConsole MenuConsole;
        
        private readonly Keyboard Kbd;
        public Queue<AsciiKey> InputQueue;
        private Being Player;

        private GameState GameState;
        private Schedule Schedule;
        private CommandDispatcher Dispatcher;
        private Describer Describer;
        private UIBuilder UIBuilder;

        private bool GameInProgress;
        private int TickWhenGameLastSaved;
        private string TopSeed;
        private readonly bool JumpToDebugger = true;

        #region Init
        public Engine(int gameWidth, int gameHeight, ILog logger, string topSeed = null)
            : base()
        {
            log = logger;

            if (topSeed == null) topSeed = GenerateSimpleTopSeed();
            log.Info($"Initial top seed:  {topSeed}");
            TopSeed = topSeed;

            IsVisible = true;
            IsFocused = true;

            Parent = SadConState.CurrentScreen;
            Kbd = SadConState.KeyboardState;

            InputQueue = new Queue<AsciiKey>();
            ModeStack = new Stack<EngineMode>();
            CallbackStack = new Stack<Action>();

            GameSize = new Size(gameWidth, gameHeight);
            MapWindowSize = new Size(GameSize.Width * 2 / 3, GameSize.Height - 8);
            MenuWindowSize = new Size(GameSize.Width - 20, GameSize.Height / 4);

            //  This may leave the constructor brittle
            Init();
        }

        //0.1: Tease apart distinct start-up goals:
        //  Loading a saved game,
        //  Debug stuff?
        private ICompoundMap FullMap;
        public void Init()
        {
            UIBuilder = new UIBuilder(GameSize, null, log); //font

            GameInProgress = false;
            PushEngineMode(EngineMode.NoGameRunning, null);

            Being.EntityFactory = new EntityFactory();
            Schedule = new Schedule(log);

            //0.2.MAP: Put map name in YAML -> CompoundMap -> CreateMapWindow

            (MenuConsole, MenuWindow) = UIBuilder.CreateM2Window(MenuWindowSize, "Game Menu");
            Children.Add(MenuWindow);

            MessageWindow = UIBuilder.CreateMessageLog();
            Children.Add(MessageWindow);
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
            TopSeed = GenerateSimpleTopSeed();
            log.InfoFormat("Beginning new game with Top Seed [{0}]", TopSeed);
            Cosmogenesis(TopSeed);
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
            MessageQueue = new Queue<string>();

            //0.2.GFX: Set a non-square font in message areas
            //var fontMaster = SadConsole.Global.LoadFont("terminal16x16_gs_ro.font");
            //var font = fontMaster.GetFont(SadConsole.Font.FontSizes.One);
            //SadConsole.Global.FontDefault = font;

            GameState = new GameState
            {
                Player = Player,
                Map = FullMap,
                Story = Engine.Compendium.Dramaticon,
            };
            Dispatcher.GameState = GameState;
            Dispatcher.AttackSystem.BlightMap = GameState.Map.BlightMap;

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

            Dispatcher.AttackSystem.BlightMap = null;
            Dispatcher.GameState = null;

            GameState = null;

            FullMap.FOV = null;

            MapConsole.Children.Remove(Player.Console);
            Children.Remove(MapWindow);
            MapConsole = null;
            MapWindow = null;

            //Schedule.AddAgent(Player, 12);
            Schedule.Clear();

            //Player = CreatePlayer(FullMap.SpaceMap.PlayerStartPoint);
            Player = null;

            //FullMap = loader.FarmMap();
            FullMap = null;

            //var loader = new Persist.MapLoader();
            //Describer.Scramble();
            //Cosmogenesis(TopSeed);

            //----
            log.Info("Shut down game");
        }

        private Being CreatePlayer(Coord playerLocation)
        {
            var player = new Player(Color.AntiqueWhite, Color.Transparent, '@')
            {
                Name = "Suvail",
                Position = playerLocation
            };
            player.AddComponent(new EntityViewSyncComponent());
            player.AddToInventory(Equipper.BuildItem("hoe"));
            player.AddToInventory(Equipper.BuildItem("seed:Healer", 2));

            log.Debug("Created player.");
            player.Console.Position = playerLocation;
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

        private readonly Stack<EngineMode> ModeStack;
        private readonly Stack<Action> CallbackStack;

        internal EngineMode CurrentMode { get; set; } = EngineMode.Unknown;

        internal Action CurrentCallback { get; set; } = () => { };

        internal void PushEngineMode(EngineMode newMode, Action callback)
        {
            if (newMode == EngineMode.Unknown)
                throw new Exception($"Should never EnterMode({newMode}).");

            var oldMode = CurrentMode;
            ModeStack.Push(CurrentMode);
            CurrentMode = newMode;
            CallbackStack.Push(CurrentCallback);
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
            CurrentMode = ModeStack.Pop();
            CurrentCallback = CallbackStack.Pop();

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
                if (JumpToDebugger) // compile to false for production builds
                {
                    if (!Debugger.IsAttached) Debugger.Launch();
                    else Debugger.Break();
                }
                else throw new Exception($"Hit the game loop in {CurrentMode} mode.");
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
        private Queue<string> MessageQueue;
        private int MessagesSentSincePause = 0;
        private int MessageLimitBeforePause = 3;  //0.1: artificially low for short-term testing

        public void AddMessage(string newMessage)
        {
            MessageQueue.Enqueue(newMessage);
            ShowMessages();
        }

        //0.1: ResetMessagesSentSincePause() needs to be called all through the ICS.
        public void ResetMessagesSentSincePause() => MessagesSentSincePause = 0;

        private void ShowMessages()
        {
            //0.2: There are probably other modes where we want to suspend messages.
            while (CurrentMode != EngineMode.MessagesPendingUserInput && MessageQueue.Any())
            {
                if (MessagesSentSincePause >= MessageLimitBeforePause)
                {
                    PromptUserForMoreAndPend();
                    return;
                }

                var nextMessage = MessageQueue.Dequeue();
                MessageWindow.WriteLine(nextMessage);
                MessagesSentSincePause++;
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
