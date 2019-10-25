using GoRogue;
using System.Collections.Generic;

namespace CopperBend.Contract
{
    public interface IBlightMap : ISpatialMap<IAreaBlight>, IReadOnlySpatialMap<IAreaBlight>
    {
        // Some parts of AdvancedSpatialMap aren't in ISaptialMap, so I patch them in
        IAreaBlight GetItem(Coord position);
        IAreaBlight GetItem(int X, int Y);

        // This one Just Makes Sense
        IEnumerable<IAreaBlight> GetItems(IEnumerable<Coord> coords);
    }
}
