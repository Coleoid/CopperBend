using CopperBend.Contract;

namespace CopperBend.Fabric
{
    public class BlightMap : SerializableSpatialMap<IAreaBlight>, IBlightMap
    {
        public string Name { get; set; }

        //0.0  the arguments sidestep a Json.net deserializing bug:
        //  it won't reload SerialItems when creating with default ctor
        public BlightMap(int dummy)
            : base()
        {
        }
    }
}
