using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;

//  Vignettes are game-advancing mini-scenes
//  They pace themselves via the Schedule

namespace CopperBend.App.tests
{
    [TestFixture]
    public class VignetteTests
    {

        [Test]
        public void Schedule_calls_action_in_entry()
        {
            bool calledFromSchedule = false;

            void call(IControlPanel cp, ScheduleEntry se) { calledFromSchedule = true; }
            var entry = new ScheduleEntry(3, call);
            var schedule = new Schedule();

            schedule.Add(entry);
            schedule.DoNext(null);

            Assert.That(calledFromSchedule);
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
            var schedule = new Schedule();

            schedule.Add(entry);
            schedule.DoNext(icp);

            icp.Received().WriteLine("foo");
            Assert.That(receivedSE, Is.SameAs(entry));
        }

    }
}
