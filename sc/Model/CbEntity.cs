using System;
using SadConsole.Entities;
using GoRogue;
using YamlDotNet.Serialization;

namespace CopperBend.Model
{
    public abstract class CbEntity : IHasID
    {
        #region My IHasID
        public static void SetIDGenerator(IDGenerator generator)
        {
            IDGenerator = generator;
        }
        private static IDGenerator IDGenerator { get; set; }
        public uint ID { get; set; }
        #endregion

        [YamlIgnore]
        public IEntity SadConEntity { get; set; } = null;

        protected CbEntity(uint id = uint.MaxValue)
        {
            if (IDGenerator == null) throw new Exception("need CBEntity.SetIDGenerator() call.");
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
        }
    }
}
