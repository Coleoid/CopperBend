using GoRogue;
using CopperBend.Contract;

namespace CopperBend.Fabric
{
    public class Space : IHasID, ISpace
    {
        public Space(uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
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
        public bool CanSeeThrough => Terrain.CanSeeThrough;
        public bool CanWalkThrough => Terrain.CanWalkThrough;

        //0.2.MAP  check for modifiers (permission, hostile aura, rot, ...)
        public bool CanPlant => Terrain.CanPlant && IsTilled && !IsSown;
        public bool CanTill => Terrain.CanPlant && !IsTilled;

        public bool IsTilled { get; set; }
        public bool IsSown { get; set; }
        public bool IsKnown { get; set; }
    }
}
