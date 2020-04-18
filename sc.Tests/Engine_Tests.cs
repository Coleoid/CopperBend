﻿using CopperBend.Fabric.Tests;
using CopperBend.Model;
using NUnit.Framework;

namespace CopperBend.Logic.Tests
{
    [TestFixture]
    public class Engine_Tests : Tests_Base
    {
        [Test]
        public void Cosmogenesis_sets_ID_generator_service()
        {
            Item.SetIDGenerator(null);
            AreaRot.SetIDGenerator(null);

            var sef = StubEntityFactory();

            Engine.Cosmogenesis("seed", sef);

            var item = new Item((2, 2));
            var rot = new AreaRot();

            Assert.That(item, Is.Not.Null);
            Assert.That(rot, Is.Not.Null);
            Assert.That(item.ID, Is.EqualTo(rot.ID - 1));
        }

        //[Test]
        //public void Can_init_plant_repos()
        //{
        //    Seed.Herbal = null;
        //    var herbal = Engine.InitHerbal();
        //    Engine.ConnectHerbal(herbal);
        //    Assert.That(Seed.Herbal, Is.SameAs(herbal));
        //}
    }
}
