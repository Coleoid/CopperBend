﻿using NSubstitute;
using NUnit.Framework;
using SadConsole.Entities;
using CopperBend.Model;
using CopperBend.Contract;
using Microsoft.Xna.Framework;

namespace CopperBend.Engine.Tests
{
    [TestFixture]
    public class Casting_Tests
    {
        [SetUp]
        public void SetUp()
        {
            var entityFactory = Substitute.For<ISadConEntityFactory>();
            entityFactory.GetSadCon(Arg.Any<ISadConInitData>())
                .Returns(Substitute.For<IEntity>());
            Being.SadConEntityFactory = entityFactory;

            Engine.ConnectIDGenerator(new GoRogue.IDGenerator());
        }

        [Test]
        public void Can_cast_new_being()
        {
            var director = new Director();

            IBeing newRat = director.BuildNewBeing("rat");

            Assert.That(newRat, Is.Not.Null);
            Assert.That(newRat.Foreground, Is.EqualTo(Color.Purple));  // I've seen odder things
        }

        [Test]
        public void Can_cast_NPC()
        {

        }

        [Test]
        public void Can_cast_player()
        {

        }
    }
}
