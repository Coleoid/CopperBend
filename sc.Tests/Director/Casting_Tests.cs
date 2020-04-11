using NUnit.Framework;
using CopperBend.Contract;
using Microsoft.Xna.Framework;
using CopperBend.Fabric.Tests;

namespace CopperBend.Logic.Tests
{
    [TestFixture]
    public class Casting_Tests
    {
        [SetUp]
        public void SetUp()
        {
            var entityFactory = UTHelp.GetSubstituteFactory();

            Engine.Cosmogenesis("casting", entityFactory);
            //var ourHero = Substitute.For<IBeing>();
            //Engine.Compendium.SocialRegister.LoadRegister(ourHero);
        }

        [Test]
        public void Can_cast_new_being()
        {
            var director = new Director();

            IBeing newRat = director.BuildNewBeing("flame rat");

            Assert.That(newRat, Is.Not.Null);
            Assert.That(newRat.Foreground, Is.EqualTo(Color.Red));
        }

        [Test]
        public void Can_cast_NPC()
        {
            var director = new Director();

            IBeing kellet = director.FindBeing("Kellet Benison");

            Assert.That(kellet, Is.Not.Null);
            Assert.That(kellet.Foreground, Is.EqualTo(Color.AliceBlue));
        }

        [Test]
        public void Can_cast_player()
        {

        }
    }
}
