using CopperBend.Contract;
using CopperBend.Model;
using Microsoft.Xna.Framework.Input;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace CopperBend.Engine.Tests
{
    [TestFixture]
    public class InputCS_command_use_Tests : InputCommandSource_TestBase
    {
        [Test]
        public void Use_cancel()
        {
            Queue(Keys.U);
            Queue(Keys.Escape);

            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __controls.Received().WriteLine("cancelled.");
            Assert.That(_source.InMultiStepCommand, Is.False);
        }

        //[Test]
        //public void Use_when_nothing_in_inventory()
        //{
        //}

        [Test]
        public void Use_wielded_item_West()
        {
            var hoe = new Item((0, 0));
            __being.WieldedTool.Returns(hoe);
            Queue(Keys.U);
            Queue(Keys.Left);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Use));
            Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.West));
            Assert.That(Cmd.Item, Is.SameAs(hoe));

            Assert.That(_source.InMultiStepCommand, Is.False);
        }

        [Test]
        public void Use_with_nothing_wielded_must_choose_item()
        {
            var hoe = new Item((0, 0));
            var fruit = new Item((0, 0), 1);
            __being.Inventory.Returns(new List<IItem> { fruit, hoe });
            __being.WieldedTool.Returns((IItem)null);

            Queue(Keys.U);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.InMultiStepCommand);
            __controls.Received().Prompt("Use item: ");

            Queue(Keys.B);
            Cmd = _source.GetCommand(__being);
            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.InMultiStepCommand);
            __controls.Received().Prompt("Direction to use the hoe, or [a-z?] to choose item: ");

            Queue(Keys.NumPad9);
            Cmd = _source.GetCommand(__being);
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Use));
            Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.Northeast));
            Assert.That(Cmd.Item, Is.SameAs(hoe));
            Assert.That(_source.InMultiStepCommand, Is.False);
        }

        //[Test]
        //public void Use_with_nothing_wielded_remembers_last_used()
        //{
        //    var hoe = new Hoe(new Point(0, 0));
        //    var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
        //    __being.Inventory.Returns(new List<IItem> { fruit, hoe });
        //    __being.WieldedTool.Returns((IItem)null);

        //    Queue(RLKey.U);
        //    Queue(RLKey.B);
        //    Queue(RLKey.Keypad9);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Use));
        //    Assert.That(_source.InMultiStepCommand, Is.False);

        //    Queue(RLKey.U);
        //    Queue(RLKey.Down);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Use));
        //    Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.South));
        //    Assert.That(Cmd.Item, Is.SameAs(hoe));
        //}

        //[Test]
        //public void Use_with_nothing_wielded_prequeued_skips_prompts()
        //{
        //    var hoe = new Hoe(new Point(0, 0));
        //    var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
        //    __being.Inventory.Returns(new List<IItem> { fruit, hoe });
        //    __being.WieldedTool.Returns((IItem)null);

        //    Queue(RLKey.U);
        //    Queue(RLKey.B);
        //    Queue(RLKey.Keypad9);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Use));
        //    Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.Northeast));
        //    Assert.That(Cmd.Item, Is.SameAs(hoe));
        //    Assert.That(_source.InMultiStepCommand, Is.False);
        //    __gameWindow.DidNotReceive().Prompt(Arg.Any<string>());
        //}

        //[Test]
        //public void Use_change_item_although_wielding()
        //{
        //    var knife = new Knife(new Point(0, 0));
        //    var hoe = new Hoe(new Point(0, 0));
        //    var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
        //    __being.Inventory.Returns(new List<IItem> { fruit, hoe });
        //    __being.WieldedTool.Returns(knife);

        //    Queue(RLKey.U);
        //    Cmd = _source.GetCommand(__being);

        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    Assert.That(_source.InMultiStepCommand);
        //    __gameWindow.Received().Prompt("Direction to use the knife, or [a-z?] to choose item: ");

        //    Queue(RLKey.B);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    Assert.That(_source.InMultiStepCommand);
        //    __gameWindow.Received().Prompt("Direction to use the hoe, or [a-z?] to choose item: ");

        //    Queue(RLKey.Keypad9);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Use));
        //    Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.Northeast));
        //    Assert.That(Cmd.Item, Is.SameAs(hoe));
        //    Assert.That(_source.InMultiStepCommand, Is.False);
        //}

        //[Test]
        //public void Use_unhappy_paths()
        //{
        //    var knife = new Knife(new Point(0, 0));
        //    var hoe = new Hoe(new Point(0, 0));
        //    var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
        //    __being.Inventory.Returns(new List<IItem> { fruit, hoe });
        //    __being.WieldedTool.Returns((IItem)null);

        //    Queue(RLKey.U);
        //    Queue(RLKey.C);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    Assert.That(_source.InMultiStepCommand);
        //    __gameWindow.Received().WriteLine("The key [c] does not match an inventory item.  Pick another.");

        //    Queue(RLKey.A);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    Assert.That(_source.InMultiStepCommand);
        //    __gameWindow.Received().WriteLine("The smooth fruit is not a usable item.  Pick another.");

        //    Queue(RLKey.Period);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    Assert.That(_source.InMultiStepCommand);
        //    __gameWindow.Received().WriteLine("The key [.] does not match an inventory item.  Pick another.");

        //    Queue(RLKey.Right);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    Assert.That(_source.InMultiStepCommand);
        //    __gameWindow.Received().WriteLine("The key [Right] does not match an inventory item.  Pick another.");

        //    Queue(RLKey.B);
        //    Queue(RLKey.Period);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    Assert.That(_source.InMultiStepCommand);
        //    __gameWindow.Received().WriteLine("The key [.] does not match an inventory item or a direction.  Pick another.");
        //    __gameWindow.ClearReceivedCalls();
        //}
    }
}
