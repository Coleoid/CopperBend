using CopperBend.Contract;
using GoRogue;
using System.Collections.Generic;

namespace CopperBend.Fabric
{
    public class BlightMap : SpatialMap<IAreaBlight>, IBlightMap
    {
        public IEnumerable<IAreaBlight> GetItems(IEnumerable<Coord> coords)
        {
            foreach (var coord in coords)
            {
                var item = GetItem(coord);
                if (item != null) yield return item;
            }
        }
    }
}
