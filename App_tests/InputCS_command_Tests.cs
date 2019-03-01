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
        //  Directional input

        [TestCase(RLKey.Up, CmdDirection.North)]
        [TestCase(RLKey.Keypad1, CmdDirection.South | CmdDirection.West)]
        public void Move_from_directional_input(RLKey key, CmdDirection direction)
        {
            Queue(key);
            Cmd = _source.GetCommand(__actor);
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Direction));
            Assert.That(Cmd.Direction, Is.EqualTo(direction));
        }

        [Test]
        public void GiveCommand_routes_directional_commands()
        {
            Queue(RLKey.Left);
            _source.GiveCommand(__actor);

            __controls.Received().CommandActor(Arg.Any<IActor>(), Arg.Any<Command>());
            var cmdGiven = (Command)__controls.ReceivedCalls().Single().GetArguments()[0];
            Assert.That(cmdGiven.Action, Is.EqualTo(CmdAction.Direction));
            Assert.That(cmdGiven.Direction, Is.EqualTo(CmdDirection.West));
            Assert.That(cmdGiven.Item, Is.Null);
            var actor = (IActor)__controls.ReceivedCalls().Single().GetArguments()[1];
            Assert.That(actor, Is.SameAs(__actor));
        }


        //Do_Consume();

        [Test]
        public void Consume_nothing_available()
        {
            var knife = new Knife(new Point(0, 0));
            __actor.Inventory.Returns(new List<IItem> { knife });
            Queue(RLKey.C);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __gameWindow.Received().WriteLine("Nothing to eat or drink.");
            Assert.IsFalse(_source.InMultiStepCommand);
        }

        [Test]
        public void Consume_cancel()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.Inventory.Returns(new List<IItem> { fruit });
            Queue(RLKey.C);
            Queue(RLKey.Escape);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __gameWindow.Received().WriteLine("Do_Consume cancelled.");
            Assert.IsFalse(_source.InMultiStepCommand);
        }

        [Test]
        public void Consume_inventory()
        {
            var knife = new Knife(new Point(0, 0));
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.ReachableItems().Returns(new List<IItem> { });
            __actor.Inventory.Returns(new List<IItem> { knife, fruit });

            Queue(RLKey.C);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __gameWindow.Received().Prompt("Do_Consume (inventory letter or ? to show inventory): ");
            Assert.That(_source.InMultiStepCommand);

            Queue(RLKey.B);
            Cmd = _source.GetCommand(__actor);
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Consume));
            Assert.That(Cmd.Item, Is.SameAs(fruit));
            Assert.IsFalse(_source.InMultiStepCommand);
        }

        [Test]
        public void Consume_unqualified_inventory()
        {
            var knife = new Knife(new Point(0, 0));
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.ReachableItems().Returns(new List<IItem> { });
            __actor.Inventory.Returns(new List<IItem> { knife, fruit });

            Queue(RLKey.C);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __gameWindow.Received().Prompt("Do_Consume (inventory letter or ? to show inventory): ");

            Queue(RLKey.A);
            Cmd = _source.GetCommand(__actor);

            __gameWindow.Received().WriteLine("I can't eat or drink a knife.");
            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.InMultiStepCommand);
        }

        [Test]
        public void Consume_unfilled_inventory_letter()
        {
            var knife = new Knife(new Point(0, 0));
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.ReachableItems().Returns(new List<IItem> { });
            __actor.Inventory.Returns(new List<IItem> { knife, fruit });

            Queue(RLKey.C);
            Queue(RLKey.C);
            Cmd = _source.GetCommand(__actor);

            __gameWindow.Received().WriteLine("Nothing in inventory slot c.");
            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.InMultiStepCommand);
        }

        //[Test]  // not until I care more AND work out the UI flow
        private void Consume_reachable()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.ReachableItems().Returns(new List<IItem> { fruit });
            Queue(RLKey.C);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __gameWindow.Received().Prompt("Eat Healer fruit?");  //or something
            Queue(RLKey.Y);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Consume));
            Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.None));
            Assert.That(Cmd.Item, Is.SameAs(fruit));
        }


        //Drop();

        [Test]
        public void Drop_nothing_available()
        {
            __actor.Inventory.Returns(new List<IItem> { });
            Queue(RLKey.D);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __gameWindow.Received().WriteLine("Nothing to drop.");
            Assert.IsFalse(_source.InMultiStepCommand);
        }

        [Test]
        public void Drop_cancel()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.Inventory.Returns(new List<IItem> { fruit });
            Queue(RLKey.D);
            Queue(RLKey.Escape);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __gameWindow.Received().WriteLine("Drop cancelled.");
            Assert.IsFalse(_source.InMultiStepCommand);
        }

        [Test]
        public void Drop_inventory()
        {
            var knife = new Knife(new Point(0, 0));
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.Inventory.Returns(new List<IItem> { knife, fruit });

            Queue(RLKey.D);
            Cmd = _source.GetCommand(__actor);

            __gameWindow.Received().Prompt("Drop (inventory letter or ? to show inventory): ");
            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.InMultiStepCommand);

            Queue(RLKey.B);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Drop));
            Assert.That(Cmd.Item, Is.SameAs(fruit));
            Assert.IsFalse(_source.InMultiStepCommand);
        }

        [Test]
        public void Drop_unfilled_inventory_letter()
        {
            var knife = new Knife(new Point(0, 0));
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.Inventory.Returns(new List<IItem> { knife, fruit });

            Queue(RLKey.D);
            Queue(RLKey.C);
            Cmd = _source.GetCommand(__actor);

            __gameWindow.Received().WriteLine("Nothing in inventory slot c.");
            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.InMultiStepCommand);
        }


        //Help();

        [Test]
        public void Help()
        {
            Queue(RLKey.H);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __gameWindow.Received().WriteLine("Help:");
        }


        //Inventory();

        [Test]
        public void Inventory()
        {
        }


        //Use() tests are broken out to InputCS_command_use_Tests


        //Wield();

        [Test]
        public void Wield_nothing()
        {
        }


        //PickUp();

        [Test]
        public void PickUp_nothing()
        {
            __actor.ReachableItems().Returns(new List<IItem> { });
            Queue(RLKey.Comma);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __gameWindow.Received().WriteLine("Nothing to pick up here.");
        }

        [Test]
        public void PickUp_single()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.ReachableItems().Returns(new List<IItem> { fruit });
            Queue(RLKey.Comma);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.PickUp));
            Assert.That(Cmd.Item, Is.SameAs(fruit));
        }

        //[Test]
        //public void PickUp_multiple()
        //{
        //    var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
        //    var knife = new Knife(new Point(0,0));
        //    __actor.ReachableItems().Returns(new List<IItem> { fruit, knife });
        //    Queue(RLKey.Comma);
        //    Cmd = _source.GetCommand(__actor);

        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    __gameWindow.Received().Prompt("Pick up a-b or ? to see items in range: ");
        //    Queue(RLKey.A);
        //    Cmd = _source.GetCommand(__actor);

        //    Assert.That(Cmd.Action, Is.EqualTo(CmdAction.PickUp));
        //    Assert.That(Cmd.Item, Is.SameAs(fruit));
        //    //TODO:  put multiple items on player tile, expect prompt and choice
        //}
    }
}
