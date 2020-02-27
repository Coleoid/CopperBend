using NUnit.Framework;
using SadConsole.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CopperBend.Engine.Tests
{
    [TestFixture]
    public class Casting_Tests
    {
        [Test]
        public void Can_cast_new_entity()
        {
            var director = new Director();
            //IEntity newRat = null;
            //IEntity newRat = director.GetCast().GetNewRat();  //inprog
            IEntity newRat = director.Cast["newRat"];  //inprog

            Assert.That(newRat, Is.Not.Null);
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
