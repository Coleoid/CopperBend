using RLNET;


namespace CopperBend.App
{
    class Program
    {
        private static bool _renderRequired;
        public static CommandSystem CommandSystem;
        private static RLRootConsole _rootConsole;

        static void Main(string[] args)
        {
            _renderRequired = true;
            CommandSystem = new CommandSystem();

            var settings = new RLSettings
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

            _rootConsole = new RLRootConsole(settings);

            var game = new GameLoop();
            game.Init(_rootConsole);

            var loader = new MapLoader();
            var map = loader.DemoMap();
            game.Map = map;

            var player = new Player();
            game.Player = player;
            map.Actors.Add(player);
            map.UpdatePlayerFieldOfView(player);

            game.Run();
        }
    }
}
