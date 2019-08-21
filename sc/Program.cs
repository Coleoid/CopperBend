using System;
using System.IO;
using log4net;
using log4net.Config;
using McMaster.Extensions.CommandLineUtils;
using NUnit.Engine;
using Game = SadConsole.Game;

namespace CopperBend.Application
{
    public class Program
    {
        [Option("-t|--Test")]
        public bool TestMode { get; }

        // main() should boil down to:
        //public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        public static int Main(string[] args)
        {
            return CommandLineApplication.Execute<Program>(args);
            //var app = new CommandLineApplication();

            //app.HelpOption();
            //return app.Execute(args);
        }

        public void OnExecute()
        {
            RunGame();
        }

        public void RunGame()
        {
            ILog log;
            var repo = LogManager.CreateRepository("CB");
            XmlConfigurator.Configure(repo, new FileInfo("sc.log.config"));
            log = LogManager.GetLogger("CB", "CB");
            log.Info("\n======================================");
            log.Info("Run started");

            int gameWidth = 80;
            int gameHeight = 60;

            try
            {
                //Game.Create("Zaratustra.font", gameWidth, gameHeight);
                Game.Create(gameWidth, gameHeight);

                //  Engine is now a console, which cannot be created before .Run() below.
                //  .OnInitialize must be set before .Run is called.
                Game.OnInitialize = () => new Engine.Engine(gameWidth, gameHeight, TestMode);
                Game.Instance.Window.Title = "Copper Bend";
                Game.Instance.Run();
            }
            catch (Exception ex)
            {
                log.Fatal("Exception terminated app", ex);
            }
            log.Info("Run ended");
            Game.Instance?.Dispose();
        }

            //static void Main()
            //{
            //    ILog log;
            //    var repo = LogManager.CreateRepository("CB");
            //    XmlConfigurator.Configure(repo, new FileInfo("sc.log.config"));
            //    log = LogManager.GetLogger("CB", "CB");
            //    log.Info("\n======================================");
            //    log.Info("Run started");

            //    int gameWidth = 80;
            //    int gameHeight = 60;

            //    try
            //    {
            //        Game.Create("Zaratustra.font", gameWidth, gameHeight);

            //        //  Engine is now a console, which cannot be created before .Run() below.
            //        //  .OnInitialize must be set before .Run is called.
            //        Game.OnInitialize = () => new Engine.Engine(gameWidth, gameHeight);
            //        Game.Instance.Window.Title = "Copper Bend";
            //        Game.Instance.Run();
            //    }
            //    catch (Exception ex)
            //    {
            //        log.Fatal("Exception terminated app", ex);
            //    }
            //    log.Info("Run ended");
            //    Game.Instance?.Dispose();
            //}
        }
    }
