using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using log4net;
using log4net.Config;
//using Autofac;
using Game = SadConsole.Game;
using XNAGame = Microsoft.Xna.Framework.Game;
using SadGlobal = SadConsole.Global;
using CopperBend.Fabric;
using CopperBend.Contract;
using CopperBend.Model;
using GoRogue;

namespace CopperBend.Logic
{
    public class Composer
    {
        private string InitialSeed { get; set; }
        public void Compose(string seed, bool testMode)
        {
            InitialSeed = seed;

            //var builder = new ContainerBuilder();
            //builder.RegisterInstance(Logger).As<ILog>();
            //builder.RegisterType<Compendium>();
            //builder.RegisterInstance(new IDGenerator()).As<IDGenerator>();


            var repo = LogManager.CreateRepository("CB");
            XmlConfigurator.Configure(repo, new FileInfo("sc.log.config"));
            Logger = LogManager.GetLogger("CB", "CB");

        }
        public ILog Logger { get; private set; }

        //private ILog logger;
        //public ILog Logger
        //{
        //    get
        //    {
        //        if (logger == null)
        //        {
        //            var repo = LogManager.CreateRepository("CB");
        //            XmlConfigurator.Configure(repo, new FileInfo("sc.log.config"));
        //            logger = LogManager.GetLogger("CB", "CB");
        //        }
        //        return logger;
        //    }
        //}
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

            //0.0: Only enough to hoist the dependency, so far.
            Action<string> writeLine = (s) => { };
            IControlPanel dispatcher = new CommandDispatcher(sched, gameState, describer, writeLine, Logger);

            Engine engine = new Engine(
                Logger, sched,
                scefactory, kbd,
                szGame, uib, describer, gameState, dispatcher
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
