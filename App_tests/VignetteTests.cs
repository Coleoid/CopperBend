using System.Collections.Generic;
using NUnit.Framework;

/*
    What am I talking about here?  I have candidates:
        EventBus Events
        Game-advancing mini-scenes
        Callbacks put into the Schedule

    The mini-scenes probably pace themselves via the Schedule



 */

namespace CopperBend.App.tests
{
    [TestFixture]
    public class VignetteTests
    {
        [Test]
        public void Event_can_send_message()
        {
            List<string> messages = null;

            

            Assert.That(messages.Count, Is.EqualTo(1));
        }
    }
}
