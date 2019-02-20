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
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Move));
            Assert.That(Cmd.Direction, Is.EqualTo(direction));
        }

        [Test]
        public void GiveCommand_routes_directional_commands()
        {
            Queue(RLKey.Left);
            _source.GiveCommand(__actor);

            __controls.Received().CommandActor(Arg.Any<Command>(), Arg.Any<IActor>());
            var cmdGiven = (Command)__controls.ReceivedCalls().Single().GetArguments()[0];
            Assert.That(cmdGiven.Action, Is.EqualTo(CmdAction.Move));
            Assert.That(cmdGiven.Direction, Is.EqualTo(CmdDirection.West));
            Assert.That(cmdGiven.Item, Is.Null);
            var actor = (IActor)__controls.ReceivedCalls().Single().GetArguments()[1];
            Assert.That(actor, Is.SameAs(__actor));
        }


        //Consume();

        [Test]
        public void Consume_nothing()
        {
            __actor.ReachableItems().Returns(new List<IItem> { });
            Queue(RLKey.C);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandNone));
            __gameWindow.Received().WriteLine("Nothing to eat or drink.");
        }

        [Test]
        public void Consume_inventory()
        {
        }

        [Test]
        public void Consume_reachable()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.ReachableItems().Returns(new List<IItem> { fruit });
            Queue(RLKey.C);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandNone));
            __gameWindow.Received().Prompt("Eat Healer fruit?");
            Queue(RLKey.Y);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Consume));
            Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.None));
            Assert.That(Cmd.Item, Is.SameAs(fruit));
        }


        //Drop();

        [Test]
        public void Drop_nothing()
        {
            Queue(RLKey.D);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandNone));
            __gameWindow.Received().WriteLine("Nothing to drop.");
        }

        [Test]
        public void Drop_inventory()
        {
            var knife = new Knife(new Point(0,0));

            __actor.Inventory.Returns(new List<IItem> {knife});
            Queue(RLKey.D);
            Queue(RLKey.A);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Drop));
            Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.None));
            Assert.That(Cmd.Item, Is.SameAs(knife));
        }


        //Help();

        [Test]
        public void Help()
        {
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

            Assert.That(Cmd, Is.EqualTo(CommandNone));
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
            __gameWindow.Received().WriteLine("Picked up a Healer fruit.");
        }

        [Test]
        public void PickUp_multiple()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            var knife = new Knife(new Point(0,0));
            __actor.ReachableItems().Returns(new List<IItem> { fruit, knife });
            Queue(RLKey.Comma);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandNone));
            __gameWindow.Received().Prompt("Pick up a-b or ? to see items in range: ");
            Queue(RLKey.A);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.PickUp));
            Assert.That(Cmd.Item, Is.SameAs(fruit));
            //TODO:  put multiple items on player tile, expect prompt and choice
        }
    }
}
