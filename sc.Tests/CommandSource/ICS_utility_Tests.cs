using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CopperBend.Logic.Tests
{
    [TestFixture]
    public class ICS_utility_Tests : InputStrategy_TestBase
    {
//        [TestCase(RLKey.A)]
//        [TestCase(RLKey.X)]
//        public void DirectionOf_nondirectional_is_None(RLKey key)
//        {
//            RLKeyPress press = KeyPressFrom(key);
//            var dir = _source.DirectionOf(press);

//            Assert.That(dir, Is.EqualTo(CmdDirection.None));
//        }

//        [TestCase(RLKey.Left, CmdDirection.West)]
//        [TestCase(RLKey.Right, CmdDirection.East)]
//        [TestCase(RLKey.Up, CmdDirection.North)]
//        [TestCase(RLKey.Down, CmdDirection.South)]
//        [TestCase(RLKey.Keypad1, CmdDirection.South | CmdDirection.West)]
//        [TestCase(RLKey.Keypad2, CmdDirection.South)]
//        [TestCase(RLKey.Keypad3, CmdDirection.South | CmdDirection.East)]
//        [TestCase(RLKey.Keypad4, CmdDirection.West)]
//        [TestCase(RLKey.Keypad6, CmdDirection.East)]
//        [TestCase(RLKey.Keypad7, CmdDirection.North | CmdDirection.West)]
//        [TestCase(RLKey.Keypad8, CmdDirection.North)]
//        [TestCase(RLKey.Keypad9, CmdDirection.North | CmdDirection.East)]
//        public void DirectionOf_directional_is_correct(RLKey key, CmdDirection expectedDir)
//        {
//            RLKeyPress press = KeyPressFrom(key);
//            var dir = _source.DirectionOf(press);

//            Assert.That(dir, Is.EqualTo(expectedDir));
//        }

//        [Test]
//        public void No_input_no_command()
//        {
//            Cmd = _source.GetCommand(__actor);
//            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Incomplete));
//            Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.None));
//        }

//        [Test]
//        public void Callback_sends_command_to_actor_on_completion()
//        {
//            bool enteredNewMode = false;
//            EngineMode newMode = EngineMode.Unknown;
//            Func<bool> callback = null;
//            void captureModeDetails(object s, EnterModeEventArgs a)
//            {
//                enteredNewMode = true;
//                newMode = a.Mode;
//                callback = a.Callback;
//            }
//            _bus.EnterEngineModeSubscribers += captureModeDetails;

//            __actor.Inventory.Returns(new List<IItem> { new Hoe(new Point(0, 0)) });
//            Queue(RLKey.D);

//            _source.GiveCommand(__actor);

//            Assert.That(enteredNewMode);
//            Assert.That(newMode, Is.EqualTo(EngineMode.InputBound));
//            Assert.That(callback, Is.Not.Null);

//            callback();

//            Queue(RLKey.A);
//            callback();
//            __controls.Received().CommandActor(Arg.Any<IActor>(), Arg.Any<Command>());
//        }

//        [Test]
//        public void Queued_inputs_not_needed_to_complete_command_will_remain_on_queue()
//        {
//            bool enteredNewMode = false;
//            _bus.EnterEngineModeSubscribers += (s, a) => enteredNewMode = true;
//            __controls.CommandActor(null, CommandIncomplete).ReturnsForAnyArgs(true);

//            Queue(RLKey.Down);
//            Queue(RLKey.D);
//            Queue(RLKey.A);

//            _source.GiveCommand(__actor);

//            Assert.That(_inQ.Count, Is.EqualTo(2));
//            __controls.Received().CommandActor(Arg.Any<IActor>(), Arg.Any<Command>());
//            Assert.That(enteredNewMode, Is.False);
//        }

//        [Test]
//        public void When_command_completed_in_single_call_engine_mode_will_not_change()
//        {
//            bool enteredNewMode = false;
//            _bus.EnterEngineModeSubscribers += (s, a) => enteredNewMode = true;

//            __actor.Inventory.Returns(new List<IItem> { new Hoe(new Point(0, 0)) });
//            __controls.CommandActor(null, CommandIncomplete).ReturnsForAnyArgs(true);

//            Queue(RLKey.D);
//            Queue(RLKey.A);

//            Assert.That(_inQ.Count, Is.EqualTo(2));
//            __controls.DidNotReceive().CommandActor(Arg.Any<IActor>(), Arg.Any<Command>());
//            Assert.That(enteredNewMode, Is.False);

//            _source.GiveCommand(__actor);

//            Assert.That(_inQ.Count, Is.EqualTo(0));
//            __controls.Received().CommandActor(Arg.Any<IActor>(), Arg.Any<Command>());
//            Assert.That(enteredNewMode, Is.False);
//        }

//        [Test]
//        public void MultiKey_Command_flow()
//        {
//            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
//            __actor.Inventory.Returns(new List<IItem> { fruit });
//            Assert.That(_source.InMultiStepCommand, Is.False);

//            Queue(RLKey.C);
//            var cmd = _source.GetCommand(__actor);
//            Assert.That(_source.InMultiStepCommand, "In process of choosing what to consume");
//            Assert.That(cmd.Action, Is.EqualTo(CmdAction.Incomplete));
//            __gameWindow.Received().Prompt("Do_Consume (inventory letter or ? to show inventory): ");

//            Queue(RLKey.A);
//            cmd = _source.GetCommand(__actor);
//            Assert.That(_source.InMultiStepCommand, Is.False, "Picked item to consume");
//            Assert.That(cmd.Action, Is.EqualTo(CmdAction.Consume));
//            Assert.That(cmd.Direction, Is.EqualTo(CmdDirection.None));
//            Assert.That(cmd.Item, Is.EqualTo(fruit));
//        }

//        [Test]
//        public void Prequeued_input_skips_prompts()
//        {
//            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
//            __actor.Inventory.Returns(new List<IItem> { fruit });

//            Queue(RLKey.C);
//            Queue(RLKey.A);
//            var cmd = _source.GetCommand(__actor);
//            Assert.That(cmd.Action, Is.EqualTo(CmdAction.Consume));
//            Assert.That(cmd.Item, Is.EqualTo(fruit));

//            __gameWindow.DidNotReceive().Prompt(Arg.Any<string>());
//            __gameWindow.DidNotReceive().ShowInventory(Arg.Any<IEnumerable<IItem>>(), Arg.Any<Func<IItem, bool>>());
//        }

//        [Test]
//        public void Can_check_Inventory_mid_command()
//        {
//            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
//            __actor.Inventory.Returns(new List<IItem> { fruit });

//            Queue(RLKey.C);
//            Queue(KP_Question);
//            var cmd = _source.GetCommand(__actor);
//            Assert.That(cmd, Is.EqualTo(CommandIncomplete));

//            __gameWindow.Received().ShowInventory(Arg.Any<IEnumerable<IItem>>(), Arg.Any<Func<IItem, bool>>());
//            Assert.That(_source.InMultiStepCommand, "Displaying inventory does not abort command");
//        }
    }
}
