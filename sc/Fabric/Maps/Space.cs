using GoRogue;
using CopperBend.Contract;
using YamlDotNet.Serialization;

namespace CopperBend.Fabric
{
    public class Space : IHasID, ISpace
    {
        public Space()
        {
            ID = IDGenerator.UseID();
        }

        public Space(uint id)
        {
            ID = id;
        }

        #region My IHasID
        public static void SetIDGenerator(IDGenerator generator)
        {
            IDGenerator = generator;
        }
        private static IDGenerator IDGenerator { get; set; }
        public uint ID { get; private set; }
        #endregion

        //public int Elevation;  //for later movement/attack mod
        public Terrain Terrain { get; set; }

        //0.2.MAP  check for modifiers (smoke, dust, giant creature, ...)
        [YamlIgnore]
        public bool CanSeeThrough => Terrain.CanSeeThrough;
        [YamlIgnore]
        public bool CanWalkThrough => Terrain.CanWalkThrough;

        //0.2.MAP  check for modifiers (permission, hostile aura, rot, ...)
        [YamlIgnore]
        public bool CanPlant => Terrain.CanPlant && IsTilled && !IsSown;
        [YamlIgnore]
        public bool CanTill => Terrain.CanPlant && !IsTilled;

        public bool IsTilled { get; set; }
        public bool IsSown { get; set; }
        public bool IsKnown { get; set; }
    }
}
