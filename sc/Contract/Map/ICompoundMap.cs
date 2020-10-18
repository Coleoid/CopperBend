using System.Collections.Generic;
using SadConsole;
using SadConsole.Effects;
using GoRogue;
using GoRogue.MapViews;

namespace CopperBend.Contract
{
    public interface ICompoundMap : ITriggerHolder
    {
        string Name { get; }
        int Width { get; }
        int Height { get; }

        ISpaceMap SpaceMap { get; }
        IRotMap RotMap { get; }
        IBeingMap BeingMap { get; }
        IItemMap ItemMap { get; }

        //TODO:  A collection of special areas on this map
        List<Trigger> Triggers { get; }


        FOV FOV { get; set; }
        /// <summary> When a space changes its CanSeeThrough, set this flag true </summary>
        bool VisibilityChanged { get; set; }
        /// <summary> When a space's appearance (may) change, add its coords to this list </summary>
        List<Coord> CoordsWithChanges { get; }

        bool IsWithinMap(Coord position);
        bool CanSeeThrough(Coord position);
        bool CanWalkThrough(Coord position);
        /// <summary> Can Plant considering terrain, rot, existing plants, and ownership. </summary>
        bool CanPlant(Coord position);

        IMapView<bool> GetView_CanSeeThrough();
        IMapView<bool> GetView_CanWalkThrough();
        IMapView<int> GetView_RotStrength();

        EffectsManager EffectsManager { get; }

        void SetInitialConsoleCells(ScrollingConsole console, ISpaceMap spaceMap);
        void UpdateFOV(ScrollingConsole console, Coord position);
        void UpdateViewOfCoords(ScrollingConsole console, IEnumerable<Coord> coords);
    }
}
