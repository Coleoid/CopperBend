using GoRogue;
using CopperBend.Contract;

namespace CopperBend.Fabric
{
    public class ItemMap : MultiSpatialMap<IItem>, IItemMap
    {
        // Experiment.  The default serialization doesn't save properties
        // added to this class, probably because it derives from IEnumerable.
        public string MyName { get; set; }
    }
}
