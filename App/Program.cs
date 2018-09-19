using RLNET;


namespace CopperBend.App
{
    class Program
    {
        private static RLRootConsole _rootConsole;

        static void Main(string[] args)
        {
            InitRootConsole();

            var game = new GameLoop();
            game.Init(_rootConsole);

            var loader = new MapLoader();
            var map = loader.DemoMap();
            game.Map = map;

            var rock = new Item
            {
                Name = "rock",
                Color = Palette.DbOldStone,
                X = 5, Y = 1,
                Symbol = '*',
            };
            map.Items.Add(rock);

            var player = new Player();
            game.Player = player;
            map.Actors.Add(player);

            //  currently needed, yet seems like it shouldn't be here.
            map.UpdatePlayerFieldOfView(player);

            game.Run();
        }

        private static void InitRootConsole()
        {
            var consoleSettings = new RLSettings
            {
                Title = "Copper Bend",
                BitmapFile = "assets\\terminal8x8.png",
                Width = 60,
                Height = 40,
                CharWidth = 8,
                CharHeight = 8,
                Scale = 1f,
                WindowBorder = RLWindowBorder.Resizable,
                ResizeType = RLResizeType.ResizeCells,
            };

            _rootConsole = new RLRootConsole(consoleSettings);
        }
    }
}
