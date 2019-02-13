using CopperBend.App.Model;
using CopperBend.MapUtil;
using NSubstitute;
using NUnit.Framework;
using RLNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class InputCS_command_Tests : InputCommandSourceTestsBase
    {
        [Test]
        public void MultiKey_Command_flow()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.Inventory.Returns(new List<IItem> { fruit });
            Assert.That(_source.InMultiStepCommand, Is.False);

            Queue(RLKey.C);
            var cmd = _source.GetCommand(__actor);
            Assert.That(_source.InMultiStepCommand, "In process of choosing what to consume");
            Assert.That(cmd.Action, Is.EqualTo(CmdAction.None));
            __gameWindow.Received().Prompt("Consume (inventory letter or ? to show inventory): ");

            Queue(RLKey.A);
            cmd = _source.GetCommand(__actor);
            Assert.That(_source.InMultiStepCommand, Is.False, "Picked item to consume");
            Assert.That(cmd.Action, Is.EqualTo(CmdAction.Consume));
            Assert.That(cmd.Direction, Is.EqualTo(CmdDirection.None));
            Assert.That(cmd.Item, Is.EqualTo(fruit));
        }

        [Test]
        public void Prequeued_input_skips_prompts()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.Inventory.Returns(new List<IItem> { fruit });

            Queue(RLKey.C);
            Queue(RLKey.A);
            var cmd = _source.GetCommand(__actor);
            Assert.That(cmd.Action, Is.EqualTo(CmdAction.Consume));
            Assert.That(cmd.Item, Is.EqualTo(fruit));

            __gameWindow.DidNotReceive().Prompt(Arg.Any<string>());
            __gameWindow.DidNotReceive().ShowInventory(Arg.Any<IEnumerable<IItem>>(), Arg.Any<Func<IItem, bool>>());
        }

        [Test]
        public void Can_check_Inventory_mid_command()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.Inventory.Returns(new List<IItem> { fruit });

            Queue(RLKey.C);
            Queue(KP_Question);
            var cmd = _source.GetCommand(__actor);
            Assert.That(cmd, Is.EqualTo(CommandNone));

            __gameWindow.Received().ShowInventory(Arg.Any<IEnumerable<IItem>>(), Arg.Any<Func<IItem, bool>>());
            Assert.That(_source.InMultiStepCommand, "Displaying inventory does not abort command");
        }

        [Test]
        public void PickUp_flow()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.ReachableItems().Returns(new List<IItem> { fruit });
            Queue(RLKey.Comma);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.PickUp));
            Assert.That(Cmd.Item, Is.SameAs(fruit));
        }

        [Test]
        public void PickUp_nothing()
        {
            __actor.ReachableItems().Returns(new List<IItem> { });
            Queue(RLKey.Comma);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandNone));
            __gameWindow.Received().WriteLine("Nothing to pick up here.");
        }

        [Test]
        public void ActorBuild()
        {
            Queue(RLKey.Left);
            _source.GiveCommand(__actor);

            __actor.Received().Command(Arg.Any<Command>());
            var cmdSent = (Command)__actor.ReceivedCalls().Single().GetArguments()[0];
            Assert.That(cmdSent.Action, Is.EqualTo(CmdAction.Move));
            Assert.That(cmdSent.Direction, Is.EqualTo(CmdDirection.West));
            Assert.That(cmdSent.Item, Is.Null);
        }
    }
}
