using System.Collections.Generic;
using CopperBend.Contract;
using GoRogue;

namespace CopperBend.Fabric
{
    public class BlightMap : SpatialMap<IAreaBlight>, IBlightMap
    {
        public IEnumerable<IAreaBlight> GetNonNullItems(IEnumerable<Coord> coords)
        {
            foreach (var coord in coords)
            {
                var item = GetItem(coord);
                if (item != null) yield return item;
            }
        }
    }
}
