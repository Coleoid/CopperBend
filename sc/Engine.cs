using SadConsole;
using Microsoft.Xna.Framework;
using SadConsole.Entities;
using GameState = SadConsole.Global;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using log4net;
//using System;

namespace CbRework
{
    public class Engine
    {
        public Console StartingConsole;
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

        public int Width = 200;
        public int Height = 130;

        public void Init()
        {
            var gen = new MapGen();
            var map = gen.GenerateMap(Width, Height, 500, 5, 15);
            StartingConsole = new ScrollingConsole(Width, Height, Global.FontDefault, new Rectangle(0, 0, Width, Height), map.Tiles);

            Player = CreatePlayer();
            StartingConsole.Children.Add(Player);


            GameState.CurrentScreen = StartingConsole;
        }

        // Create a player using SadConsole's Entity class
        private Entity CreatePlayer()
        {
            var player = new SadConsole.Entities.Entity(1, 1);
            player.Animation.CurrentFrame[0].Glyph = '@';
            player.Animation.CurrentFrame[0].Foreground = Color.HotPink;
            player.Position = new Point(15, 15);

            return player;
        }

        public void Update(GameTime time)
        {
            if (GameState.KeyboardState.IsKeyReleased(Keys.F5))
            {
                StartingConsole.Fill(new Rectangle(3, 3, 23, 3), Color.Violet, Color.Black, 0, 0);
                StartingConsole.Print(4, 4, "Again from SadConsole");
            }

            int xOff = 0;
            int yOff = 0;
            if (Kbd.IsKeyPressed(Keys.Left)) xOff = -1;
            if (Kbd.IsKeyPressed(Keys.Right)) xOff = 1;
            if (Kbd.IsKeyPressed(Keys.Up)) yOff = -1;
            if (Kbd.IsKeyPressed(Keys.Down)) yOff = 1;
            Player.Position += new Point(xOff, yOff);
        }
    }
}
