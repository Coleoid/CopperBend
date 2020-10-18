using Microsoft.Xna.Framework;
using CopperBend.Contract;
using NUnit.Framework;
using CopperBend.Creation;
using CopperBend.Model;
using CopperBend.Fabric;

namespace CopperBend.Logic.Tests
{
    [TestFixture]
    public class Casting_Tests : Tests_Base
    {
        protected override bool ShouldPrepDI => true;
        protected override MockableServices GetServicesToMock()
        {
            return MockableServices.Log
                | MockableServices.EntityFactory
                | base.GetServicesToMock();
        }


        private Director director;

        [SetUp]
        public void SetUp()
        {
            Basis.ConnectIDGenerator();
            director = SourceMe.The<Director>();
        }

        [Test]
        public void Can_cast_new_being()
        {
            IBeing newRat = director.BuildNewBeing("flame rat");

            Assert.That(newRat, Is.Not.Null);
            Assert.That(newRat.Foreground, Is.EqualTo(Color.Red));
        }

        [Test]
        public void Can_cast_NPC()
        {
            var reg = SourceMe.The<SocialRegister>();
            reg.LoadRegister();
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
