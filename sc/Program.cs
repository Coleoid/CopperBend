﻿using System;
using System.IO;
using log4net;
using log4net.Config;
using Game = SadConsole.Game;

namespace CopperBend.Application
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

            int gameWidth = 80;
            int gameHeight = 60;

            try
            {
                Game.Create("Cheepicus_14x14.font", gameWidth, gameHeight);

                //  Engine is now a console, which cannot be created before .Run() below.
                //  .OnInitialize must be set before .Run is called.
                Game.OnInitialize = () => new Engine.Engine(gameWidth, gameHeight);
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
    }
}
