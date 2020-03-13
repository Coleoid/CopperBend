using System;
using Size = System.Drawing.Size;
using MG = Microsoft.Xna.Framework;
using SadConsole;
using GoRogue;
using log4net;
using CopperBend.Contract;

namespace CopperBend.Fabric
{
    public class UIBuilder : IUIBuilder
    {
        private readonly Size gameSize;
        private readonly ILog log;
        public FontMaster MapFontMaster { get; set; }
        public Font MapFont { get; set; }

        public UIBuilder(Size gameSize, FontMaster mapFontMaster, ILog logger)
        {
            this.gameSize = gameSize;
            MapFontMaster = mapFontMaster;
            MapFont = MapFontMaster.GetFont(Font.FontSizes.One);
            log = logger;
        }

        public IMessageLogWindow CreateMessageLog()
        {
            var messageLog = new MessageLogWindow(gameSize.Width, 8, "Message Log")
            {
                Position = new Coord(0, gameSize.Height - 8),
                DefaultBackground = MG.Color.Black,
            };

            return messageLog;
        }

        public (ControlsConsole, Window) CreateM2Window(Size windowSize, string title)
        {
            int viewWidth = windowSize.Width - 2;
            int viewHeight = windowSize.Height - 2;

            Window menuWindow = new Window(windowSize.Width, windowSize.Height)
            {
                CanDrag = true,
                Title = title.Align(HorizontalAlignment.Center, viewWidth),
                DefaultBackground = MG.Color.Black,
            };
            log.DebugFormat("Created menu window, [{0}].", menuWindow.AbsoluteArea);

            var menuConsole = new ControlsConsole(viewWidth, viewHeight)
            {
                DefaultBackground = MG.Color.Black,
            };

            // Fit the Console inside the Window border
            menuConsole.Position = new Coord(1, 1);
            log.DebugFormat("Created menu console, [{0}].", menuConsole.AbsoluteArea);

            menuConsole.Clear();

            menuWindow.Children.Add(menuConsole);
            return (menuConsole, menuWindow);
        }

        public (ScrollingConsole, Window) CreateMapWindow(Size windowSize, string title, ICompoundMap fullMap)
        {
            int viewWidth = windowSize.Width - 2;
            int viewHeight = windowSize.Height - 2;

            Window mapWindow = new Window(windowSize.Width, windowSize.Height, MapFont)
            {
                CanDrag = true,
                Title = title.Align(HorizontalAlignment.Center, viewWidth),
            };
            log.DebugFormat("Created map window, [{0}].", mapWindow.AbsoluteArea);

            var mapConsole = new ScrollingConsole(
                fullMap.Width, fullMap.Height, MapFont,
                new MG.Rectangle(0, 0, viewWidth, viewHeight));

            // Fit the MapConsole inside the MapWindow border
            mapConsole.Position = new Coord(1, 1);

            mapWindow.Children.Add(mapConsole);

            log.DebugFormat("Created map console, map size [{0},{1}], viewport size [{2}].", fullMap.Width, fullMap.Height, mapWindow.ViewPort);
            return (mapConsole, mapWindow);
        }
    }
}
