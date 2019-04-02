using System;
using SadConsole;
using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Console = SadConsole.Console;

namespace CbRework
{
    public static class Program
    {
        private static SadConsole.Entities.Entity player;
        private static Console startingConsole;

        static void Main()
        {
            // Setup the engine and create the main window.
            SadConsole.Game.Create(60, 30);

            // Hook the start event so we can add consoles to the system.
            SadConsole.Game.OnInitialize = Init;
            SadConsole.Game.OnUpdate = Update;

            // Start the game.
            SadConsole.Game.Instance.Run();
            SadConsole.Game.Instance.Dispose();
        }

        static void Init()
        {
            startingConsole = new Console(60, 30);
            //startingConsole.Font = "cheepicus12";
            startingConsole.Fill(new Rectangle(3, 3, 23, 3), Color.Violet, Color.Black, 0, 0);
            startingConsole.Print(4, 4, "Hello from SadConsole");

            CreatePlayer();
            startingConsole.Children.Add(player);

            SadConsole.Global.CurrentScreen = startingConsole;
        }

        // Create a player using SadConsole's Entity class
        private static void CreatePlayer()
        {
            player = new SadConsole.Entities.Entity(1, 1);
            player.Animation.CurrentFrame[0].Glyph = '@';
            player.Animation.CurrentFrame[0].Foreground = Color.HotPink;
            player.Position = new Point(20, 10);
        }

        private static void Update(GameTime time)
        {
            // Called each logic update.

            if (SadConsole.Global.KeyboardState.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.F5))
            {
                startingConsole.Fill(new Rectangle(3, 3, 23, 3), Color.Violet, Color.Black, 0, 0);
                startingConsole.Print(4, 4, "Again from SadConsole");
            }

            if (SadConsole.Global.KeyboardState.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Up))
            {
                player.Position += new Point(0, -1);
            }
        }
    }
}
