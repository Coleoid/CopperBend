using System;
using System.Collections.Generic;
using CopperBend.Model;
using GoRogue;
using Microsoft.Xna.Framework;
using SadConsole;
using Rectangle = GoRogue.Rectangle;

namespace CopperBend.Contract
{
    public interface ISmallMap
    {
        MultiSpatialMap<CbEntity> Entities { get; set; }
        int Height { get; set; }
        Point PlayerStartPoint { get; set; }
        //TileBase[] Tiles { get; set; }
        int Width { get; set; }

        void Add(CbEntity entity);
        IEnumerable<T> GetEntitiesAt<T>(Point location) where T : CbEntity;
        bool IsTileWalkable(Point location);
        void Remove(CbEntity entity);
        //TileBase GetTileAt(Point newPoint);
    }
}
