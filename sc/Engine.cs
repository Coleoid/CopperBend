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
        public MessageLogWindow MessageLog;

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
            log = LogManager.GetLogger("CB", "CB.NewEngine");

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
            CreateMapWindow(WindowWidth / 2, WindowHeight - 8, "Game Map");
            Children.Add(MapWindow);

            MapConsole.CenterViewPortOnPoint(Player.Position);

            MessageLog = new MessageLogWindow(WindowWidth, 8, "Message Log");
            Children.Add(MessageLog);
            MessageLog.Show();
            MessageLog.Position = new Point(0, WindowHeight - 8);


            MessageLog.Add("Testing");
            MessageLog.Add("Testing B");
            MessageLog.Add("Testing three");
            MessageLog.Add("Testing 4");
            MessageLog.Add("Testing V");
            MessageLog.Add("Testing x6");
            MessageLog.Add("Testing Seventh");
        }

        public override void Update(TimeSpan timeElapsed)
        {
            CheckKeyboard();
            base.Update(timeElapsed);
        }

        public void CreateConsoles()
        {
            MapConsole = new ScrollingConsole(MapWidth, MapHeight, Global.FontDefault, new Rectangle(0, 0, WindowWidth, WindowHeight), Map.Tiles);
            log.Debug("Created map console.");
        }

        public void CreateMapWindow(int width, int height, string title)
        {
            // Fit the MapConsole into the window
            int mapConsoleWidth = width - 2;
            int mapConsoleHeight = height - 2;

            MapWindow = new Window(width, height)
            {
                CanDrag = true,
                Title = title.Align(HorizontalAlignment.Center, mapConsoleWidth)
            };

            //TODO: make click do something
            Button closeButton = new Button(3, 1)
            {
                Position = new Point(width-3, 0),
                Text = "X"
            };
            MapWindow.Add(closeButton);


            MapWindow.Children.Add(MapConsole);
            MapConsole.ViewPort = new Rectangle(0, 0, mapConsoleWidth, mapConsoleHeight);
            MapConsole.Position = new Point(1, 1);

            MapConsole.Children.Add(Player);

            MapWindow.Show();
            log.Debug("Created map window.");
        }

        private CbEntity CreatePlayer(Point playerLocation)
        {
            var player = new Player(Color.AntiqueWhite, Color.Transparent);
            player.Animation.CurrentFrame[0].Glyph = '@';
            player.Animation.CurrentFrame[0].Foreground = Color.AntiqueWhite;
            player.Position = playerLocation;
            player.Components.Add(new EntityViewSyncComponent());

            log.Debug("Created player entity.");
            return player;
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

        // The entities in the given map will be the MapConsole's only entities
        private void SyncMapEntities(Map map)
        {
            // update the MapConsole to hold only the Map's entities
            MapConsole.Children.Clear();
            foreach (var entity in map.Entities.Items)
            {
                MapConsole.Children.Add(entity);
            }

            // keep future changes to the map Entities up-to-date in the MapConsole
            map.Entities.ItemAdded += (s, a) => MapConsole.Children.Add(a.Item);
            map.Entities.ItemRemoved += (s, a) => MapConsole.Children.Remove(a.Item);
        }
    }
}
