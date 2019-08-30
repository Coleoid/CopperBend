using GoRogue;
using System.Collections.Generic;

namespace CopperBend.Contract
{
    public interface IItemMap
    {
        string MyName { get; set; }
        IEnumerable<IItem> GetItems(Coord coord);
        bool Add(IItem item, Coord coord);
        bool Remove(IItem item);
    }
}
