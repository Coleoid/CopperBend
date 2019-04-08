using DryIoc;
using log4net;
using System;
using RLNET;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using SadConsole;
//using Console = SadConsole.Console;

namespace CopperBend.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = LogManager.GetLogger("CB");
            log.Info("\n======================================");
            log.Info("Run started");

            int Width = 80;
            int Height = 25;

            try
            {
                SadConsole.Game.Create(Width, Height);
                SadConsole.Game.OnInitialize = Init;

                SadConsole.Game.Instance.Run();
                SadConsole.Game.Instance.Dispose();
            }
            catch (Exception ex)
            {
                log.Fatal("Exception terminated app", ex);
            }
            log.Info("Run ended");
        }

        private static void Init()
        {
            // Any startup code for your game. We will use an example console for now
            var startingConsole = SadConsole.Global.CurrentScreen;
            startingConsole.FillWithRandomGarbage();
            startingConsole.Fill(new Rectangle(3, 3, 27, 5), null, Color.Black, 0, SpriteEffects.None);
            startingConsole.Print(6, 5, "Hello from SadConsole", ColorAnsi.CyanBright);
        }


        static void SideMain(string[] args)
        {
            var log = LogManager.GetLogger("CB");
            log.Info("\n======================================");
            log.Info("Run started");
            try
            {
                var c = new Container(rules => rules.WithDefaultReuse(Reuse.InCurrentScope));

                c.Register<EventBus, EventBus>();
                c.Register<Describer, Describer>();
                c.Register<Schedule, Schedule>();
                c.Register<ISchedule, Schedule>();
                c.Register<IGameWindow, GameWindow>();
                c.Register<IMessageOutput, GameWindow>();
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
