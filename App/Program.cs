using CopperBend.App.Model;
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

            var loader = new MapLoader();
            // var map = loader.DemoMap();
            var map = loader.FarmMap();

            map.ViewpointActor = player;
            game.LoadMap(map);

            game.Run();
        }

        private static Actor InitPlayer()
        {
            var player = new Actor(1, 1)
            {
                Name = "Our Dude",
                Symbol = '@',
                Color = Palette.DbLight,
                Awareness = 4,
                Health = 23
            };

            var hoe = new Hoe(0, 0)
            {
                Name = "hoe",
                Color = RLColor.Brown,
                Symbol = '/',
                IsUsable = true,
            };

            var rock = new Item(0, 0)
            {
                Name = "rock",
                Color = RLColor.Gray,
                Symbol = '*'
            };

            var seed = new Seed(0, 0, 1, SeedType.Boomer)
            {
                Name = "seed",
                Color = RLColor.LightGreen,
                Symbol = '.'
            };

            var seed_2 = new HealerSeed(0, 0, 2)
            {
                Name = "seed",
                Color = RLColor.LightGreen,
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
