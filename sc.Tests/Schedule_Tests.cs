using System;
using log4net;
using NSubstitute;
using NUnit.Framework;

namespace CopperBend.Engine.Tests
{

    [TestFixture]
    public class Schedule_Tests
    {
        private Schedule schedule;
        private ILog __log;
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            __log = Substitute.For<ILog>();
            Engine.Cosmogenesis("monobloc delenda est!");
        }

        [SetUp]
        public void SetUp()
        {
            schedule = new Schedule(__log);
        }

        [Test]
        public void Empty_Schedule_GetNext_should_never_happen()
        {
            var ex = Assert.Throws<Exception>(() => schedule.GetNextAction());
            Assert.That(ex.Message, Contains.Substring("should never"));
        }
    }
}
