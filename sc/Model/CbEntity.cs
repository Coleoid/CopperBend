using SadConsole.Entities;
using GoRogue;

namespace CopperBend.Model
{
    public abstract class CbEntity : IHasID
    {
        #region standard IHasID
        public static IDGenerator IDGenerator { get; set; }
        public uint ID { get; private set; }
        #endregion

        public IEntity SadConEntity { get; set; } = null;

        protected CbEntity(uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
        }
    }
}
