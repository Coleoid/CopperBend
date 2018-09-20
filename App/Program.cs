using CopperBend.App.Model;
using RLNET;


namespace CopperBend.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootConsole = InitRootConsole();
            var game = new GameEngine(rootConsole);

            var loader = new MapLoader();
            var map = loader.DemoMap();
            game.Map = map;

            var player = new Player();
            game.Player = player;
            map.Actors.Add(player);

            //  currently needed, yet seems like it shouldn't be here.
            map.UpdatePlayerFieldOfView(player);

            game.Run();
        }

        private static RLRootConsole InitRootConsole()
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

            return new RLRootConsole(consoleSettings);
        }
    }
}
