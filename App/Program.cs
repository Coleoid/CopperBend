using CopperBend.App.Model;
using CopperBend.MapUtil;
using log4net.Appender;
using RLNET;
using log4net.Config;
using log4net.Layout;

namespace CopperBend.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = log4net.LogManager.GetLogger("CB");
            log.Info("Run started");
            var rootConsole = InitRootConsole();
            var player = InitPlayer();
            var game = new GameEngine(rootConsole, player);

            game.StartNewGame();
            game.Run();
            log.Info("Run ended");
        }

        private static RLRootConsole InitRootConsole()
        {
            var consoleSettings = new RLSettings
            {
                Title = "Copper Bend",
                BitmapFile = "assets\\terminal12x12_gs_ro.png",
                Width = 80,
                Height = 80,
                CharWidth = 12,
                CharHeight = 12,
                Scale = 1f,
                WindowBorder = RLWindowBorder.Resizable,
                ResizeType = RLResizeType.ResizeCells,
            };

            return new RLRootConsole(consoleSettings);
        }

        private static Actor InitPlayer()
        {
            var player = new Actor(At(0, 0))
            {
                Name = "Our Dude",
                Symbol = '@',
                ColorForeground = Palette.DbLight,
                Awareness = 222,
                Health = 23
            };

            var hoe = new Hoe(At(0, 0))
            {
                Name = "hoe",
                ColorForeground = RLColor.Brown,
                Symbol = '/',
                IsUsable = true,
            };

            var rock = new Item(At(0, 0))
            {
                Name = "rock",
                ColorForeground = RLColor.Gray,
                Symbol = '*'
            };

            var seed = new Seed(At(0, 0), 1, PlantType.Boomer)
            {
                Name = "seed",
                ColorForeground = RLColor.LightGreen,
                Symbol = '.'
            };

            var seed_2 = new HealerSeed(At(0, 0), 2)
            {
                Name = "seed",
                ColorForeground = RLColor.LightGreen,
                Symbol = '.'
            };

            player.AddToInventory(hoe);
            player.AddToInventory(rock);
            player.AddToInventory(seed);
            player.AddToInventory(seed_2);
            player.WieldedTool = hoe;

            return player;
        }

        private static Point At(int x, int y)
        {
            return new Point(x, y);
        }
    }
}
