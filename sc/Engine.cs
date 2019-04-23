using System;
using System.Collections.Generic;
using SadConsole;
using SadConsole.Input;
using SadConsole.Components;
using SadConsole.Controls;
using SadConsole.Entities;
using GameState = SadConsole.Global;
using Microsoft.Xna.Framework;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using log4net;

namespace CbRework
{

    public class Engine : ContainerConsole
    {
        private ILog log;

        public int MapWidth = 200;
        public int MapHeight = 130;
        public int WindowWidth;
        public int WindowHeight;
        private ScrollingConsole MapConsole;
        public Window MapWindow;
        public MessageLogWindow MessageLog;
        
        private Keyboard Kbd;
        public Queue<AsciiKey> InputQueue;
        private Map Map;
        private Entity Player;


        public override void Update(TimeSpan timeElapsed)
        {
            CheckKeyboard();
            base.Update(timeElapsed);
        }

        public Engine(int windowWidth, int windowHeight)
            : base()
        {
            log = LogManager.GetLogger("CB", "CB.NewEngine");

            IsVisible = true;
            IsFocused = true;
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;

            Parent = GameState.CurrentScreen;
            Kbd = GameState.KeyboardState;
            InputQueue = new Queue<AsciiKey>();

            Init();
        }

        public void Init()
        {
            var gen = new MapGen();
            Map = gen.GenerateMap(MapWidth, MapHeight, 100, 5, 15);
            log.Debug("Generated map");

            Player = CreatePlayer(Map.PlayerStartPoint);

            MapWindow = CreateMapWindow(WindowWidth / 2, WindowHeight - 8, "Game Map");
            MapConsole.Children.Add(Player);
            MapWindow.Children.Add(MapConsole);
            Children.Add(MapWindow);
            MapWindow.Show();

            MapConsole.CenterViewPortOnPoint(Player.Position);

            MessageLog = CreateMessageLog();
            Children.Add(MessageLog);
            MessageLog.Show();
        }

        public MessageLogWindow CreateMessageLog()
        {
            var messageLog = new MessageLogWindow(WindowWidth, 8, "Message Log");
            MessageLog.Position = new Point(0, WindowHeight - 8);

            ////  Rudimentary fill the window
            //MessageLog.Add("Testing");
            //MessageLog.Add("Testing B");
            //MessageLog.Add("Testing three");
            //MessageLog.Add("Testing 4");
            //MessageLog.Add("Testing V");
            //MessageLog.Add("Testing x6");
            //MessageLog.Add("Testing Seventh");

            return messageLog;
        }

        public Window CreateMapWindow(int width, int height, string title)
        {
            int consoleWidth = width - 2;
            int consoleHeight = height - 2;

            Window mapWindow = new Window(width, height)
            {
                CanDrag = true,
                Title = title.Align(HorizontalAlignment.Center, consoleWidth)
            };

            //TODO: make click do something
            Button closeButton = new Button(3, 1)
            {
                Position = new Point(width - 3, 0),
                Text = "X"
            };
            mapWindow.Add(closeButton);

            MapConsole = new ScrollingConsole(
                MapWidth, MapHeight,
                Global.FontDefault, new Rectangle(0, 0, WindowWidth, WindowHeight),
                Map.Tiles);
            log.Debug("Created map console.");

            // Fit the MapConsole inside the border
            MapConsole.ViewPort = new Rectangle(0, 0, consoleWidth, consoleHeight);
            MapConsole.Position = new Point(1, 1);

            log.Debug("Created map window.");
            return mapWindow;
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
            foreach (var key in Kbd.KeysPressed)
            {
                InputQueue.Enqueue(key);
            }

            int xOff = 0;
            int yOff = 0;
            if (Kbd.IsKeyPressed(Keys.Left)) xOff = -1;
            if (Kbd.IsKeyPressed(Keys.Right)) xOff = 1;
            if (Kbd.IsKeyPressed(Keys.Up)) yOff = -1;
            if (Kbd.IsKeyPressed(Keys.Down)) yOff = 1;
            if (xOff == 0 && yOff == 0) return;

            Player.Position += new Point(xOff, yOff);
        }

        // The entities in the given map will be the MapConsole's only entities
        private void SyncMapEntities(Map map)
        {
            // update the Map Console to hold only the Map's entities
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
