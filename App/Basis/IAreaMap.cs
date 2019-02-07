﻿using System.Collections.Generic;
using CopperBend.MapUtil;
using RLNET;

namespace CopperBend.App
{
    public interface IAreaMap : IMap
    {
        string Name { get; set; }
        ITile this[Point point] { get; }
        ITile[,] Tiles { get; set; }
        Dictionary<string, TileType> TileTypes { get; set; }
        bool IsDisplayDirty { get; set; }

        Dictionary<Point, List<string>> LocationMessages { get; }
        //Dictionary<Point, List<CommandEntry>> LocationEventEntries { get; }
        bool HasEventAtPoint(Point point);
        //void AddEventAtLocation(Point point, CommandEntry entry);
        List<string> FirstSightMessage { get; set; }

        List<IItem> Items { get; set; }
        List<IActor> Actors { get; set; }
        IActor ViewpointActor { get; set; }
        Point PlayerStartsAt { get; set; }
        //  0.1.  0.2 ~ how we came from our prior location affects start point

        //  These likely leave AreaMap
        bool MoveActor(IActor actor, Point point);
        void UpdatePlayerFieldOfView(IActor player);
        void DrawMap(RLConsole mapConsole);
        IActor GetActorAtPoint(Point point);
        void OpenDoor(ITile tile);
    }
}
