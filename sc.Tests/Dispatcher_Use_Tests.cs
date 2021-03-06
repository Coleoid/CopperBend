﻿using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;
using NSubstitute;
using NUnit.Framework;
using CopperBend.Creation;

namespace CopperBend.Logic.Tests
{
    [TestFixture]
    public class Dispatcher_Use_Tests : Dispatcher_Tests_Base
    {
        protected override bool ShouldPrepDI => true;
        protected override MockableServices GetServicesToMock()
        {
            return MockableServices.Log
                | MockableServices.Schedule
                | MockableServices.Messager
                | base.GetServicesToMock();
        }

        private Equipper Equipper { get; set; }

        [SetUp]
        public void SetUp()
        {
            Equipper = SourceMe.The<Equipper>();
        }


        private IBeing Prep_being_at_coord(Coord coord, bool isPlayer = false)
        {
            string name = isPlayer ? "Suvail" : "Phredde";
            var being = BeingCreator.CreateBeing(name);
            being.MoveTo(_gameState.Map.BeingMap);
            being.MoveTo(coord);
            being.IsPlayer = isPlayer;

            return being;
        }

        #region Hoe/Tilling
        [Test]
        public void Cannot_till_untillable_terrain()
        {
            var player = Prep_being_at_coord((2, 2), true);
            var hoe = Equipper.BuildItem("hoe");
            player.Wield(hoe);

            var usable = hoe.Aspects.GetComponent<IUsable>();
            var cmd = new Command(CmdAction.Use, CmdDirection.North, hoe, usable);
            _controls.CommandBeing(player, cmd);

            __schedule.DidNotReceive().AddAgent(player, Arg.Any<int>());
            __messager.Received().WriteLineIfPlayer(player, "Cannot till the floor.");
        }

        [Test]
        public void Cannot_till_already_tilled_space()
        {
            var player = Prep_being_at_coord((2, 2), true);
            var hoe = Equipper.BuildItem("hoe");
            player.Wield(hoe);

            var sp = _gameState.Map.SpaceMap.GetItem((2, 1));
            sp.Terrain = ttSoil;
            _controls.Till(sp);

            var usable = hoe.Aspects.GetComponent<IUsable>();
            var cmd = new Command(CmdAction.Use, CmdDirection.North, hoe, usable);
            _controls.CommandBeing(player, cmd);

            __schedule.DidNotReceive().AddAgent(player, Arg.Any<int>());
            __messager.Received().WriteLineIfPlayer(player, "Ground here's already tilled.");
        }

        [Test]
        public void Can_till_tillable_space()
        {
            Coord coord = (2, 2);
            var player = Prep_being_at_coord(coord);
            player.IsPlayer = true;
            var tool = Equipper.BuildItem("hoe");
            player.Wield(tool);

            var sp = _gameState.Map.SpaceMap.GetItem((2, 1));
            sp.Terrain = ttSoil;

            var usable = tool.Aspects.GetComponent<IUsable>();
            var cmd = new Command(CmdAction.Use, CmdDirection.North, tool, usable);
            _controls.CommandBeing(player, cmd);

            __schedule.Received().AddAgent(player, 24);
        }

        [TestCase(true, 24)]
        [TestCase(false, 30)]
        public void Use_unwielded_tool_takes_longer_and_wields_it(bool startsWielded, int tickOff)
        {
            var player = Prep_being_at_coord((2, 2), true);
            var tool = Equipper.BuildItem("hoe");
            if (startsWielded)
                player.Wield(tool);
            else
                player.AddToInventory(tool);

            var sp = _gameState.Map.SpaceMap.GetItem((2, 1));
            sp.Terrain = ttSoil;

            var usable = tool.Aspects.GetComponent<IUsable>();
            var cmd = new Command(CmdAction.Use, CmdDirection.North, tool, usable);
            _controls.CommandBeing(player, cmd);

            __schedule.Received().AddAgent(player, tickOff);
            Assert.That(player.WieldedTool, Is.SameAs(tool));
        }

        //#endregion

        //#region Seed/Planting

        [Test]
        public void Can_plant_in_tilled_space()
        {
            Engine.Cosmogenesis("hiya", __factory);

            var player = Prep_being_at_coord((2, 2), true);
            var seed = Equipper.BuildItem("seed:Healer");
            player.AddToInventory(seed);

            var sp = _gameState.Map.SpaceMap.GetItem((2, 1));
            sp.Terrain = ttSoil;
            _controls.Till(sp);

            var usable = seed.Aspects.GetComponent<IUsable>();
            var cmd = new Command(CmdAction.Use, CmdDirection.North, seed, usable);
            _controls.CommandBeing(player, cmd);

            __schedule.Received().AddAgent(player, 6);
        }

        //[Test]
        //public void Use_seed_on_untilled_tile()
        //{
        //    (var actor, var startingPoint) = SU_actor_at_point(2, 2);
        //    var seed = new HealerSeed(startingPoint, 1);
        //    actor.AddToInventory(seed);

        //    var cmd = new Command(CmdAction.Use, CmdDirection.North, seed);
        //    _dispatcher.CommandActor(actor, cmd);

        //    __schedule.DidNotReceive().AddActor(actor, Arg.Any<int>());
        //    __messageOutput.Received().WriteLine("Cannot sow floor.");
        //}
        #endregion
    }
}
