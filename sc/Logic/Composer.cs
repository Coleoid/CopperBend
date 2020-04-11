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

        public XNAGame GetGameInstance()
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
            Schedule sched = new Schedule(Logger);
            var fmMap = SadGlobal.LoadFont("Cheepicus_14x14.font");
            var scefactory = new SadConEntityFactory(fmMap);
            var kbd = SadGlobal.KeyboardState;

            var szGame = new Size(gameWidth, gameHeight);
            var uib = new UIBuilder(szGame, fmMap, Logger);
            var describer = new Describer();  // (must be attached to Herbal &c per-game)
            var gameState = new GameState();
            var node = new ModeNode(Logger);
            var msgr = new Messager(node);

            IControlPanel dispatcher = new CommandDispatcher(
                Logger, sched,
                gameState, describer,
                msgr
            );

            Engine engine = new Engine(
                Logger, sched,
                scefactory, kbd,
                szGame, uib, describer, gameState, dispatcher,
                node, msgr
            );

            engine.Init(InitialSeed);

            // this, next?
            //dispatcher.Init(engine, attackSystem);
        }

        public void Release()
        {
            Game.Instance?.Dispose();
        }
    }
}
