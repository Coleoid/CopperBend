using System;
using System.Collections.Generic;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;

namespace CopperBend.Model
{
    public class AreaRot : IHasID, IDelible, IAreaRot
    {
        public string Name { get => "Area rot"; set { } }

        public AreaRot()
            : this(uint.MaxValue) { }
        public AreaRot(uint id)
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

        public int Health { get; set; }

        #region IDelible
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
            var dm = new DefenseMethod();
            dm.Resistances.Add("physical", "1/3 +2 ..6");
            dm.Resistances.Add("default", "1/4 +1 ..6");

            return dm;
        }

        public List<IModifier> GetDefenseModifiers(IAttacker attacker, IAttackMethod method)
        {
            throw new NotImplementedException();
        }

    }
}
