using log4net;
using log4net.Config;
using System;
using System.IO;
using Game = SadConsole.Game;

namespace CbRework
{
    public static class Program
    {
        static void Main()
        {
            ILog log;
            var repo = LogManager.CreateRepository("CB");
            XmlConfigurator.Configure(repo, new FileInfo("sc.log.config"));
            log = LogManager.GetLogger("CB", "CB");
            log.Info("\n======================================");
            log.Info("Run started");

            int windowWidth = 80;
            int windowHeight = 40;

            try
            {
                Game.Create(windowWidth, windowHeight);

                //  Engine is now a console, which cannot be created before .Run() below.
                Game.OnInitialize = () => new Engine(windowWidth, windowHeight);
                Game.Instance.Run();
            }
            catch (Exception ex)
            {
                log.Fatal("Exception terminated app", ex);
            }
            log.Info("Run ended");
            Game.Instance.Dispose();
        }
    }
}
