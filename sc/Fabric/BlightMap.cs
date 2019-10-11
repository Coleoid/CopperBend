using CopperBend.Contract;

namespace CopperBend.Fabric
{
    public class BlightMap : SerializableSpatialMap<IAreaBlight>, IBlightMap
    {
        public string Name { get; set; }

        public BlightMap()
            : base()
        {
        }
    }
}
