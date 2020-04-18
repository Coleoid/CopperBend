﻿using System.Diagnostics;
using log4net;
using McMaster.Extensions.CommandLineUtils;
using CopperBend.Logic;

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

            try
            {
                composer.LaunchGame();
            }
            finally
            {
                log.Info("Run ended");
                composer.Release();
            }
        }
    }
}
