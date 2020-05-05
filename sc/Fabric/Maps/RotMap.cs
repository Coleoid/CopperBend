using System.Collections.Generic;
using CopperBend.Contract;
using GoRogue;

namespace CopperBend.Fabric
{
    public class RotMap : SpatialMap<IAreaRot>, IRotMap
    {
        public IEnumerable<IAreaRot> GetNonNullItems(IEnumerable<Coord> coords)
        {
            foreach (var coord in coords)
            {
                var item = GetItem(coord);
                if (item != null) yield return item;
            }
        }
    }
}
