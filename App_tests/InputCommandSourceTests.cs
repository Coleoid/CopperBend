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
    public class InputCommandSourceTests : InputCommandSourceTestsBase
    {
        [TestCase(RLKey.A)]
        [TestCase(RLKey.X)]
        public void DirectionOf_nondirectional_is_None(RLKey key)
        {
            RLKeyPress press = KeyPressFrom(key);
            var dir = Source.DirectionOf(press);

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
            var dir = Source.DirectionOf(press);

            Assert.That(dir, Is.EqualTo(expectedDir));
        }

        [TestCase(RLKey.Up, CmdDirection.North)]
        [TestCase(RLKey.Keypad1, CmdDirection.West | CmdDirection.South)]
        public void Directional_keypress_becomes_Move(RLKey key, CmdDirection direction)
        {
            Queue(key);
            Cmd = Source.GetCommand();
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Move));
            Assert.That(Cmd.Direction, Is.EqualTo(direction));
        }

        [Test]
        public void No_input_no_command()
        {
            Cmd = Source.GetCommand();
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.None));
            Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.None));
        }

        [Test]
        public void MultiKey_Command_flow()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            Actor.Inventory.Returns(new List<IItem> { fruit });
            Assert.That(Source.InMultiStepCommand, Is.False);

            Queue(RLKey.C);
            var cmd = Source.GetCommand();
            Assert.That(Source.InMultiStepCommand, "In process of choosing what to consume");
            Assert.That(cmd.Action, Is.EqualTo(CmdAction.None));
            Window.Received().Prompt("Consume (inventory letter or ? to show inventory): ");

            Queue(RLKey.A);
            cmd = Source.GetCommand();
            Assert.That(Source.InMultiStepCommand, Is.False, "Picked item to consume");
            Assert.That(cmd.Action, Is.EqualTo(CmdAction.Consume));
            Assert.That(cmd.Direction, Is.EqualTo(CmdDirection.None));
            Assert.That(cmd.Item, Is.EqualTo(fruit));
        }

        [Test]
        public void Prequeued_input_skips_prompts()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            Actor.Inventory.Returns(new List<IItem> { fruit });

            Queue(RLKey.C);
            Queue(RLKey.A);
            var cmd = Source.GetCommand();
            Assert.That(cmd.Action, Is.EqualTo(CmdAction.Consume));
            Assert.That(cmd.Item, Is.EqualTo(fruit));

            Window.DidNotReceive().Prompt(Arg.Any<string>());
            Window.DidNotReceive().ShowInventory(Arg.Any<IEnumerable<IItem>>(), Arg.Any<Func<IItem, bool>>());
        }

        [Test]
        public void Can_check_Inventory_mid_command()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            Actor.Inventory.Returns(new List<IItem> { fruit });

            Queue(RLKey.C);
            Queue(KP_Question);
            var cmd = Source.GetCommand();
            Assert.That(cmd, Is.EqualTo(CommandNone));

            Window.Received().ShowInventory(Arg.Any<IEnumerable<IItem>>(), Arg.Any<Func<IItem, bool>>());
            Assert.That(Source.InMultiStepCommand, "Displaying inventory does not abort command");
        }

        [Test]
        public void PickUp_flow()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            Actor.ReachableItems().Returns(new List<IItem> { fruit });
            Queue(RLKey.Comma);
            Cmd = Source.GetCommand();

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.PickUp));
            Assert.That(Cmd.Item, Is.EqualTo(fruit));
        }

        [Test]
        public void PickUp_nothing()
        {
            Actor.ReachableItems().Returns(new List<IItem> { });
            Queue(RLKey.Comma);
            Cmd = Source.GetCommand();

            Assert.That(Cmd, Is.EqualTo(CommandNone));
            Window.Received().WriteLine("Nothing to pick up here.");
        }
    }
}
