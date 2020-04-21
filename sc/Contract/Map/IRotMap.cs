using System.Collections.Generic;
using GoRogue;

namespace CopperBend.Contract
{
    public interface IRotMap : ISpatialMap<IAreaRot>, IReadOnlySpatialMap<IAreaRot>
    {
        // Some parts of AdvancedSpatialMap aren't in ISaptialMap, so I patch them in
        IAreaRot GetItem(Coord position);
        IAreaRot GetItem(int x, int y);

        // This one Just Makes Sense
        IEnumerable<IAreaRot> GetNonNullItems(IEnumerable<Coord> coords);
    }
}
