using System.Collections.Generic;
using CopperBend.MapUtil;
using RLNET;

namespace CopperBend.App
{
    public interface IAreaMap : IMap
    {
        string Name { get; set; }
        ITile[,] Tiles { get; set; }
        Dictionary<string, TileType> TileTypes { get; set; }
        List<IItem> Items { get; set; }
        List<IActor> Actors { get; set; }
        IActor ViewpointActor { get; set; }
        bool DisplayDirty { get; set; }

        ITile this[Point point] { get; }

        bool MoveActor(IActor actor, Point point);
        void UpdatePlayerFieldOfView(IActor player);
        void DrawMap(RLConsole mapConsole);
        IActor GetActorAtPoint(Point point);
        List<string> FirstSightMessages { get; set; }
        Dictionary<Point, List<string>> LocationMessages { get; }
        Point PlayerStartsAt { get; set; }

        void OpenDoor(ITile tile);
        bool HasEventAtPoint(Point point);
        void RunEvent(IActor player, ITile tile, IControlPanel controls);
        void AddEventAtLocation(Point point);
    }
}
