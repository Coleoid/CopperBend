using System;
using System.Collections.Generic;
using CopperBend.App.Model;
using CopperBend.MapUtil;
using NSubstitute;
using NUnit.Framework;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class ScheduleTests
    {
        private Schedule schedule;
        private IControlPanel nullControlPanel;

        [SetUp]
        public void SetUp()
        {
            schedule = new Schedule();
            nullControlPanel = null;
        }

        [Test]
        public void Schedule_Add_null_Entry_throws_clear()
        {
            var ex = Assert.Throws<Exception>(() => schedule.Add(null, 0));
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
            var actor = new Actor(new Point(0, 0));
            var targets = new List<Actor>();
            IActor passedActor = null;
            List<Actor> passedTargets = null;

            void check_actor_and_targets(IControlPanel cp, IActor argActor, List<Actor> argTargets)
            {
                passedActor = argActor;
                passedTargets = argTargets;
            }

            Action<IControlPanel> wrapper = (IControlPanel icp) =>
                check_actor_and_targets(icp, actor, targets);

            schedule.Add(wrapper, 2);
            var nextAction = schedule.GetNextAction();
            nextAction(nullControlPanel);

            Assert.That(passedActor, Is.SameAs(actor));
            Assert.That(passedTargets, Is.SameAs(targets));
        }

    }
}
