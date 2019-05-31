//using NUnit.Framework;

////  Vignettes are game-advancing mini-scenes
////  They pace themselves via the Schedule
//namespace CopperBend.Engine.tests
//{
//    [TestFixture]
//    public class VignetteTests
//    {
//        private Schedule schedule;
//        //private IControlPanel nullControlPanel;

//        [SetUp]
//        public void SetUp()
//        {
//            schedule = new Schedule();
//            //nullControlPanel = null;
//        }


///*
//Fantasy syntax for Vignettes.  We want to...
//Have multiple actions, with time offsets
//  30, WriteLine, "Howdy..."
//  120, WriteLine, "Pardner"
//or
//  (30 (WriteLine "Howdy"))
//Run same action against actor or targets?
//  Not an issue, we can put actor in targets
//Chain actions for player and NPCs/entities
//  (Walk player
//    (1 -5)
//    (0 -2)
//    (3  0)
//  )

//  Too generic.  Since I already know of some vignettes I want to implement,
//let's try syntax to support those.
//*/
//    }
//}
