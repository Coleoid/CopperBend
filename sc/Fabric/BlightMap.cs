using CopperBend.Contract;
using GoRogue;
using Newtonsoft.Json;
using System;

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

    public class AreaBlight : IHasID, IDestroyable, IAreaBlight
    {
        public AreaBlight(uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
        }

        #region standard IHasID
        public static IDGenerator IDGenerator;
        public uint ID { get; private set; }
        #endregion

        public int Extent { get; set; }

        #region IDestroyable
        public int MaxHealth { get; set; } = 80;

        [JsonIgnore]
        public int Health => Extent;

        public void Heal(int amount)
        {
            Guard.Against(amount < 0, $"Cannot heal negative amount {amount}.");
            Extent = Math.Min(MaxHealth, Extent + amount);
        }

        public void Hurt(int amount)
        {
            Guard.Against(amount < 0, $"Cannot hurt negative amount {amount}.");
            Extent = Math.Max(0, Extent - amount);
        }
        #endregion
    }
}
