using System.Collections.Generic;
using GoRogue;

namespace CopperBend.Contract
{
    public interface IRotMap : ISpatialMap<IAreaRot>, IReadOnlySpatialMap<IAreaRot>
    {
        // This part of AdvancedSpatialMap isn't in ISaptialMap, so I patch it in
        IAreaRot GetItem(Coord position);

        // This one Just Makes Sense
        IEnumerable<IAreaRot> GetNonNullItems(IEnumerable<Coord> coords);
    }
}
