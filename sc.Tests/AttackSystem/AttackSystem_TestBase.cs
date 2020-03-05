using CopperBend.Contract;
using CopperBend.Model;
using log4net;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CopperBend.Engine.Tests
{
    [TestFixture]
    public class AttackSystem_TestBase
    {
        public AttackMethod tac;
        public AttackEffect tac_impact;
        public AttackEffect tac_flame;

        public AttackMethod bfh;
        public AttackEffect bfh_impact;
        public AttackEffect bfh_flame;

        public DefenseMethod leather_armor;
        public DefenseMethod ring_armor;

        public ILog __log;

        public void Prepare_game_entity_creation()
        {
            Engine.Cosmogenesis("attack!");
            Being.SadConEntityFactory = Substitute.For<ISadConEntityFactory>();
        }

        [SetUp]
        public void SetUp()
        {
            __log = Substitute.For<ILog>();
            Prepare_game_entity_creation();

            // Torch as club, crunch and burn
            tac = new AttackMethod();
            tac_impact = new AttackEffect
            {
                Type = "physical.impact.blunt",
                DamageRange = "1d5"
            };
            tac_flame = new AttackEffect
            {
                Type = "energetic.fire",
                DamageRange = "1d3 - 1"
            };
            tac.AttackEffects.Add(tac_impact);
            tac.AttackEffects.Add(tac_flame);

            // Brekka-onu's Flame Hammer, bigger crunch, bigger burn
            bfh = new AttackMethod();
            bfh_impact = new AttackEffect
            {
                Type = "physical.impact.blunt",
                DamageRange = "2d6 + 4"
            };
            bfh_flame = new AttackEffect
            {
                Type = "energetic.fire",
                DamageRange = "1d4 + 2"
            };
            bfh.AttackEffects.Add(bfh_impact);
            bfh.AttackEffects.Add(bfh_flame);

            leather_armor = new DefenseMethod();
            leather_armor.Resistances["physical.impact.blunt"] = "1/4 ..4";
            leather_armor.Resistances["physical"] = "1/2 ..4";
            leather_armor.Resistances["energetic"] = "2/3 ..4";
            leather_armor.Resistances["magical"] = "1/3 ..1";
            leather_armor.Resistances["vital"] = "1/3 ..2";
            //leather_armor.Resistances["default"] = "1/3 ..3";  //not needed with all branches covered

            ring_armor = new DefenseMethod();
            ring_armor.Resistances["physical.impact.blunt"] = "1/2 ..6";
            ring_armor.Resistances["physical"] = "2/3 ..8";
            ring_armor.Resistances["energetic.fire"] = "2/3 ..5";
            ring_armor.Resistances["default"] = "1/2 ..5";

            //0.2: Keep the tree of damage types in data, and type-check attacks/defenses at load time...
        }
    }
}
