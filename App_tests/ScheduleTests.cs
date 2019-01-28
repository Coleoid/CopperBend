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
        public void Schedule_calls_action_in_entry()
        {
            bool calledFromSchedule = false;

            void call(IControlPanel cp, ScheduleEntry se) { calledFromSchedule = true; }
            var entry = new ScheduleEntry(3, call);

            schedule.Add(entry);
            schedule.DoNext(nullControlPanel);

            Assert.That(calledFromSchedule);
        }

        [Test]
        public void ScheduleEntry_with_null_action_throws_clear()
        {
            Action<IControlPanel, ScheduleEntry> nullCall = null;
            var ex = Assert.Throws<Exception>(() => new ScheduleEntry(3, nullCall));
            //  This is nearly the stupidest test, yet it found a bug.
        }

        [Test]
        public void Schedule_Add_null_Entry_throws_clear()
        {
            var ex = Assert.Throws<Exception>(() => schedule.Add(null));
        }

        [Test]
        public void Empty_Schedule_GetNext_throws_clear()
        {
            schedule.GetNext();
        }

        [Test]
        public void Schedule_passes_ControlPanel_and_ScheduleEntry_to_action()
        {
            var icp = Substitute.For<IControlPanel>();
            ScheduleEntry receivedSE = null;

            void write_foo(IControlPanel cp, ScheduleEntry se)
            {
                cp.WriteLine("foo");
                receivedSE = se;
            }

            var entry = new ScheduleEntry(3, write_foo);

            schedule.Add(entry);
            schedule.DoNext(icp);

            icp.Received().WriteLine("foo");
            Assert.That(receivedSE, Is.SameAs(entry));
        }

        [Test]
        public void ScheduleEntry_gets_actor_and_targets()
        {
            bool checksRan = false;
            var actor = new Actor(new Point(0, 0));
            var targets = new List<Actor>();
            void check_actor_and_targets(IControlPanel cp, ScheduleEntry se)
            {
                Assert.That(se.Actor, Is.SameAs(actor));
                Assert.That(se.Targets, Is.SameAs(targets));
                checksRan = true;
            }

            var entry = new ScheduleEntry(3, check_actor_and_targets, actor, targets);
            schedule.Add(entry);
            schedule.DoNext(nullControlPanel);

            Assert.That(checksRan);
        }

    }
}
