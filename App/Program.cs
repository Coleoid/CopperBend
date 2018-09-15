using RLNET;


namespace CopperBend.App
{
    //  Code originally a reference sample from WinMan
    class Program
    {
        private static MainGameScreen mainGameScreen;

        static void Main(string[] args)
        {
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

            GameLoop.Init(settings);
            mainGameScreen = new MainGameScreen();

            var loader = new MapLoader();
            var map = loader.DemoMap();

            mainGameScreen.SetMap(map);

            mainGameScreen.Show();
            GameLoop.Run();
        }
    }
}
