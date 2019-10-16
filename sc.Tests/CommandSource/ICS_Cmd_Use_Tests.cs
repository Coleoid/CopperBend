﻿using CopperBend.Contract;
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
        public void Use_when_inventory_empty()
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

        [Test]
        public void Use_with_prequeued_input_skips_output()
        {
            (_, _, var hoe) = Fill_pack();
            __being.WieldedTool.Returns((IItem)null);

            Queue(Keys.U, Keys.C, Keys.NumPad9);
            Cmd = _source.GetCommand(__being);

            __prompt.DidNotReceive().Invoke(Arg.Any<string>());
            __writeLine.DidNotReceive().Invoke(Arg.Any<string>());
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Use));
            Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.Northeast));
            Assert.That(Cmd.Item, Is.SameAs(hoe));
            Assert.That(_source.InMultiStepCommand, Is.False);
        }

        [Test]
        public void Use_change_item_although_wielding()
        {
            var (knife, _, hoe) = Fill_pack();
            __being.WieldedTool.Returns(knife);

            Queue(Keys.U);
            Cmd = _source.GetCommand(__being);

            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.InMultiStepCommand);
            __prompt.Received().Invoke("Direction to use the knife, or [a-z?] to choose item: ");

            Queue(Keys.C);
            Cmd = _source.GetCommand(__being);
            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.InMultiStepCommand);
            __prompt.Received().Invoke("Direction to use the hoe, or [a-z?] to choose item: ");

            Queue(Keys.NumPad9);
            Cmd = _source.GetCommand(__being);
            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.Use));
            Assert.That(Cmd.Direction, Is.EqualTo(CmdDirection.Northeast));
            Assert.That(Cmd.Item, Is.SameAs(hoe));
            Assert.That(_source.InMultiStepCommand, Is.False);
        }

        [Test]
        public void Use_unhappy_paths()
        {
            var (_, fruit, hoe) = Fill_pack();
            //__being.Inventory.Returns(new List<IItem> { fruit, hoe });
            __being.WieldedTool.Returns((IItem)null);

            Queue(Keys.U, Keys.D);
            Cmd = _source.GetCommand(__being);
            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.InMultiStepCommand);
            __writeLine.Received().Invoke("The key [d] does not match an inventory item.  Pick another.");
            __writeLine.ClearReceivedCalls();

            //Queue(Keys.B);
            //Cmd = _source.GetCommand(__being);
            //Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            //Assert.That(_source.InMultiStepCommand);
            //__writeLine.Received().Invoke("The smooth fruit is not a usable item.  Pick another.");
            //__writeLine.ClearReceivedCalls();

            Queue(Keys.OemPeriod);
            Cmd = _source.GetCommand(__being);
            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.InMultiStepCommand);
            __writeLine.Received().Invoke("The key [.] does not match an inventory item.  Pick another.");
            __writeLine.ClearReceivedCalls();

            Queue(Keys.Right);
            Cmd = _source.GetCommand(__being);
            Assert.That(Cmd, Is.EqualTo(CommandIncomplete));
            Assert.That(_source.InMultiStepCommand);
            __writeLine.Received().Invoke("The key [Right] does not match an inventory item.  Pick another.");
            __writeLine.ClearReceivedCalls();
        }
    }
}