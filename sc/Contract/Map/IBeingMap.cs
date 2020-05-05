using System.Collections.Generic;
using GoRogue;

namespace CopperBend.Contract
{
    public interface IBeingMap
    {
        IEnumerable<IBeing> GetItems(Coord coord);
        bool Add(IBeing item, Coord coord);
        bool Remove(IBeing item);
        bool Move(IBeing item, Coord coord);
        Coord GetPosition(IBeing item);
    }
}
