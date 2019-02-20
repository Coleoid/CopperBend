using CopperBend.App.Model;
using CopperBend.MapUtil;
using NSubstitute;
using NUnit.Framework;
using RLNET;
using System;
using System.Collections.Generic;

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
            Cmd = _source.GetCommand(__actor);
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Move));
            Assert.That(Cmd.Direction, Is.EqualTo(direction));
        }

        [Test]
        public void No_input_no_command()
        {
            Cmd = _source.GetCommand(__actor);
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.None));
            Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.None));
            __actor.DidNotReceive().Command(Arg.Any<Command>());
        }

        [Test]
        public void Callback_sends_command_to_actor_on_completion()
        {
            bool enteredNewMode = false;
            EngineMode newMode = EngineMode.Unknown;
            Func<bool> callback = null;
            void captureModeDetails(object s, EnterModeEventArgs a)
            {
                enteredNewMode = true;
                newMode = a.Mode;
                callback = a.Callback;
            }
            _bus.EnterEngineModeSubscribers += captureModeDetails;

            __actor.Inventory.Returns(new List<IItem> { new Hoe(new Point(0, 0)) });
            Queue(RLKey.D);

            _source.GiveCommand(__actor);

            Assert.That(enteredNewMode);
            Assert.That(newMode, Is.EqualTo(EngineMode.InputBound));
            Assert.That(callback, Is.Not.Null);

            callback();

            Queue(RLKey.A);
            callback();
            __controls.Received().CommandActor(Arg.Any<Command>(), Arg.Any<IActor>());
        }

        [Test]
        public void Queued_input_not_needed_to_complete_command_remain_on_queue()
        {
            bool enteredNewMode = false;
            _bus.EnterEngineModeSubscribers += (s, a) => enteredNewMode = true;
            __controls.CommandActor(CommandNone, null).ReturnsForAnyArgs(true);

            Queue(RLKey.Down);
            Queue(RLKey.D);
            Queue(RLKey.A);

            _source.GiveCommand(__actor);

            Assert.That(_inQ.Count, Is.EqualTo(2));
            __controls.Received().CommandActor(Arg.Any<Command>(), Arg.Any<IActor>());
            Assert.That(enteredNewMode, Is.False);
        }

        [Test]
        public void Does_not_change_engine_mode_when_command_completed_in_single_call()
        {
            bool enteredNewMode = false;
            _bus.EnterEngineModeSubscribers += (s, a) => enteredNewMode = true;

            __actor.Inventory.Returns(new List<IItem> { new Hoe(new Point(0, 0)) });
            __controls.CommandActor(CommandNone, null).ReturnsForAnyArgs(true);

            Queue(RLKey.D);
            Queue(RLKey.A);

            Assert.That(_inQ.Count, Is.EqualTo(2));
            __actor.DidNotReceive().Command(Arg.Any<Command>());
            Assert.That(enteredNewMode, Is.False);

            _source.GiveCommand(__actor);

            Assert.That(_inQ.Count, Is.EqualTo(0));
            __controls.Received().CommandActor(Arg.Any<Command>(), Arg.Any<IActor>());
            Assert.That(enteredNewMode, Is.False);
        }

    }
}
