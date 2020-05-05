using System;
using System.IO;
using System.Drawing;
using log4net;
using log4net.Config;
using Game = SadConsole.Game;
using XNAGame = Microsoft.Xna.Framework.Game;
using SadGlobal = SadConsole.Global;
using CopperBend.Fabric;
using CopperBend.Contract;

namespace CopperBend.Logic
{
    public class Composer
    {
        private string InitialSeed { get; set; }
        public void Compose(string seed, bool testMode)
        {
            InitialSeed = seed;

            var repo = LogManager.CreateRepository("CB");
            XmlConfigurator.Configure(repo, new FileInfo("sc.log.config"));
            Logger = LogManager.GetLogger("CB", "CB");
        }

        public ILog Logger { get; private set; }

        private int gameWidth;
        private int gameHeight;

        public XNAGame CreateGameInstance()
        {
            gameWidth = 160;
            gameHeight = 60;

            Game.Create(gameWidth, gameHeight);

            // 0.N:  Engine is a SadConsole.ContainerConsole
            // Consider breaking this inheritance constraint.
            Game.OnInitialize = InitializeEngine;
            Game.Instance.Window.Title = "Copper Bend";

            return Game.Instance;
        }

        public void InitializeEngine()
        {
            var sched = new Schedule(Logger);
            var fmMap = SadGlobal.LoadFont("Cheepicus_14x14.font");
            var scefactory = new SadConEntityFactory(fmMap);
            var kbd = SadGlobal.KeyboardState;

            var gameSize = new Size(gameWidth, gameHeight);
            var uibuilder = new UIBuilder(gameSize, fmMap, Logger);
            var gameState = new GameState();

            var describer = new Describer();
            var node = new ModeNode(Logger);
            var msgr = new Messager(node);
            IServicePanel servicePanel = new ServicePanel()
                .Register(Logger)
                .Register(sched)
                .Register(describer)
                .Register(msgr)
                .Register(node);

            IControlPanel dispatcher = new CommandDispatcher(servicePanel, gameState);

            Engine engine = new Engine(
                servicePanel,
                scefactory, kbd,
                gameSize, uibuilder, gameState, dispatcher
            );

            engine.Init(InitialSeed);

            // this, next?
            //dispatcher.Init(engine, attackSystem);
        }


        public void LaunchGame()
        {
            XNAGame game;
            try
            {
                game = CreateGameInstance();
            }
            catch (Exception ex)
            {
                Logger.Fatal("Exception terminated construction", ex);
                return;
            }

            try
            {
                game.Run();
            }
            catch (Exception ex)
            {
                Logger.Fatal("Exception terminated run", ex);
            }
        }


        public void Release()
        {
            Game.Instance?.Dispose();
        }
    }
}
