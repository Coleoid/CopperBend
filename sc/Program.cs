﻿using System;
using System.Diagnostics;
using System.IO;
using CopperBend.Logic;
using log4net;
using log4net.Config;
using McMaster.Extensions.CommandLineUtils;
using Game = SadConsole.Game;

namespace CopperBend.Application
{
    public class Program
    {
        [Option("-d|--Debug")]
        public bool LaunchDebugger { get; }

        [Option("-t|--Test")]
        public bool TestMode { get; }

        [Option("-s|--seed")]
        public string InitialSeed { get; } = null;

#pragma warning disable IDE0052 // Remove unread private members
        //  Engine Engine engine.  Here to satisfy a warning, it generates a new one.  C'est la vie.
        private Engine engine;
#pragma warning restore IDE0052 // Remove unread private members

        public static int Main(string[] args)
        {
            // populate command-line arguments, then call OnExecute()
            return CommandLineApplication.Execute<Program>(args);
        }

        public void OnExecute()
        {
            if (LaunchDebugger && !Debugger.IsAttached) Debugger.Launch();

            ILog log;
            var repo = LogManager.CreateRepository("CB");
            XmlConfigurator.Configure(repo, new FileInfo("sc.log.config"));
            log = LogManager.GetLogger("CB", "CB");
            log.Info("\n======================================");
            log.Info("Run started");

            int gameWidth = 160;
            int gameHeight = 60;

            try
            {
                //Game.Create("Cheepicus_14x14.font", gameWidth, gameHeight);
                Game.Create(gameWidth, gameHeight);

                //  Engine is now a console, which cannot be created before .Run() below.
                //  .OnInitialize must be set before .Run is called.
                Game.OnInitialize = () => engine = new Engine(gameWidth, gameHeight, log, InitialSeed);
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
