using System.Collections.Generic;
using GoRogue;

namespace CopperBend.Contract
{
    public interface IBlightMap : ISpatialMap<IAreaBlight>, IReadOnlySpatialMap<IAreaBlight>
    {
        // Some parts of AdvancedSpatialMap aren't in ISaptialMap, so I patch them in
        IAreaBlight GetItem(Coord position);
        IAreaBlight GetItem(int x, int y);

        // This one Just Makes Sense
        IEnumerable<IAreaBlight> GetNonNullItems(IEnumerable<Coord> coords);
    }
}
