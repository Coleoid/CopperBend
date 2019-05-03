using System;
using CopperBend.Contract;
using SadConsole;
using SadConsole.Controls;
using Microsoft.Xna.Framework;
using Size = System.Drawing.Size;
using System.Collections.Generic;

namespace CopperBend.Engine
{
    public class UIBuilder
    {
        public readonly Size GameSize;

        public UIBuilder(Size gameSize)
        {
            GameSize = gameSize;
        }

        public MessageLogWindow CreateMessageLog()
        {
            var messageLog = new MessageLogWindow(GameSize.Width, 8, "Message Log")
            {
                Position = new Point(0, GameSize.Height - 8)
            };

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

        public (ScrollingConsole, Window) CreateMapWindow(
            Size windowSize, 
            Size mapSize, 
            string title,
            CompoundMap fullMap
            )
        {
            int consoleWidth = windowSize.Width - 2;
            int consoleHeight = windowSize.Height - 2;

            Window mapWindow = new Window(windowSize.Width, windowSize.Height)
            {
                CanDrag = true,
                Title = title.Align(HorizontalAlignment.Center, consoleWidth)
            };
            //log.Debug("Created map window.");

            //TODO: make click do something
            Button closeButton = new Button(3, 1)
            {
                Position = new Point(windowSize.Width - 3, 0),
                Text = "X"
            };
            mapWindow.Add(closeButton);

            Cell[] initialCells = GetCells(fullMap);

            var mapConsole = new ScrollingConsole(
                mapSize.Width, mapSize.Height,
                Global.FontDefault, new Rectangle(0, 0, GameSize.Width, GameSize.Height),
                initialCells)
            {

                // Fit the MapConsole inside the border
                ViewPort = new Rectangle(0, 0, consoleWidth, consoleHeight),
                Position = new Point(1, 1)
            };
            //log.Debug("Created map console.");

            mapWindow.Children.Add(mapConsole);

            return (mapConsole, mapWindow);
        }

        private Cell[] GetCells(CompoundMap fullMap)
        {
            List<Cell> cells = new List<Cell>();

            for (int y = 0; y < fullMap.SpaceMap.Height; y++)
            {
                for (int x = 0; x < fullMap.SpaceMap.Width; x++)
                {
                    cells.Add(fullMap.SpaceMap.GetItem(x,y).Terrain.Looks);
                }
            }

            return cells.ToArray();
        }
    }
}
