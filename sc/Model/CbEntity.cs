using GoRogue;

namespace CopperBend.Model
{
    public abstract class CbEntity : IHasID
    {
        protected internal SadConsole.Entities.IEntity ScEntity = null;

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
