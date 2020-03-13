using System;
using System.Diagnostics;
using CopperBend.Logic;
using log4net;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Xna.Framework;

namespace CopperBend.Application
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // populate command-line arguments, then call OnExecute()
            return CommandLineApplication.Execute<Program>(args);
        }

        [Option("-d|--Debug")]
        public bool LaunchDebugger { get; }

        [Option("-t|--Test")]
        public bool TestMode { get; }

        [Option("-s|--seed")]
        public string InitialSeed { get; } = null;

        public void OnExecute()
        {
            if (LaunchDebugger && !Debugger.IsAttached) Debugger.Launch();

            Composer composer = new Composer();
            composer.Compose(InitialSeed, TestMode);

            ILog log = composer.Logger;
            log.Info("\n======================================");
            log.Info("Run started");

            LaunchGame(composer, log);

            log.Info("Run ended");

            composer.Release();
        }

        public void LaunchGame(Composer composer, ILog log)
        {
            Game game;
            try
            {
                game = composer.GetGameInstance();
            }
            catch (Exception ex)
            {
                log.Fatal("Exception terminated construction", ex);
                return;
            }

            try
            {
                game.Run();
            }
            catch (Exception ex)
            {
                log.Fatal("Exception terminated run", ex);
            }
        }
    }
}
