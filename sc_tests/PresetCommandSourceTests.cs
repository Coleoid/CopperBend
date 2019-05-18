//using NUnit.Framework;

//namespace CopperBend.Engine.tests
//{
//    [TestFixture]
//    public class PresetCommandSourceTests
//    {
//        [Test]
//        public void When_empty_returns_wait()
//        {
//            var source = new PresetCommandSource();

//            var cmd = source.GetCommand();
//            Assert.That(cmd.Action, Is.EqualTo(CmdAction.Wait));
//        }

//        [Test]
//        public void When_filled_returns_FIFO()
//        {
//            var source = new PresetCommandSource();
//            source.AddCommand(new Command(CmdAction.Consume, CmdDirection.None));
//            source.AddCommand(new Command(CmdAction.Drop, CmdDirection.None));
//            var cmd = source.GetCommand();
//            Assert.That(cmd.Action, Is.EqualTo(CmdAction.Consume));
//            cmd = source.GetCommand();
//            Assert.That(cmd.Action, Is.EqualTo(CmdAction.Drop));
//            cmd = source.GetCommand();
//            Assert.That(cmd.Action, Is.EqualTo(CmdAction.Wait));
//        }
//    }
//}
