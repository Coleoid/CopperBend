using NSubstitute;
using NUnit.Framework;
using RLNET;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class InputCS_utility_Tests : InputCommandSourceTestsBase
    {
        [TestCase(RLKey.A)]
        [TestCase(RLKey.X)]
        public void DirectionOf_nondirectional_is_None(RLKey key)
        {
            RLKeyPress press = KeyPressFrom(key);
            var dir = _source.DirectionOf(press);

            Assert.That(dir, Is.EqualTo(CmdDirection.None));
        }

        [TestCase(RLKey.Left, CmdDirection.West)]
        [TestCase(RLKey.Right, CmdDirection.East)]
        [TestCase(RLKey.Up, CmdDirection.North)]
        [TestCase(RLKey.Down, CmdDirection.South)]
        [TestCase(RLKey.Keypad1, CmdDirection.South | CmdDirection.West)]
        [TestCase(RLKey.Keypad2, CmdDirection.South)]
        [TestCase(RLKey.Keypad3, CmdDirection.South | CmdDirection.East)]
        [TestCase(RLKey.Keypad4, CmdDirection.West)]
        [TestCase(RLKey.Keypad6, CmdDirection.East)]
        [TestCase(RLKey.Keypad7, CmdDirection.North | CmdDirection.West)]
        [TestCase(RLKey.Keypad8, CmdDirection.North)]
        [TestCase(RLKey.Keypad9, CmdDirection.North | CmdDirection.East)]
        public void DirectionOf_directional_is_correct(RLKey key, CmdDirection expectedDir)
        {
            RLKeyPress press = KeyPressFrom(key);
            var dir = _source.DirectionOf(press);

            Assert.That(dir, Is.EqualTo(expectedDir));
        }

        [TestCase(RLKey.Up, CmdDirection.North)]
        [TestCase(RLKey.Keypad1, CmdDirection.South | CmdDirection.West)]
        public void Directional_keypress_becomes_Move(RLKey key, CmdDirection direction)
        {
            Queue(key);
            var actor = Substitute.For<IActor>();
            Cmd = _source.GetCommand(actor);
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Move));
            Assert.That(Cmd.Direction, Is.EqualTo(direction));
        }

        [Test]
        public void No_input_no_command()
        {
            var actor = Substitute.For<IActor>();
            Cmd = _source.GetCommand(actor);
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.None));
            Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.None));
            actor.DidNotReceive().Command(Arg.Any<Command>());
        }


        [Test]
        public void Callback_returns_false_until_command_completes()
        {
        }

        [Test]
        public void Callback_sends_command_to_actor_on_completion()
        {
        }

        [Test]
        public void Queued_input_not_needed_to_complete_command_remain_on_queue()
        {
        }
    }
}
