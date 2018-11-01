using CopperBend.App.Model;
using CopperBend.MapUtil;
using RLNET;

namespace CopperBend.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootConsole = InitRootConsole();
            var player = InitPlayer();
            var game = new GameEngine(rootConsole, player);

            game.StartNewGame();
            game.Run();
        }

        private static Point At(int x, int y)
        {
            return new Point(x, y);
        }

        private static Actor InitPlayer()
        {
            var player = new Actor(At(18, 21))
            {
                Name = "Our Dude",
                Symbol = '@',
                ColorForeground = Palette.DbLight,
                Awareness = 8,
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

        private static RLRootConsole InitRootConsole()
        {
            var consoleSettings = new RLSettings
            {
                Title = "Copper Bend",
                BitmapFile = "assets\\terminal12x12_gs_ro.png",
                Width = 60,
                Height = 60,
                CharWidth = 12,
                CharHeight = 12,
                Scale = 1f,
                WindowBorder = RLWindowBorder.Resizable,
                ResizeType = RLResizeType.ResizeCells,
            };

            return new RLRootConsole(consoleSettings);
        }
    }
}
