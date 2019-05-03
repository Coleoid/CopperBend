using CopperBend.Model;
using Microsoft.Xna.Framework;
using SadConsole;
using System.Collections.Generic;

namespace CopperBend.Contract
{
    public interface IAreaMap : IMap
    {
        string Name { get; set; }
        ITile this[Point point] { get; }
        ITile[,] Tiles { get; set; }
        Dictionary<string, TileType> TileTypes { get; set; }
        bool IsDisplayDirty { get; set; }

        void SetTile(ITile tile);

        Dictionary<Point, List<string>> LocationMessages { get; }
        //Dictionary<Point, List<CommandEntry>> LocationEventEntries { get; }
        bool HasEventAtPoint(Point point);
        //void AddEventAtLocation(Point point, CommandEntry entry);
        List<string> FirstSightMessage { get; set; }

        List<IItem> Items { get; set; }
        List<IBeing> Actors { get; set; }
        IBeing ViewpointBeing { get; set; }
        Point PlayerStartsAt { get; set; }
        //  0.1.  0.2 ~ how we came from our prior location affects start point

        //  These likely leave AreaMap
        void MoveActor(IBeing being, Point point);
        void UpdatePlayerFieldOfView(IBeing player);
        void DrawMap(Console mapConsole);
        IBeing GetActorAtPoint(Point point);
        void OpenDoor(ITile tile);
    }
}
