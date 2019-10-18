using Microsoft.Xna.Framework;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Model;
using CopperBend.Fabric;
using NSubstitute;
using NUnit.Framework;

namespace CopperBend.Engine.Tests
{
    [TestFixture]
    public class Dispatcher_Use_Tests : Dispatcher_Tests_Base
    {
        private Being SU_being_at_coord(Coord coord, int glyph)
        {
            var being = new Being(Color.White, Color.Black, glyph);
            being.MoveTo(coord);
            return being;
        }

        #region Hoe/Tilling
        [Test]
        public void Use_hoe_on_untillable_tile()
        {
            Coord coord = (2, 2);
            var player = SU_being_at_coord(coord, '@');
            player.IsPlayer = true;
            var hoe = Equipper.BuildItem("hoe");
            player.AddToInventory(hoe);
            player.Wield(hoe);

            var usable = hoe.Aspects.GetComponent<IUsable>();
            var cmd = new Command(CmdAction.Use, CmdDirection.North, hoe, usable);
            _dispatcher.CommandBeing(player, cmd);

            __schedule.DidNotReceive().AddAgent(player, Arg.Any<int>());
            __messageOutput.Received().WriteLine("Cannot till the floor.");
        }

        [Test]
        public void Use_hoe_on_tilled_tile()
        {
            Coord coord = (2, 2);
            var player = SU_being_at_coord(coord, '@');
            player.IsPlayer = true;
            var hoe = Equipper.BuildItem("hoe");
            player.AddToInventory(hoe);
            player.Wield(hoe);

            Tile soil = new Tile(2, 1, new TileType { IsTillable = true, IsTransparent = true, IsWalkable = true, Name = "soil", Symbol = '.' });
            _dispatcher.Till(soil);
            _gameState.Map.SetTile(soil);

            var usable = hoe.Aspects.GetComponent<IUsable>();
            var cmd = new Command(CmdAction.Use, CmdDirection.North, hoe, usable);
            _dispatcher.CommandBeing(player, cmd);

            __schedule.DidNotReceive().AddAgent(player, Arg.Any<int>());
            __messageOutput.Received().WriteLine("Cannot till the floor.");
        }

        //[Test]
        //public void Use_hoe_on_tilled_tile()
        //{
        //    (var actor, var startingPoint) = SU_actor_at_point(2, 2);
        //    var hoe = new Hoe(startingPoint);
        //    actor.AddToInventory(hoe);
        //    actor.WieldedTool = hoe;

        //    Tile soil = new Tile(2, 1, new TileType { IsTillable = true, IsTransparent = true, IsWalkable = true, Name = "soil", Symbol = '.' });
        //    _dispatcher.Till(soil);
        //    _gameState.Map.SetTile(soil);

        //    var cmd = new Command(CmdAction.Use, CmdDirection.North, hoe);
        //    _dispatcher.CommandActor(actor, cmd);

        //    __schedule.DidNotReceive().AddActor(actor, Arg.Any<int>());
        //    __messageOutput.Received().WriteLine("Already tilled.");
        //}

        //[Test]
        //public void Use_hoe_tills_ground_in_direction()
        //{
        //    (var actor, var startingPoint) = SU_actor_at_point(2, 2);
        //    var hoe = new Hoe(startingPoint);
        //    actor.AddToInventory(hoe);
        //    actor.WieldedTool = hoe;

        //    Tile soil = new Tile(2, 1, new TileType { IsTillable = true, IsTransparent = true, IsWalkable = true, Name = "soil", Symbol = '.' });
        //    _gameState.Map.SetTile(soil);

        //    var cmd = new Command(CmdAction.Use, CmdDirection.North, hoe);
        //    _dispatcher.CommandActor(actor, cmd);

        //    Assert.That(soil.IsTilled);
        //}

        //[TestCase(true, 15)]
        //[TestCase(false, 21)]
        //public void Use_unwielded_hoe_takes_longer_and_wields_it(bool startsWielded, int tickOff)
        //{
        //    (var actor, var startingPoint) = SU_actor_at_point(2, 2);
        //    var hoe = new Hoe(startingPoint);
        //    actor.AddToInventory(hoe);
        //    if (startsWielded)
        //        actor.WieldedTool = hoe;

        //    Tile soil = new Tile(2, 1, new TileType { IsTillable = true, IsTransparent = true, IsWalkable = true, Name = "soil", Symbol = '.' });
        //    _gameState.Map.SetTile(soil);

        //    var cmd = new Command(CmdAction.Use, CmdDirection.North, hoe);
        //    _dispatcher.CommandActor(actor, cmd);

        //    Assert.That(soil.IsTilled);
        //    Assert.That(actor.WieldedTool, Is.SameAs(hoe));
        //    __schedule.Received().AddActor(actor, tickOff);
        //}
        //#endregion

        //#region Seed/Planting
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
