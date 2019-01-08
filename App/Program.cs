using CopperBend.App.Model;
using CopperBend.MapUtil;
using RLNET;
using log4net;
using System;

namespace CopperBend.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = LogManager.GetLogger("CB");
            log.Info("Run started");
            try
            {
                var game = new GameEngine();
                game.Run();
            }
            catch (Exception ex)
            {
                log.Fatal("Bah.", ex);
            }
            log.Info("Run ended");
        }
    }
}
