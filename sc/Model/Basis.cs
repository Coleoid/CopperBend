using GoRogue;

namespace CopperBend.Model
{
    public static class Basis
    {
        /// <summary> Sets IDGemerator on CBEntity, Item, Space, and AreaRot </summary>
        /// <param name="gen"> Optional prebuilt IDGenerator to connect </param>
        /// <returns> The IDGenerator connected to the ID'ing types </returns>
        public static IDGenerator ConnectIDGenerator(IDGenerator gen = null)
        {
            if (gen == null) gen = new IDGenerator();

            CbEntity.SetIDGenerator(gen);
            Item.SetIDGenerator(gen);
            Fabric.Space.SetIDGenerator(gen);
            AreaRot.SetIDGenerator(gen);

            return gen;
        }
    }
}
