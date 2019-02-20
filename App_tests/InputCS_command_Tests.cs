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
        public void GetCommand_Move_from_directional_input(RLKey key, CmdDirection direction)
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
            var cmdSent = (Command)__controls.ReceivedCalls().Single().GetArguments()[0];
            Assert.That(cmdSent.Action, Is.EqualTo(CmdAction.Move));
            Assert.That(cmdSent.Direction, Is.EqualTo(CmdDirection.West));
            Assert.That(cmdSent.Item, Is.Null);
            var actor = (IActor)__controls.ReceivedCalls().Single().GetArguments()[1];
            Assert.That(actor, Is.SameAs(__actor));
        }

        //case RLKey.C: return Consume(actor);
        //case RLKey.D: return Drop(actor);
        //case RLKey.H: return Help(actor);
        //case RLKey.I: return Inventory(actor);
        //case RLKey.U: return Use(actor);
        //case RLKey.W: return Wield(actor);

        //case RLKey.Comma: return PickUp(actor);
        [Test]
        public void GetCommand_PickUp_nothing()
        {
            __actor.ReachableItems().Returns(new List<IItem> { });
            Queue(RLKey.Comma);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd, Is.EqualTo(CommandNone));
            __gameWindow.Received().WriteLine("Nothing to pick up here.");
        }

        [Test]
        public void GetCommand_PickUp_single()
        {
            var fruit = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            __actor.ReachableItems().Returns(new List<IItem> { fruit });
            Queue(RLKey.Comma);
            Cmd = _source.GetCommand(__actor);

            Assert.That(Cmd.Action, Is.EqualTo(CmdAction.PickUp));
            Assert.That(Cmd.Item, Is.SameAs(fruit));
        }

        [Test]
        public void GetCommand_PickUp_multiple()
        {
            //TODO:  put multiple items on player tile, expect prompt and choice
        }
    }
}
