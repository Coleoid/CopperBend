﻿using System;
using Size = System.Drawing.Size;
using log4net;
using SadConsole;
using SadConsole.Controls;
using GoRogue;
using CopperBend.Contract;

namespace CopperBend.Engine
{
    public class UIBuilder
    {
        private readonly ILog log;
        public readonly Size GameSize;
        //public Font Font;

        public UIBuilder(Size gameSize, Font font, ILog logger)
        {
            log = logger;

            GameSize = gameSize;
            //Font = font;
        }

        public MessageLogWindow CreateMessageLog()
        {
            var messageLog = new MessageLogWindow(GameSize.Width, 8, "Message Log")
            {
                Position = new Coord(0, GameSize.Height - 8)
            };

            ////  Garbage to visually test the window
            //MessageLog.Add("Testing");
            //MessageLog.Add("Testing B");
            //MessageLog.Add("Testing three");
            //MessageLog.Add("Testing 4");
            //MessageLog.Add("Testing V");
            //MessageLog.Add("Testing #6");
            //MessageLog.Add("Testing Seventh");
            //MessageLog.Add("Testing[7]");

            return messageLog;
        }

        public (LayeredConsole, Window) CreateMenuWindow(Size windowSize, string title, ICompoundMap fullMap)
        {
            int viewWidth = windowSize.Width - 2;
            int viewHeight = windowSize.Height - 2;

            Window menuWindow = new Window(windowSize.Width, windowSize.Height)
            {
                CanDrag = true,
                Title = title.Align(HorizontalAlignment.Center, viewWidth)
            };
            log.DebugFormat("Created menu window, [{0}].", menuWindow.AbsoluteArea);

            var menuConsole = new LayeredConsole(fullMap.Width, fullMap.Height, 2);

            // Fit the Console inside the Window border
            menuConsole.Position = new Coord(1, 1);
            log.Debug("Created layered console.");

            menuWindow.Children.Add(menuConsole);

            return (menuConsole, menuWindow);
        }

        public (ScrollingConsole, Window) CreateMapWindow(Size windowSize, string title, ICompoundMap fullMap)
        {
            int viewWidth = windowSize.Width - 2;
            int viewHeight = windowSize.Height - 2;

            Window mapWindow = new Window(windowSize.Width, windowSize.Height)
            {
                CanDrag = true,
                Title = title.Align(HorizontalAlignment.Center, viewWidth)
            };
            log.DebugFormat("Created map window, [{0}].", mapWindow.AbsoluteArea);

            //// what would close button do, bring up save/quit?
            //Button closeButton = new Button(3, 1)
            //{
            //    Position = new Coord(windowSize.Width - 3, 0),
            //    Text = "X"
            //};
            //mapWindow.Add(closeButton);

            var mapConsole = new ScrollingConsole(
                fullMap.Width, fullMap.Height, SadConsole.Global.FontDefault,
                new Rectangle(0, 0, viewWidth, viewHeight));

            // Fit the MapConsole inside the MapWindow border
            mapConsole.Position = new Coord(1, 1);
            log.DebugFormat("Created map console, map size [{0},{1}], viewport size [{2}].", fullMap.Width, fullMap.Height, mapWindow.ViewPort);

            mapWindow.Children.Add(mapConsole);

            return (mapConsole, mapWindow);
        }
    }
}
