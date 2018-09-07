using System;
using RLNET;
using WinMan;


namespace CopperBend.App
{
    //  Test code kiped from WinMan
    class Program
    {
        private static MainGameScreen mainGameScreen;

        static void Main(string[] args)
        {

            var settings = new RLSettings
            {
                BitmapFile = "terminal8x8.png",
                Width = 60,
                Height = 40,
                CharWidth = 8,
                CharHeight = 8,
                Scale = 1f,
                Title = "RLNET Window Manager Test",
                WindowBorder = RLWindowBorder.Resizable,
                ResizeType = RLResizeType.ResizeCells
            };
            // All the test panels here work with resize via scaling as well.
            //settings.ResizeType = RLResizeType.ResizeScale;

            Engine.Init(settings);

            mainGameScreen = new MainGameScreen();
            mainGameScreen.Show();

            Engine.Run();
        }
    }

    internal class MainGameScreen : Screen
    {
        private readonly MapPanel mapPanel;
        private readonly MenuPanel menuPanel;
        private readonly AlertPanel alertPanel;

        public MainGameScreen()
        {
            mapPanel = new MapPanel(SizeC(0), SizeC(0), WidthMinus(10), HeightMinus(0));
            menuPanel = new MenuPanel(WidthMinus(10), SizeC(0), SizeC(10), HeightMinus(0));
            alertPanel = new AlertPanel(HalfWidth(), HalfHeight(), "I'm an overlay! Press a key to toggle me!");

            addPanel(mapPanel);
            addPanel(menuPanel);
            addPanel(alertPanel);
        }


        internal class MapPanel : Panel
        {
            private int[,] map;

            public MapPanel(ResizeCalc rootX, ResizeCalc rootY, ResizeCalc width, ResizeCalc height)
                : base(rootX, rootY, width, height, false, true)
            {
                Random rng = new Random();
                map = new int[Width, Height];

                for (int x = 0; x < Width; ++x)
                for (int y = 0; y < Height; ++y)
                    map[x, y] = rng.Next(0, 5);

                OnResize += (object s, EventArgs e) => resizeMap(widthCalc(), heightCalc());
            }

            // would be unnecessary here, honestly.
            // just a hack to resize array so i dont have to have an actual map
            private void resizeMap(int width, int height)
            {
                Random rng = new Random();
                map = new int[width, height];

                for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                    map[x, y] = rng.Next(0, 5);
            }

            public override void UpdateLayout(object sender, UpdateEventArgs e)
            {
                for (int x = 0; x < Width; ++x)
                for (int y = 0; y < Height; ++y)
                {
                    char tileChar = (map[x, y] == 0 ? '#' : '.');
                    console.Set(x, y, RLColor.White, RLColor.Black, tileChar, 1);
                }
            }
        }


        internal class MenuPanel : Panel
        {
            public MenuPanel(ResizeCalc rootX, ResizeCalc rootY, ResizeCalc width, ResizeCalc height) : base(rootX, rootY, width, height, true, false)
            {
            }

            public override void UpdateLayout(object sender, UpdateEventArgs e)
            {
                for (int i = 0; i < 5; ++i)
                    console.Print(1, i, $"op{i}", RLColor.White, RLColor.Black);
            }

            protected override void OnKeyPress(object sender, KeyPressEventArgs e)
            {
                System.Console.WriteLine($"Key: [{e.KeyPress.Key}]");
                //if (e.KeyPress.Key == RLKey.C && e.KeyPress.Control)
                //    exit;

                e.Cancel = true;
            }
        }


        internal class AlertPanel : Panel
        {
            private string message;

            public AlertPanel(ResizeCalc centerX, ResizeCalc centerY, string message)
                : base(() => centerX() - (message.Length / 2), () => centerY() - 2, () => message.Length, Screen.SizeC(5), true, false)
            {
                this.message = message;
            }

            // Reposition map based on the following center coordinates
            //public void Reposition(int centerX, int centerY)
            //{
            //RootX = centerX - (message.Length / 2);
            //RootY = centerY - 2;
            //}

            public override void UpdateLayout(object sender, UpdateEventArgs e)
            {
                console.Print(0, 2, message, RLColor.White, 2);
            }

            protected override void OnKeyPress(object sender, KeyPressEventArgs e)
            {
                if (Shown)
                    Hide();

                // Greedy capture don't send it to anyone else! This basically sets this as modal window, nobody else can get any key input.
                // Defaults to false
                e.Cancel = true;
            }
        }
    }
}
