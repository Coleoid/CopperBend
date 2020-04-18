using CopperBend.Fabric;
using CopperBend.Model;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CopperBend.Logic.Tests
{
    [TestFixture]
    public class Splashback_Tests : AttackSystem_TestBase
    {
        [TestCase("physical.impact.point", true)]
        [TestCase("physical.impact.blunt", true)]
        [TestCase("energetic.fire", false)]
        public void Physical_impact_on_Rot_causes_splashback_damage(string damageType, bool willSplashBack)
        {
            // Anyone directly physically assaulting rot is 
            // hit with immediate vital.rot.toxin damage.
            var asys = new AttackSystem(null, __log, null);

            var flameRat = BeingCreator.CreateBeing("flame rat");
            var am = new AttackMethod(damageType, "1d3 +2");
            var rot = new AreaRot();
            Attack attack = new Attack
            {
                Attacker = flameRat,
                AttackMethod = am,
                Defender = rot,
                DefenseMethod = rot.GetDefenseMethod(am)
            };

            Assert.That(asys.AttackQueue.Count, Is.EqualTo(0));

            asys.CheckForSpecials(attack);

            int newAttackCount = willSplashBack ? 1 : 0;
            Assert.That(asys.AttackQueue.Count, Is.EqualTo(newAttackCount));
            if (!willSplashBack) return;

            var newAttack = asys.AttackQueue.Dequeue();
            var newAM = newAttack.AttackMethod;
            var newAE = newAM.AttackEffects[0];
            Assert.That(newAE.Type, Is.EqualTo("vital.rot.toxin"));
            Assert.That(newAttack.Defender, Is.EqualTo(flameRat));
            Assert.That(newAttack.Attacker, Is.EqualTo(rot));
        }

        [Test, Ignore("Need to decide how to handle ranged damage")]
        public void Ranged_physical_damage_on_Rot_skips_splashback_damage()
        {
            var asys = new AttackSystem(null, __log, null);

            var flameRat = BeingCreator.CreateBeing("flame rat");
            var am = new AttackMethod("physical.impact.point", "1d3 +2");
            var rot = new AreaRot();

            flameRat.Position = new Point(3, 3);
            //rot.Position = new Point(8, 8);  //  Not a thing.
            var rotmap = new RotMap();
            rotmap.Add(rot, new GoRogue.Coord(8, 8));

            Attack attack = new Attack
            {
                Attacker = flameRat,
                AttackMethod = am,
                Defender = rot,
                DefenseMethod = rot.GetDefenseMethod(am)
            };

            Assert.That(asys.AttackQueue.Count, Is.EqualTo(0));

            asys.CheckForSpecials(attack);

            Assert.That(asys.AttackQueue.Count, Is.EqualTo(0),
                "Because of distance, rot should not splashback attack.");
        }

        //TODO: Missed_physical_impact_on_Rot_skips_splashback_damage
    }
}
