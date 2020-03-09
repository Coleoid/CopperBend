﻿using NSubstitute;
using NUnit.Framework;
using SadConsole.Entities;
using CopperBend.Model;
using CopperBend.Contract;
using Microsoft.Xna.Framework;

namespace CopperBend.Logic.Tests
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

            Engine.Cosmogenesis("casting", entityFactory);
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

        }

        [Test]
        public void Can_cast_player()
        {

        }
    }
}
