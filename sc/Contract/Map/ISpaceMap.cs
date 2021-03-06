﻿using System.Collections.Generic;
using GoRogue;

namespace CopperBend.Contract
{
    public interface ISpaceMap : ISpatialMap<ISpace>
    {
        int Height { get; set; }
        int Width { get; set; }
        Coord PlayerStartPoint { get; set; }

        bool CanPlant(Coord position);
        bool CanSeeThrough(Coord position);
        bool CanWalkThrough(Coord position);

        ISpace GetItem(Coord coord);

        void SeeCoords(IEnumerable<Coord> newlySeen);
    }
}
