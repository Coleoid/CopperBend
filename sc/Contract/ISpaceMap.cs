using System.Collections.Generic;
using GoRogue;

namespace CopperBend.Contract
{
    public interface ISpaceMap
    {
        int Height { get; set; }
        Coord PlayerStartPoint { get; set; }
        int Width { get; set; }

        bool CanPlant(Coord position);
        bool CanSeeThrough(Coord position);
        bool CanWalkThrough(Coord position);

        ISpace GetItem(Coord coord);
        //void Sow(ISpace space, ISeed seedToSow);
        void MarkSpaceSown(ISpace space);
        void Till(ISpace space);
        bool OpenDoor(ISpace space);
        void SeeCoords(IEnumerable<Coord> newlySeen);
        void AddItem(ISpace space, Coord coord);
    }
}
