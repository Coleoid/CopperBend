﻿using CopperBend.App.Model;
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

            var player = InitPlayer();

            game.Player = player;
            map.Actors.Add(player);


            //  currently needed, yet seems like it shouldn't be here.
            map.UpdatePlayerFieldOfView(player);

            game.Run();
        }

        private static Player InitPlayer()
        {
            var player = new Player();

            var hoe = new Item(0, 0)
            {
                Name = "hoe",
                Color = RLColor.Brown,
                Symbol = '/'
            };

            var rock = new Item(0, 0)
            {
                Name = "rock",
                Color = RLColor.Gray,
                Symbol = '*'
            };

            var seed = new Item(0, 0)
            {
                Name = "seed",
                Color = RLColor.LightGreen,
                Symbol = '.'
            };

            player.Inventory.Add(hoe);
            player.Inventory.Add(rock);
            player.Inventory.Add(seed);
            return player;
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
