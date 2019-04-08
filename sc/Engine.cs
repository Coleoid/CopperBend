using SadConsole;
using Microsoft.Xna.Framework;
using SadConsole.Entities;
using GameState = SadConsole.Global;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using log4net;
using SadConsole.Components;

namespace CbRework
{
    public class Engine
    {
        private ScrollingConsole StartingConsole;
        private SadConsole.Input.Keyboard Kbd;
        private Entity Player;

        public Engine()
        {
            LogManager.CreateRepository("CB");
            var log = LogManager.GetLogger("CB", "NewEngine");
            log.Info("\n======================================");
            log.Info("Run started");

            Kbd = GameState.KeyboardState;
        }

        public int mapWidth = 200;
        public int mapHeight = 130;

        public void Init()
        {
            var gen = new MapGen();
            var map = gen.GenerateMap(mapWidth, mapHeight, 100, 5, 15);
            var roomCenter = gen.RoomCenter(0);
            StartingConsole = new ScrollingConsole(mapWidth, mapHeight, Global.FontDefault, new Rectangle(0, 0, 80, 40), map.Tiles);

            Player = CreatePlayer(roomCenter);
            StartingConsole.Children.Add(Player);
            Player.Components.Add(new EntityViewSyncComponent());

            GameState.CurrentScreen = StartingConsole;
        }

        private Entity CreatePlayer(Point playerLocation)
        {
            var player = new Entity(1, 1);
            player.Animation.CurrentFrame[0].Glyph = '@';
            player.Animation.CurrentFrame[0].Foreground = Color.AntiqueWhite;
            player.Position = playerLocation;

            return player;
        }

        public void Update(GameTime time)
        {
            int xOff = 0;
            int yOff = 0;
            if (Kbd.IsKeyPressed(Keys.Left)) xOff = -1;
            if (Kbd.IsKeyPressed(Keys.Right)) xOff = 1;
            if (Kbd.IsKeyPressed(Keys.Up)) yOff = -1;
            if (Kbd.IsKeyPressed(Keys.Down)) yOff = 1;
            Player.Position += new Point(xOff, yOff);
            StartingConsole.CenterViewPortOnPoint(Player.Position);
        }
    }
}
