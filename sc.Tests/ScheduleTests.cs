﻿using System;
using System.Collections.Generic;
using CopperBend.Contract;
using CopperBend.Model;
using log4net;
using log4net.Config;
using log4net.Repository;
using NUnit.Framework;

namespace CopperBend.Engine.tests
{

    [TestFixture]
    public class ScheduleTests
    {
        private Schedule schedule;
        private IControlPanel nullControlPanel;

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

            Engine.InitializeIDGenerator();
            Engine.InitializePlantRepos();
        }

        [SetUp]
        public void SetUp()
        {
            schedule = new Schedule();
            nullControlPanel = null;
        }

        [Test]
        public void Empty_Schedule_GetNext_should_never_happen()
        {
            var ex = Assert.Throws<Exception>(() => schedule.GetNextAction());
            Assert.That(ex.Message, Contains.Substring("should never"));
        }

        [Test]
        public void Local_sub_or_lambda_to_close_over_more_data()
        {
            var targets = new List<Being>();
            IBeing passedActor = null;
            List<Being> passedTargets = null;

            void check_actor_and_targets(IControlPanel cp, IBeing argActor, List<Being> argTargets)
            {
                passedActor = argActor;
                passedTargets = argTargets;
            }

            Action<IControlPanel> wrapper = (IControlPanel icp) =>
                check_actor_and_targets(icp, null, targets);

            schedule.AddEntry(new ScheduleEntry{Action = wrapper, Agent = null, Offset = 2});
            var nextAction = schedule.GetNextAction();
            nextAction(nullControlPanel);

            Assert.That(passedActor, Is.Null);
            Assert.That(passedTargets, Is.SameAs(targets));
        }

    }
}
