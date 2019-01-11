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
                var c = new Container(rules => rules.WithDefaultReuse(Reuse.InCurrentScope));

                c.Register<EventBus, EventBus>();
                c.Register<Describer, Describer>();
                c.Register<Schedule, Schedule>();
                c.Register<GameWindow, GameWindow>();
                c.Register<Queue<GameCommand>, Queue<GameCommand>>(Made.Of(() => new Queue<GameCommand>()));
                c.Register<Queue<RLKeyPress>, Queue<RLKeyPress>>(Made.Of(() => new Queue<RLKeyPress>()));
                c.Register<MapLoader, MapLoader>();
                c.Register<IGameState, GameState>();
                c.Register<CommandDispatcher, CommandDispatcher>();

                c.Register<GameEngine, GameEngine>();

                using (var scope = c.OpenScope())
                {
                    var game = scope.Resolve<GameEngine>();
                    game.Run();
                }
            }
            catch (Exception ex)
            {
                log.Fatal("Exception terminated app", ex);
            }
            log.Info("Run ended");
        }
    }
}
