using DryIoc;
using log4net;
using System;
using RLNET;
using System.Collections.Generic;

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
                var c = new Container();
                c.Register<EventBus, EventBus>(Reuse.Singleton);
                c.Register<Describer, Describer>(Reuse.Singleton);
                c.Register<Scheduler, Scheduler>(Reuse.Singleton);
                c.Register<GameWindow, GameWindow>(Reuse.Singleton);
                c.Register<Queue<GameCommand>, Queue<GameCommand>>(Reuse.Singleton, Made.Of(() => new Queue<GameCommand>()));
                c.Register<Queue<RLKeyPress>, Queue<RLKeyPress>>(Reuse.Singleton, Made.Of(() => new Queue<RLKeyPress>()));
                c.Register<MapLoader, MapLoader>(Reuse.Singleton);
                c.Register<IGameState, GameState>(Reuse.Singleton);
                c.Register<CommandDispatcher, CommandDispatcher>(Reuse.Singleton);

                c.Register<GameEngine, GameEngine>(Reuse.Singleton);

                var game = c.Resolve<GameEngine>();
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
