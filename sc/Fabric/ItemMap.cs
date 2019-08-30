using GoRogue;
using Newtonsoft.Json;
using CopperBend.Contract;
using CopperBend.Persist;

namespace CopperBend.Fabric
{
    [JsonConverter(typeof(ItemMapConverter))]
    public class ItemMap : MultiSpatialMap<IItem>, IItemMap
    {
        // Experiment.  The default serialization doesn't save properties
        // added to this class, probably because it derives from IEnumerable.
        public string MyName { get; set; }
    }
}
