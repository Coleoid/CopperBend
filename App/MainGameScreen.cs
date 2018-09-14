using System;
using RLNET;
using WinMan;


namespace CopperBend.App
{
    internal class MainGameScreen : Screen
    {
        private readonly MapPanel mapPanel;
        private readonly MenuPanel menuPanel;
        private readonly AlertPanel alertPanel;

        public MainGameScreen()
        {
            mapPanel = new MapPanel(SizeC(0), SizeC(0), WidthMinus(10), HeightMinus(0));
            menuPanel = new MenuPanel(WidthMinus(10), SizeC(0), SizeC(10), HeightMinus(0));
            alertPanel = new AlertPanel(HalfWidth(), HalfHeight(), "Alert:  Press key to dismiss.");

            addPanel(mapPanel);
            addPanel(menuPanel);
            addPanel(alertPanel);
        }

        public void SetMap(IcbMap map)
        {
            mapPanel.Map = map;
        }

        internal class MapPanel : Panel
        {
            public IcbMap Map { get; set; }

            public MapPanel(
                ResizeCalc rootX, ResizeCalc rootY, 
                ResizeCalc width, ResizeCalc height)
                : base(
                    rootX, rootY, 
                    width, height, 
                    false, true)
            {
                resizeMap(widthCalc(), heightCalc());

                OnResize += (object s, EventArgs e) => 
                    resizeMap(widthCalc(), heightCalc());
            }

            private void resizeMap(int width, int height)
            {
            }

            public override void UpdateLayout(object sender, UpdateEventArgs e)
            {
                if (Map == null)
                {
                    console.Print(0, 0, "No map loaded.", RLColor.Yellow);
                    return;
                }

                var renderWidth = Math.Min(Width, Map.Width);
                var renderHeight = Math.Min(Height, Map.Height);

                for (int x = 0; x < renderWidth; ++x)
                for (int y = 0; y < renderHeight; ++y)
                {
                    var rep = TileRepresentation.OfTerrain(Map.Terrain[x, y]);
                    console.Set(x, y, rep.Foreground, rep.Background, rep.Symbol);
                }
            }


            protected override void OnKeyPress(object sender, KeyPressEventArgs e)
            {
                System.Console.WriteLine($"Map Key: [{e.KeyPress.Key}]");
                if (e.KeyPress.Key == RLKey.C && e.KeyPress.Control)
                {
                    //TODO:  Set some flag to be seen by outer event loop
                }

                e.Cancel = true;
            }

        }


        internal class MenuPanel : Panel
        {
            public MenuPanel(ResizeCalc rootX, ResizeCalc rootY, ResizeCalc width, ResizeCalc height)
                : base(rootX, rootY, width, height, true, false)
            {
            }

            public override void UpdateLayout(object sender, UpdateEventArgs e)
            {
                for (int i = 0; i < 5; ++i)
                    console.Print(1, i, $"op{i}", RLColor.White, RLColor.Black);
            }

            protected override void OnKeyPress(object sender, KeyPressEventArgs e)
            {
                System.Console.WriteLine($"Menu Key: [{e.KeyPress.Key}]");
                if (e.KeyPress.Key == RLKey.C && e.KeyPress.Control)
                {
                    //TODO:  Set some flag to be seen by outer event loop
                }

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

            public override void UpdateLayout(object sender, UpdateEventArgs e)
            {
                console.Print(0, 2, message, RLColor.White, 2);
            }

            protected override void OnKeyPress(object sender, KeyPressEventArgs e)
            {
                //  Any keypress will be swallowed and close the alert
                if (Shown)
                {
                    Hide();
                    e.Cancel = true;
                }
            }
        }
    }
}
