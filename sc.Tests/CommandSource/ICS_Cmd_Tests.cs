using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using CopperBend.Contract;
using CopperBend.Model;
using NSubstitute;
using NUnit.Framework;
using GoRogue;

namespace CopperBend.Logic.Tests
{
    [TestFixture]
    public class ICS_Cmd_Tests : ICS_TestBase
    {
        //  Directional input

        [TestCase(Keys.Up, CmdDirection.North)]
        [TestCase(Keys.NumPad1, CmdDirection.South | CmdDirection.West)]
        public void Move_from_directional_input(Keys key, CmdDirection direction)
        {
            Queue(key);
            Cmd = _source.GetCommand(__being);
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Direction));
            Assert.That(Cmd.Direction, Is.EqualTo(direction));
        }

        [Test]
        public void GiveCommand_routes_directional_commands()
        {
            Queue(Keys.Left);
            _source.GiveCommand(__being);

            __controls.Received().CommandBeing(Arg.Any<IBeing>(), Arg.Any<Command>());
            var call = __controls.ReceivedCalls().Single(c => c.GetMethodInfo().Name == "CommandBeing");
            var cmdGiven = (Command)call.GetArguments()[1];
            Assert.That(cmdGiven.Action, Is.EqualTo(CmdAction.Direction));
            Assert.That(cmdGiven.Direction, Is.EqualTo(CmdDirection.West));
            Assert.That(cmdGiven.Item, Is.Null);
            var being = (IBeing)call.GetArguments()[0];
            Assert.That(being, Is.SameAs(__being));
        }


        ////Do_Consume();

        [Test]
        public void Consume_nothing_available()
        {
            var knife = new Item();
            __being.Inventory.Returns(new List<IItem> { knife });
            Queue(Keys.C);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __messager.Received().WriteLine("Nothing to eat or drink on me.");
            Assert.IsFalse(_source.IsAssemblingCommand);
        }

        [Test]
        public void Consume_cancel()
        {
            Fill_pack();

            Queue(Keys.C, Keys.Escape);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __messager.Received().WriteLine("cancelled.");
            Assert.IsFalse(_source.IsAssemblingCommand);
        }

        [Test]
        public void Consume_inventory()
        {
            (_, var fruit, _) = Fill_pack();

            Queue(Keys.C);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __messager.Received().Prompt("Consume (inventory letter or ? to show inventory): ");
            Assert.That(_source.IsAssemblingCommand);

            Queue(Keys.B);
            Cmd = _source.GetCommand(__being);
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Consume));
            Assert.That(Cmd.Item, Is.SameAs(fruit));
            Assert.IsFalse(_source.IsAssemblingCommand);
        }

        [Test]
        public void Consume_unqualified_inventory()
        {
            Fill_pack();

            Queue(Keys.C, Keys.A);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Incomplete));
            __messager.Received().WriteLine("I can't eat or drink a knife.");
            Assert.That(_source.IsAssemblingCommand);
        }

        [Test]
        public void Consume_unfilled_inventory_letter()
        {
            Fill_pack();

            Queue(Keys.C, Keys.D);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Incomplete));
            __messager.Received().WriteLine("Nothing in inventory slot d.");
            Assert.That(_source.IsAssemblingCommand);
        }

        ////[Test]  // not until I care more AND work out the UI flow
        //private void Consume_reachable()
        //{
        //    var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
        //    __being.ReachableItems().Returns(new List<IItem> { fruit });
        //    Queue(Keys.C);
        //    Cmd = _source.GetCommand(__being);

        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    __gameWindow.Received().Prompt("Eat Healer fruit?");  //or something
        //    Queue(Keys.Y);
        //    Cmd = _source.GetCommand(__being);

        //    Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Consume));
        //    Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.None));
        //    Assert.That(Cmd.Item, Is.SameAs(fruit));
        //}


        ////Drop();

        [Test]
        public void Drop_with_empty_inventory()
        {
            __being.Inventory.Returns(new List<IItem> { });

            Queue(Keys.D);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __messager.Received().WriteLine("Nothing to drop.");
            Assert.IsFalse(_source.IsAssemblingCommand);
        }

        [Test]
        public void Drop_then_cancel()
        {
            Fill_pack();

            Queue(Keys.D, Keys.Escape);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __messager.Received().WriteLine("Drop cancelled.");
            Assert.IsFalse(_source.IsAssemblingCommand);
        }

        [Test]
        public void Drop_happypath()
        {
            var (_, fruit, _) = Fill_pack();

            Queue(Keys.D);
            Cmd = _source.GetCommand(__being);

            __messager.Received().Prompt("Drop (inventory letter or ? to show inventory): ");
            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.IsAssemblingCommand);

            Queue(Keys.B);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Drop));
            Assert.That(Cmd.Item, Is.SameAs(fruit));
            Assert.IsFalse(_source.IsAssemblingCommand);
        }

        [Test]
        public void Drop_unfilled_inventory_letter()
        {
            Fill_pack();

            Queue(Keys.D, Keys.D);
            Cmd = _source.GetCommand(__being);

            __messager.DidNotReceive().Prompt("Drop (inventory letter or ? to show inventory): ");
            __messager.Received().WriteLine("Nothing in inventory slot d.");
            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.IsAssemblingCommand);
        }


        ////Help();

        [Test]
        public void Help()
        {
            Queue(Keys.H);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __messager.Received().WriteLine("Help:");
        }


        ////Inventory();

        //[Test]
        //public void Inventory()
        //{
        //}


        ////Use() tests are broken out to InputCS_command_use_Tests


        ////Wield();

        //[Test]
        //public void Wield_nothing()
        //{
        //}


        ////PickUp();

        [Test]
        public void PickUp_nothing()
        {
            __being.ReachableItems().Returns(new List<IItem> { });
            Queue(Keys.OemComma);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __messager.Received().WriteLine("Nothing to pick up here.");
        }

        [Test]
        public void PickUp_single()
        {
            var widget = new Item(1);
            _gameState.Map.ItemMap.Add(widget, (2, 2));
            __being.GetPosition().Returns(new Coord(2, 2));

            Queue(Keys.OemComma);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.PickUp));
            Assert.That(Cmd.Item, Is.SameAs(widget));
        }

        [Test]
        public void PickUp_multiple()
        {
            var widget = new Item() { Name = "widget" };
            var gadget = new Item() { Name = "gadget" };
            _gameState.Map.ItemMap.Add(widget, (2, 2));
            _gameState.Map.ItemMap.Add(gadget, (2, 2));
            __being.GetPosition().Returns(new Coord(2, 2));

            Queue(Keys.OemComma);
            Cmd = _source.GetCommand(__being);

            //0.2: pickup is simplified at the moment
            //Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            //__prompt.Received().Invoke("Pick up a-b or ? to see items in range: ");
            //Queue(Keys.A);
            //Cmd = _source.GetCommand(__being);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.PickUp));
            Assert.That(Cmd.Item, Is.SameAs(widget));
            //TODO:  put multiple items on player tile, expect prompt and choice
        }
    }
}
