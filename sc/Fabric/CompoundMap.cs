using System;
using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using GoRogue;
using GoRogue.MapViews;
using CopperBend.Contract;
using System.Linq;
using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Effects;
using YamlDotNet.Serialization;

namespace CopperBend.Fabric
{
    public class CompoundMap : ICompoundMap
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public SpaceMap SpaceMap { get; set; }
        public MultiSpatialMap<IBeing> BeingMap { get; set; }
        public ItemMap ItemMap { get; set; }
        public BlightMap BlightMap { get; set; }
        public List<LocatedTrigger> LocatedTriggers { get; set; }

        public FOV FOV { get; set; }
        public bool VisibilityChanged { get; set; }

        [YamlIgnore]
        public List<Coord> CoordsWithChanges { get; } = new List<Coord>();

        public bool CanSeeThrough(Coord location) => SpaceMap.CanSeeThrough(location);
        public bool CanWalkThrough(Coord location) => SpaceMap.CanWalkThrough(location);
        public bool CanPlant(Coord location) => SpaceMap.CanPlant(location);

        public IMapView<int> GetView_BlightStrength()
        {
            throw new NotImplementedException();
        }

        public IMapView<bool> GetView_CanSeeThrough()
        {
            return new LambdaMapView<bool>(
                () => Width, () => Height,
                (coord) => CanSeeThrough(coord)
            );
        }

        public IMapView<bool> GetView_CanWalkThrough()
        {
            throw new NotImplementedException();
        }

        public bool IsWithinMap(Coord location)
        {
            throw new NotImplementedException();
        }

        public EffectsManager EffectsManager { get; private set; }


        /// <summary> Set all cells to blank if unknown, or 'unseen' color of terrain if known </summary>
        public void SetInitialConsoleCells(ScrollingConsole console, SpaceMap spaceMap)
        {
            var unknownCell = new Cell(Color.Black, Color.Black, ' ');
            var knownCell = new Cell(Color.DarkGray, Color.Black, 'i');

            for (int y = 0; y < spaceMap.Height; y++)
            {
                for (int x = 0; x < spaceMap.Width; x++)
                {
                    var space = spaceMap.GetItem((x, y));
                    if (space.IsKnown)
                    {
                        knownCell.Glyph = space.Terrain.Looks.Glyph;
                        console.SetCellAppearance(x, y, knownCell);
                    }
                    else
                    {
                        console.SetCellAppearance(x, y, unknownCell);
                    }
                }
            }

            EffectsManager = new EffectsManager(console);
        }

        public void UpdateFOV(ScrollingConsole console, Coord position)
        {
            FOV.Calculate(position);

            SpaceMap.SeeCoords(FOV.NewlySeen);

            //  Cells outside of FOV are gray on black,
            // with only the glyph of the terrain showing.
            var unseenCell = new Cell(Color.DarkGray, Color.Black, ' ');
            foreach (var location in FOV.NewlyUnseen.Where(c => console.ViewPort.Contains(c)))
            {
                //  Wiping the effect restores the original cell colors,
                // so it must happen before setting OOV appearance
                EffectsManager.SetEffect(console.Cells[location.Y * Width + location.X], null);
                unseenCell.Glyph = SpaceMap.GetItem(location).Terrain.Looks.Glyph;
                console.SetCellAppearance(location.X, location.Y, unseenCell);
            }

            UpdateViewOfCoords(console, FOV.NewlySeen);
        }

        public void UpdateViewOfCoords(ScrollingConsole console, IEnumerable<Coord> coords)
        {
            //  Show the BG color of the terrain, then whatever comes first:
            //  The FG of any visible being at the location, or
            //  the FG of the topmost item at the loc'n, or
            //  the FG of the terrain type.
            //  Then (later, 0.5?) modified by lighting, et c.
            foreach (var position in coords.Where(c => console.ViewPort.Contains(c)))
            {
                var targetCell = new Cell();
                var rawCell = SpaceMap.GetItem(position).Terrain.Looks;
                targetCell.Background = rawCell.Background;

                var beings = BeingMap.GetItems(position).ToList();
                var items = ItemMap.GetItems(position).ToList();
                if (beings.Any())
                {
                    var being = beings.Last();
                    targetCell.Foreground = being.Foreground;
                    targetCell.Glyph = being.Glyph;
                }
                else if (items.Any())
                {
                    var item = items.Last();
                    targetCell.Foreground = item.Foreground;
                    targetCell.Glyph = item.Glyph;
                }
                else
                {
                    targetCell.Foreground = rawCell.Foreground;
                    targetCell.Glyph = rawCell.Glyph;
                }

                console.SetCellAppearance(position.X, position.Y, targetCell);

                var blight = BlightMap.GetItem(position);
                Fade fade = null;
                if (blight?.Extent > 0)
                    fade = GetFadeForBlightExtent(blight.Extent, rawCell.Background);
                EffectsManager.SetEffect(console.Cells[position.Y * Width + position.X], fade);
            }
        }

        //0.2  review for wasteful object creation
        public Fade GetFadeForBlightExtent(int extent, Color bgColor)
        {
            var rand = new Random();

            if (extent < 1)
            {
                return null;
            }
            else if (extent < 3)
            {
                var lowColors = new Color[] { bgColor, new Color(11, 44, 0), bgColor, new Color(23, 22, 17), bgColor, new Color(8, 28, 11), bgColor };
                //var lowColors = new Color[] { bgColor, new Color(31, 64, 0), bgColor, new Color(47, 45, 26), bgColor, new Color(16, 57, 52), bgColor };
                var lowFade = new Fade
                {
                    FadeBackground = true,
                    DestinationBackground = new ColorGradient(lowColors),
                    FadeDuration = 4 + rand.NextDouble() * 2,
                    Repeat = true,
                    UseCellBackground = false,
                };
                return lowFade;
            }
            else if (extent < 6)
            {
                var midColors = new Color[] { bgColor, new Color(31, 64, 0), bgColor, new Color(47, 45, 26), bgColor, new Color(16, 57, 52), bgColor };
                //var midColors = new Color[] { bgColor, new Color(63, 128, 0), new Color(94, 91, 53), new Color(32, 112, 104), bgColor };
                var midFade = new Fade
                {
                    FadeBackground = true,
                    DestinationBackground = new ColorGradient(midColors),
                    FadeDuration = 2.3 + rand.NextDouble() * 1.1,
                    Repeat = true,
                    UseCellBackground = true
                };
                return midFade;
            }
            else
            {
                var highColors = new Color[] { bgColor, new Color(63, 128, 0), new Color(94, 91, 53), new Color(32, 112, 104), bgColor };
                var highFade = new Fade
                {
                    FadeBackground = true,
                    DestinationBackground = new ColorGradient(highColors),
                    FadeDuration = 1.3 + rand.NextDouble() * .7,
                    Repeat = true,
                    UseCellBackground = true
                };
                return highFade;
            }
        }
    }
}
