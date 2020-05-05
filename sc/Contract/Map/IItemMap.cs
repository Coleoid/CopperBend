using System.Collections.Generic;
using GoRogue;

namespace CopperBend.Contract
{
    public interface IItemMap
    {
        IEnumerable<IItem> GetItems(Coord coord);
        bool Add(IItem item, Coord coord);
        bool Remove(IItem item);
    }
}
