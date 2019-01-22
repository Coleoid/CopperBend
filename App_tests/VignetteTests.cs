using NUnit.Framework;

//  Vignettes are game-advancing mini-scenes
//  They pace themselves via the Schedule
namespace CopperBend.App.tests
{
    [TestFixture]
    public class VignetteTests
    {
        private Schedule schedule;
        private IControlPanel nullControlPanel;

        [SetUp]
        public void SetUp()
        {
            schedule = new Schedule();
            nullControlPanel = null;
        }
    }
}
