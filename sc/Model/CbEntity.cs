using SadConsole.Entities;
using GoRogue;

namespace CopperBend.Model
{
    public abstract class CbEntity : IHasID
    {
        public IEntity ScEntity { get; set; } = null;

        protected CbEntity(uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue? IDGenerator.UseID() : id);
        }

        #region standard IHasID
        public static IDGenerator IDGenerator;
        public uint ID { get; private set; }
        #endregion
    }
}
