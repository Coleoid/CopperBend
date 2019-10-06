using System;
using log4net;
using log4net.Config;
using log4net.Repository;
using NUnit.Framework;

namespace CopperBend.Engine.Tests
{

    [TestFixture]
    public class Schedule_Tests
    {
        private Schedule schedule;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            bool foundRepo = false;
            var repos = LogManager.GetAllRepositories();
            foreach (var r in repos)
            {
                if (r.Name == "CB") foundRepo = true;
            }

            ILoggerRepository repo = null;
            if (foundRepo)
            {
                repo = LogManager.GetRepository("CB");
            }
            else
            {
                repo = LogManager.CreateRepository("CB");
                BasicConfigurator.Configure(repo);
            }
            // nfw this should be this difficult.

            var log = LogManager.GetLogger("CB", "CB");

            Engine.ConnectIDGenerator();
            Engine.ConnectHerbal();
        }

        [SetUp]
        public void SetUp()
        {
            schedule = new Schedule();
        }

        [Test]
        public void Empty_Schedule_GetNext_should_never_happen()
        {
            var ex = Assert.Throws<Exception>(() => schedule.GetNextAction());
            Assert.That(ex.Message, Contains.Substring("should never"));
        }
    }
}
