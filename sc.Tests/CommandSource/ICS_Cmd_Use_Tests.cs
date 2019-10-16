using CopperBend.Contract;
using CopperBend.Model;
using CopperBend.Model.Aspects;
using Microsoft.Xna.Framework.Input;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace CopperBend.Engine.Tests
{
    [TestFixture]
    public class ICS_Cmd_Use_Tests : ICS_TestBase
    {
        [Test]
        public void Use_then_cancel()
        {
            Fill_pack();
            __being.WieldedTool.Returns((IItem)null);
            Queue(Keys.U, Keys.Escape);

            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __writeLine.Received().Invoke("cancelled.");
            Assert.That(_source.InMultiStepCommand, Is.False);
        }

        [Test]
        public void Use_when_nothing_in_inventory()
        {
            __being.Inventory.Returns(new List<IItem>());
            __being.WieldedTool.Returns((IItem)null);
            Queue(Keys.U);

            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            __writeLine.Received().Invoke("Nothing usable on me.");
            Assert.That(_source.InMultiStepCommand, Is.False);
        }

        [Test]
        public void Use_wielded_item_West()
        {
            var (_, _, hoe) = Fill_pack();
            __being.WieldedTool.Returns(hoe);
            Queue(Keys.U, Keys.Left);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Use));
            Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.West));
            Assert.That(Cmd.Item, Is.SameAs(hoe));

            Assert.That(_source.InMultiStepCommand, Is.False);
        }
        // Use_with_nonUsable_wielded_must_choose_item()

        // Use_with_self_targeting_item_wielded_asks()
        // 'e': [eat apple], '?': pick item and use, '
        // Use_with_other_targeting_item_wielded_goes_()


        [Test]
        public void Use_with_nothing_wielded_must_choose_item()
        {
            (_, _, var hoe) = Fill_pack();
            __being.WieldedTool.Returns((IItem)null);

            Queue(Keys.U);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.InMultiStepCommand);
            __prompt.Received().Invoke("Use item: ");

            Queue(Keys.C);
            Cmd = _source.GetCommand(__being);
            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.InMultiStepCommand);
            __prompt.Received().Invoke("Direction to use the hoe, or [a-z?] to choose item: ");
            //__prompt.DidNotReceive().Invoke(Arg.Any<string>());

            Queue(Keys.NumPad9);
            Cmd = _source.GetCommand(__being);
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Use));
            Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.Northeast));
            Assert.That(Cmd.Item, Is.SameAs(hoe));
            Assert.That(_source.InMultiStepCommand, Is.False);
        }

        [Test]
        public void Use_with_nothing_wielded_remembers_last_used()
        {
            (_, _, var hoe) = Fill_pack();
            __being.WieldedTool.Returns((IItem)null);

            Queue(Keys.U, Keys.C, Keys.NumPad9);
            Cmd = _source.GetCommand(__being);
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Use));
            Assert.That(_source.InMultiStepCommand, Is.False);

            Queue(Keys.U, Keys.Down);
            Cmd = _source.GetCommand(__being);
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Use));
            Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.South));
            Assert.That(Cmd.Item, Is.SameAs(hoe));
        }

        //[Test]
        //public void Use_with_nothing_wielded_prequeued_skips_prompts()
        //{
        //    var hoe = new Hoe(new Point(0, 0));
        //    var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
        //    __being.Inventory.Returns(new List<IItem> { fruit, hoe });
        //    __being.WieldedTool.Returns((IItem)null);

        //    Queue(Keys.U);
        //    Queue(Keys.B);
        //    Queue(Keys.NumPad9);
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

        //    Queue(Keys.U);
        //    Cmd = _source.GetCommand(__being);

        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    Assert.That(_source.InMultiStepCommand);
        //    __gameWindow.Received().Prompt("Direction to use the knife, or [a-z?] to choose item: ");

        //    Queue(Keys.B);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    Assert.That(_source.InMultiStepCommand);
        //    __gameWindow.Received().Prompt("Direction to use the hoe, or [a-z?] to choose item: ");

        //    Queue(Keys.NumPad9);
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

        //    Queue(Keys.U);
        //    Queue(Keys.C);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    Assert.That(_source.InMultiStepCommand);
        //    __gameWindow.Received().WriteLine("The key [c] does not match an inventory item.  Pick another.");

        //    Queue(Keys.A);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    Assert.That(_source.InMultiStepCommand);
        //    __gameWindow.Received().WriteLine("The smooth fruit is not a usable item.  Pick another.");

        //    Queue(Keys.Period);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    Assert.That(_source.InMultiStepCommand);
        //    __gameWindow.Received().WriteLine("The key [.] does not match an inventory item.  Pick another.");

        //    Queue(Keys.Right);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    Assert.That(_source.InMultiStepCommand);
        //    __gameWindow.Received().WriteLine("The key [Right] does not match an inventory item.  Pick another.");

        //    Queue(Keys.B);
        //    Queue(Keys.Period);
        //    Cmd = _source.GetCommand(__being);
        //    Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
        //    Assert.That(_source.InMultiStepCommand);
        //    __gameWindow.Received().WriteLine("The key [.] does not match an inventory item or a direction.  Pick another.");
        //    __gameWindow.ClearReceivedCalls();
        //}
    }
}
