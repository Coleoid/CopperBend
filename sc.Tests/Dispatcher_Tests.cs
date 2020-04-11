using System;
using System.Linq;
using Microsoft.Xna.Framework;
using CopperBend.Contract;
using CopperBend.Model;
using CopperBend.Model.Aspects;
using NSubstitute;
using NUnit.Framework;
using CopperBend.Fabric;
using SadConsole.Entities;

namespace CopperBend.Logic.Tests
{
    [TestFixture]
    public class Dispatcher_Tests : Dispatcher_Tests_Base
    {
        void Assert_MLW_WL_Iff_Player(bool isPlayer, string message)
        {
            if (isPlayer)
                __msgLogWindow.Received().WriteLine(message);
            else
                __msgLogWindow.DidNotReceive().WriteLine(message);
        }

        void Assert_Messager_WL_Iff_Player(bool isPlayer, string message)
        {
            if (isPlayer)
                __messager.Received().WriteLine(message);
            else
                __messager.DidNotReceive().WriteLine(message);
        }

        Being player;

        [SetUp]
        new public void SetUp()
        {
            base.SetUp();
            player = BeingCreator.CreateBeing("Suvail");
        }

        #region Consume
        [Test]
        public void Consume_throws_on_item_not_in_inventory()
        {
            __describer.Describe((Item)null).ReturnsForAnyArgs("Thing");

            var item = new Item((0, 0), 1);
            var consumable = new Ingestible
            {
                PlantID = 2,
                IsFruit = true,
            };
            item.Aspects.AddComponent(consumable);
            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item);

            var ex = Assert.Throws<Exception>(() => _dispatcher.CommandBeing(player, cmd));
            Assert.That(ex.Message, Is.EqualTo("Thing to consume not found in inventory"));
        }

        [Test]
        public void Consume_fruit_adds_seeds_and_removes_fruit_from_inventory()
        {
            __describer.Describe((Item)null).ReturnsForAnyArgs("Thing");

            var item = new Item((0, 0), 1);
            var consumable = new Ingestible
            {
                PlantID = 2,
                IsFruit = true,
            };
            consumable.AddEffect("heal", 4);
            item.Aspects.AddComponent(consumable);
            player.AddToInventory(item);

            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item, consumable);
            _dispatcher.CommandBeing(player, cmd);

            Assert.That(player.Inventory.Count(), Is.EqualTo(1));
            var seed = player.Inventory.ElementAt(0);
            Assert.That(seed.Name, Is.EqualTo("seed"));

            var plant = seed.Aspects.GetComponent<Plant>();
            Assert.That(plant.PlantDetails.MainName, Is.EqualTo("Healer"));
        }

        [Test]
        public void Consume_all_of_wielded_empties_hands()
        {
            var item = new Item((0, 0), 1);
            var consumable = new Ingestible(foodValue:22)
            {
                PlantID = 2,
                IsFruit = true,
            };
            item.Aspects.AddComponent(consumable);
            player.Wield(item);

            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item, consumable);
            _dispatcher.CommandBeing(player, cmd);

            Assert.That(player.Inventory.ElementAt(0).Name, Is.EqualTo("seed"));
            Assert.That(player.WieldedTool, Is.Null);
        }

        [Test]
        public void Consume_part_of_wielded_leaves_remainder_wielded()
        {
            var item = new Item((0, 0), 3);
            var consumable = new Ingestible
            {
                PlantID = 2,
                IsFruit = false,
            };
            item.Aspects.AddComponent(consumable);
            player.Wield(item);

            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item, consumable);
            _dispatcher.CommandBeing(player, cmd);

            Assert.That(player.Inventory.ElementAt(0), Is.SameAs(item));
            Assert.That(player.WieldedTool, Is.SameAs(item));
        }
        #endregion

        #region Direction
        [TestCase(CmdDirection.East, 12)]
        [TestCase(CmdDirection.Northeast, 17)]
        public void Direction_commands_take_time(CmdDirection direction, int tickOff)
        {
            player.MoveTo((2, 2));
            var cmd = new Command(CmdAction.Direction, direction, null);
            _dispatcher.CommandBeing(player, cmd);

            __schedule.Received().AddAgent(player, tickOff);
        }

        [TestCase(CmdDirection.East, 3, 2)]
        [TestCase(CmdDirection.Northeast, 3, 1)]
        public void Direction_commands_change_location(CmdDirection direction, int newX, int newY)
        {
            __factory.GetSadCon(Arg.Any<ISadConInitData>())
                .Returns(Substitute.For<IEntity>());
            player.MoveTo((2, 2));
            var cmd = new Command(CmdAction.Direction, direction, null);
            _dispatcher.CommandBeing(player, cmd);

            Assert.That(player.Position, Is.EqualTo(new Point(newX, newY)));
        }

        [TestCase(CmdDirection.East)]
        [TestCase(CmdDirection.Northeast)]
        public void Moving_to_unwalkable_tile_does_nothing(CmdDirection direction)
        {
            __describer.Describe("").ReturnsForAnyArgs("a wall");
            player.MoveTo((3, 2));
            player.IsPlayer = true;
            var cmd = new Command(CmdAction.Direction, direction, null);
            _dispatcher.CommandBeing(player, cmd);

            Assert.That(player.Position, Is.EqualTo(new Point(3, 2)));

            __messager.Received().WriteLineIfPlayer(player, "I can't walk through a wall.");
        }

        [Test]
        public void Moving_to_closed_door_opens_door_without_moving()
        {
            var startingPoint = new Point(3, 3);
            player.Position = startingPoint;
            var doorSpace = _gameState.Map.SpaceMap.GetItem((3, 4));

            Assert.That(player.Position, Is.EqualTo(startingPoint));
            Assert.That(doorSpace.Terrain.Name, Is.EqualTo("closed door"));

            var cmd = new Command(CmdAction.Direction, CmdDirection.South, null);
            _dispatcher.CommandBeing(player, cmd);

            Assert.That(player.Position, Is.EqualTo(startingPoint));
            Assert.That(doorSpace.Terrain.Name, Is.EqualTo("open door"));
            __schedule.Received().AddAgent(player, 4);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Moving_onto_item_notifies_if_player(bool isPlayer)
        {
            var startingPoint = new Point(2, 2);
            player.IsPlayer = isPlayer;
            player.Position = startingPoint;

            var itemPoint = new Point(2, 1);
            var item = Equipper.BuildItem("knife");
            _gameState.Map.ItemMap.Add(item, itemPoint);
            __describer.Describe(item, Arg.Any<DescMods>()).Returns("a knife");

            var cmd = new Command(CmdAction.Direction, CmdDirection.North, null);
            _dispatcher.CommandBeing(player, cmd);

            Assert.That(player.Position, Is.EqualTo(itemPoint));
            __schedule.Received().AddAgent(player, 12);

            Assert_Messager_WL_Iff_Player(isPlayer, "There is a knife here.");
        }
        #endregion

        #region Drop
        [Test]
        public void Drop_throws_on_item_not_in_inventory()
        {
            var item = new Item((0, 0), 1);
            var drop = new Command(CmdAction.Drop, CmdDirection.None, item);

            var ex = Assert.Throws<Exception>(() => _dispatcher.CommandBeing(player, drop));
            Assert.That(ex.Message, Is.EqualTo("Item to drop not found in inventory"));
        }

        [Test]
        public void Drop_moves_item_from_inventory_to_map()
        {
            var item = new Item((0, 0), 1);
            player.AddToInventory(item);
            var mySpot = new GoRogue.Coord(2, 2);
            player.MoveTo((2, 2));
            var drop = new Command(CmdAction.Drop, CmdDirection.None, item);
            Assert.That(_gameState.Map.ItemMap.GetItems((2, 2)).Count(), Is.EqualTo(0));

            _dispatcher.CommandBeing(player, drop);

            Assert.That(player.Inventory.Count(), Is.EqualTo(0));
            var items = _gameState.Map.ItemMap.GetItems((2, 2));
            Assert.That(items.Count(), Is.EqualTo(1));
            var mapItem = items.First();

            Assert.That(mapItem.Location, Is.EqualTo(mySpot));
        }

        [Test]
        public void Drop_wielded()
        {
            var item = new Item((0, 0), 1);
            player.Wield(item);
            Assert.That(player.WieldedTool, Is.SameAs(item));
            Assert.That(player.Inventory.Count(), Is.EqualTo(1));

            var drop = new Command(CmdAction.Drop, CmdDirection.None, item);
            _dispatcher.CommandBeing(player, drop);

            Assert.That(player.Inventory.Count(), Is.EqualTo(0));

            Assert.That(player.WieldedTool, Is.Null);
            Assert.That(player.Inventory.Count(), Is.EqualTo(0));
        }

        //[Test] //1.+
        //private void Drop_one_from_stack()
        //{
        //    var actor = new Actor(new Point(2, 2));
        //    var item = new HealerSeed(new Point(0, 0), 3);
        //    actor.AddToInventory(item);

        //    var drop = new Command(CmdAction.Drop, CmdDirection.None, item);
        //    _dispatcher.CommandActor(actor, drop);

        //    var mapItems = _gameState.Map.Items;
        //    Assert.That(mapItems.Count, Is.EqualTo(1));
        //    Assert.That(mapItems[0], Is.Not.SameAs(item));
        //    Assert.That(mapItems[0].Quantity, Is.EqualTo(1));
        //    Assert.That(item.Quantity, Is.EqualTo(2));
        //}
        #endregion

        #region Pick Up

        [Test]
        public void PickUp_nothing_takes_no_time()
        {
            player.MoveTo((2, 2));
            var item = new Item((2,2), 1);

            var cmd = new Command(CmdAction.PickUp, CmdDirection.None, item);
            _dispatcher.CommandBeing(player, cmd);

            __schedule.DidNotReceive().AddAgent(player, Arg.Any<int>());
        }

        [Test]
        public void PickUp_takes_time()
        {
            var coord = new GoRogue.Coord(2, 2);
            player.MoveTo(coord);
            var item = new Item(coord, 1);
            _gameState.Map.ItemMap.Add(item, coord);

            var cmd = new Command(CmdAction.PickUp, CmdDirection.None, item);
            _dispatcher.CommandBeing(player, cmd);

            __schedule.Received().AddAgent(player, 4);
        }

        // PickUp_fails_when_item_out_of_reach()  // or exception?  How'd this happen?

        [Test]
        public void PickUp_moves_item_from_map_to_actor()
        {
            var coord = new GoRogue.Coord(2, 2);
            player.MoveTo(coord);
            var item = new Item(coord, 1);
            _gameState.Map.ItemMap.Add(item, coord);

            var cmd = new Command(CmdAction.PickUp, CmdDirection.None, item);
            _dispatcher.CommandBeing(player, cmd);

            Assert.That(_gameState.Map.ItemMap.GetItems(coord), Does.Not.Contain(item));
            Assert.That(player.Inventory.ElementAt(0), Is.SameAs(item));
        }
        #endregion

        #region Wield
        [Test]
        public void Wield_sets_actor_WieldedTool()
        {
            var coord = new GoRogue.Coord(2, 2);
            player.MoveTo(coord);
            var item = new Item(coord, 1);
            player.AddToInventory(item);
            Assert.That(player.WieldedTool, Is.Null);

            var cmd = new Command(CmdAction.Wield, CmdDirection.None, item);
            _dispatcher.CommandBeing(player, cmd);

            Assert.That(player.WieldedTool, Is.SameAs(item));
        }
        #endregion
    }
}
