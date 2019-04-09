using SadConsole;
using Microsoft.Xna.Framework;
using SadConsole.Entities;
using GameState = SadConsole.Global;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using log4net;
using SadConsole.Components;
using System;
using SadConsole.Controls;

namespace CbRework
{

    public class Engine : ContainerConsole
    {
        private ILog log;
        public Window MapWindow;
        private ScrollingConsole MapConsole;
        private SadConsole.Input.Keyboard Kbd;
        private Entity Player;

        public int WindowWidth;
        public int WindowHeight;
        public int MapWidth = 200;
        public int MapHeight = 130;


        public Engine(int windowWidth, int windowHeight)
            : base()
        {
            IsVisible = true;
            IsFocused = true;
            Parent = Global.CurrentScreen;

            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            LogManager.CreateRepository("CB");
            log = LogManager.GetLogger("CB", "NewEngine");
            log.Info("\n======================================");
            log.Info("Run started");

            Kbd = GameState.KeyboardState;

            Init();
        }

        private Map Map;

        public void Init()
        {
            var gen = new MapGen();
            Map = gen.GenerateMap(MapWidth, MapHeight, 100, 5, 15);
            Player = CreatePlayer(Map.PlayerStartPoint);

            CreateConsoles();
            CreateMapWindow(WindowWidth / 2, WindowHeight / 2, "Game Map");

            MapConsole.CenterViewPortOnPoint(Player.Position);
        }

        public void CreateConsoles()
        {
            MapConsole = new ScrollingConsole(MapWidth, MapHeight, Global.FontDefault, new Rectangle(0, 0, WindowWidth, WindowHeight), Map.Tiles);
            log.Debug("Created map console.");
        }

        public void CreateMapWindow(int width, int height, string title)
        {
            MapWindow = new Window(width, height)
            {
                CanDrag = true
            };

            // Fit the MapConsole into the window
            int mapConsoleWidth = width - 2;
            int mapConsoleHeight = height - 2;
            MapConsole.ViewPort = new Rectangle(0, 0, mapConsoleWidth, mapConsoleHeight);
            MapConsole.Position = new Point(1, 1);

            //TODO: make click do something
            Button closeButton = new Button(3, 1)
            {
                Position = new Point(width-3, 0),
                Text = "[X]"
            };
            MapWindow.Add(closeButton);

            // Centre the title text at the top of the window
            MapWindow.Title = title.Align(HorizontalAlignment.Center, mapConsoleWidth);

            //  These belong elsewhere.
            Children.Add(MapWindow);
            MapWindow.Children.Add(MapConsole);
            MapConsole.Children.Add(Player);

            MapWindow.Show();
            log.Debug("Created map window.");
        }

        private Entity CreatePlayer(Point playerLocation)
        {
            var player = new Entity(1, 1);
            player.Animation.CurrentFrame[0].Glyph = '@';
            player.Animation.CurrentFrame[0].Foreground = Color.AntiqueWhite;
            player.Position = playerLocation;
            player.Components.Add(new EntityViewSyncComponent());

            log.Debug("Created player entity.");
            return player;
        }

        public override void Update(TimeSpan timeElapsed)
        {
            CheckKeyboard();
            base.Update(timeElapsed);
        }

        public void CheckKeyboard()
        {
            int xOff = 0;
            int yOff = 0;
            if (Kbd.IsKeyPressed(Keys.Left)) xOff = -1;
            if (Kbd.IsKeyPressed(Keys.Right)) xOff = 1;
            if (Kbd.IsKeyPressed(Keys.Up)) yOff = -1;
            if (Kbd.IsKeyPressed(Keys.Down)) yOff = 1;
            if (xOff == 0 && yOff == 0) return;

            Player.Position += new Point(xOff, yOff);
            MapConsole.CenterViewPortOnPoint(Player.Position);
        }
    }
}
