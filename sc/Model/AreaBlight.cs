using System;
using GoRogue;
using CopperBend.Contract;
using System.Collections.Generic;
using CopperBend.Fabric;

namespace CopperBend.Model
{
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

        public int Health { get; set; }

        #region IDestroyable
        public int MaxHealth { get; set; } = 80;

        public void Heal(int amount)
        {
            Guard.Against(amount < 0, $"Cannot heal negative amount {amount}.");
            Health = Math.Min(MaxHealth, Health + amount);
        }

        public void Hurt(int amount)
        {
            Guard.Against(amount < 0, $"Cannot hurt negative amount {amount}.");
            Health = Math.Max(0, Health - amount);
        }
        #endregion


        //  IAttacker
        public IAttackMethod GetAttackMethod(IDefender defender)
        {
            throw new NotImplementedException();
        }
        public List<IModifier> GetAttackModifiers(IDefender defender, IAttackMethod method)
        {
            throw new NotImplementedException();
        }

        //  IDefender
        public IDefenseMethod GetDefenseMethod(IAttackMethod method)
        {
            throw new NotImplementedException();
        }
        public List<IModifier> GetDefenseModifiers(IAttacker attacker, IAttackMethod method)
        {
            throw new NotImplementedException();
        }

    }
}
