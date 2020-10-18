using System;
using System.Linq;
using CopperBend.Contract;
using CopperBend.Model;
using CopperBend.Model.Aspects;
using NSubstitute;
using NUnit.Framework;
using GoRogue;
using CopperBend.Fabric;
using CopperBend.Creation;

namespace CopperBend.Logic.Tests
{
    [TestFixture]
    public class Dispatcher_Tests : Dispatcher_Tests_Base
    {
        protected override bool ShouldPrepDI => true;
        protected override MockableServices GetServicesToMock()
        {
            return MockableServices.Log
                | MockableServices.Describer
                | MockableServices.Schedule
                | MockableServices.Messager
                | base.GetServicesToMock();
        }

        private IBeing player;
        private Equipper Equipper { get; set; }

        [SetUp]
        public void SetUp()
        {
            player = BeingCreator.CreateBeing("Suvail");
            player.MoveTo(_gameState.Map.BeingMap);
            Equipper = SourceMe.The<Equipper>();
        }

        #region Consume
        [Test]
        public void Consume_throws_on_item_not_in_inventory()
        {
            __describer.Describe((Item)null).ReturnsForAnyArgs("Thing");

            var item = new Item(1);
            var consumable = new Ingestible
            {
                PlantID = 2,
                IsFruit = true,
            };
            item.Aspects.AddComponent(consumable);
            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item);

            var ex = Assert.Throws<Exception>(() => _controls.CommandBeing(player, cmd));
            Assert.That(ex.Message, Is.EqualTo("Thing to consume not found in inventory"));
        }

        [Test]
        public void Consume_fruit_adds_seeds_and_removes_fruit_from_inventory()
        {
            __describer.Describe((Item)null).ReturnsForAnyArgs("Thing");

            var item = new Item(1);
            var consumable = new Ingestible
            {
                PlantID = 2,
                IsFruit = true,
            };
            consumable.AddEffect("heal", 4);
            item.Aspects.AddComponent(consumable);
            player.AddToInventory(item);

            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item, consumable);
            _controls.CommandBeing(player, cmd);

            Assert.That(player.Inventory.Count(), Is.EqualTo(1));
            var seed = player.Inventory.ElementAt(0);
            Assert.That(seed.Name, Is.EqualTo("seed"));

            var plant = seed.Aspects.GetComponent<Plant>();
            Assert.That(plant.PlantDetails.MainName, Is.EqualTo("Healer"));
        }

        [Test]
        public void Consume_all_of_wielded_empties_hands()
        {
            var item = new Item(1);
            var consumable = new Ingestible(foodValue:22)
            {
                PlantID = 2,
                IsFruit = true,
            };
            item.Aspects.AddComponent(consumable);
            player.Wield(item);

            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item, consumable);
            _controls.CommandBeing(player, cmd);

            Assert.That(player.Inventory.ElementAt(0).Name, Is.EqualTo("seed"));
            Assert.That(player.WieldedTool, Is.Null);
        }

        [Test]
        public void Consume_part_of_wielded_leaves_remainder_wielded()
        {
            var item = new Item(3);
            var consumable = new Ingestible
            {
                PlantID = 2,
                IsFruit = false,
            };
            item.Aspects.AddComponent(consumable);
            player.Wield(item);

            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item, consumable);
            _controls.CommandBeing(player, cmd);

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
            _controls.CommandBeing(player, cmd);

            __schedule.Received().AddAgent(player, tickOff);
        }

        [TestCase(CmdDirection.East, 3, 2)]
        [TestCase(CmdDirection.Northeast, 3, 1)]
        public void Direction_commands_change_location(CmdDirection direction, int newX, int newY)
        {
            var newCoord = new Coord(newX, newY);
            player.MoveTo((2, 2));
            var cmd = new Command(CmdAction.Direction, direction, null);
            _controls.CommandBeing(player, cmd);

            Assert.That(player.GetPosition(), Is.EqualTo(newCoord));
            var mapPosition = _gameState.Map.BeingMap.GetPosition(player);
            Assert.That(mapPosition, Is.EqualTo(newCoord));
        }

        [TestCase(CmdDirection.East)]
        [TestCase(CmdDirection.Northeast)]
        public void Moving_to_unwalkable_tile_does_nothing(CmdDirection direction)
        {
            __describer.Describe("").ReturnsForAnyArgs("a wall");

            player.MoveTo((3, 2));
            player.IsPlayer = true;
            var cmd = new Command(CmdAction.Direction, direction, null);
            _controls.CommandBeing(player, cmd);

            var curPosition = player.GetPosition();
            Assert.That(curPosition, Is.EqualTo(new Coord(3, 2)));

            __messager.Received().WriteLineIfPlayer(player, "I can't walk through a wall.");
        }

        [Test]
        public void Moving_to_closed_door_opens_door_without_moving()
        {
            var startingCoord = new Coord(3, 3);
            player.MoveTo(startingCoord);
            var doorSpace = _gameState.Map.SpaceMap.GetItem((3, 4));

            Assert.That(player.GetPosition(), Is.EqualTo(startingCoord));
            Assert.That(doorSpace.Terrain.Name, Is.EqualTo(TerrainEnum.DoorClosed));

            var cmd = new Command(CmdAction.Direction, CmdDirection.South, null);
            _controls.CommandBeing(player, cmd);

            Assert.That(player.GetPosition(), Is.EqualTo(startingCoord));
            Assert.That(doorSpace.Terrain.Name, Is.EqualTo(TerrainEnum.DoorOpen));
            __schedule.Received().AddAgent(player, 4);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Moving_onto_item_notifies_if_player(bool isPlayer)
        {
            var startingCoord = new Coord(2, 2);
            player.IsPlayer = isPlayer;
            player.MoveTo(startingCoord);

            var itemCoord = new Coord(2, 1);
            IItem item = Equipper.BuildItem("knife");
            _gameState.Map.ItemMap.Add(item, itemCoord);
            __describer.Describe(item, Arg.Any<DescMods>()).Returns("a knife");

            var cmd = new Command(CmdAction.Direction, CmdDirection.North, null);
            _controls.CommandBeing(player, cmd);

            Assert.That(player.GetPosition(), Is.EqualTo(itemCoord));
            __schedule.Received().AddAgent(player, 12);

            Assert_Messager_WL_Iff_Player(isPlayer, "There is a knife here.");
        }
        #endregion

        #region Drop
        [Test]
        public void Drop_throws_on_item_not_in_inventory()
        {
            var item = new Item(1);
            var drop = new Command(CmdAction.Drop, CmdDirection.None, item);

            var ex = Assert.Throws<Exception>(() => _controls.CommandBeing(player, drop));
            Assert.That(ex.Message, Is.EqualTo("Item to drop not found in inventory"));
        }

        [Test]
        public void Drop_moves_item_from_inventory_to_map()
        {
            var mySpot = new Coord(2, 2);
            var item = new Item(1);
            player.AddToInventory(item);
            player.MoveTo(_gameState.Map.BeingMap);
            player.MoveTo(mySpot);

            var drop = new Command(CmdAction.Drop, CmdDirection.None, item);
            var items = _gameState.Map.ItemMap.GetItems(mySpot);
            Assert.That(items.Count(), Is.EqualTo(0));

            _controls.CommandBeing(player, drop);

            Assert.That(player.Inventory.Count(), Is.EqualTo(0));
            items = _gameState.Map.ItemMap.GetItems(mySpot);
            Assert.That(items.Count(), Is.EqualTo(1));

            var mapItem = items.First();
            Assert.That(mapItem, Is.SameAs(item));
        }

        [Test]
        public void Drop_wielded()
        {
            var item = new Item(1);
            player.Wield(item);
            Assert.That(player.WieldedTool, Is.SameAs(item));
            Assert.That(player.Inventory.Count(), Is.EqualTo(1));

            var drop = new Command(CmdAction.Drop, CmdDirection.None, item);
            _controls.CommandBeing(player, drop);

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
            var item = new Item(1);

            var cmd = new Command(CmdAction.PickUp, CmdDirection.None, item);
            _controls.CommandBeing(player, cmd);

            __schedule.DidNotReceive().AddAgent(player, Arg.Any<int>());
        }

        [Test]
        public void PickUp_takes_time()
        {
            var coord = new GoRogue.Coord(2, 2);
            player.MoveTo(coord);
            var item = new Item(1);
            _gameState.Map.ItemMap.Add(item, coord);

            var cmd = new Command(CmdAction.PickUp, CmdDirection.None, item);
            _controls.CommandBeing(player, cmd);

            __schedule.Received().AddAgent(player, 4);
        }

        // PickUp_fails_when_item_out_of_reach()  // or exception?  How'd this happen?

        [Test]
        public void PickUp_moves_item_from_map_to_actor()
        {
            var coord = new GoRogue.Coord(2, 2);
            player.MoveTo(coord);
            var item = new Item(1);
            _gameState.Map.ItemMap.Add(item, coord);

            var cmd = new Command(CmdAction.PickUp, CmdDirection.None, item);
            _controls.CommandBeing(player, cmd);

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
            var item = new Item(1);
            player.AddToInventory(item);
            Assert.That(player.WieldedTool, Is.Null);

            var cmd = new Command(CmdAction.Wield, CmdDirection.None, item);
            _controls.CommandBeing(player, cmd);

            Assert.That(player.WieldedTool, Is.SameAs(item));
        }
        #endregion
    }
}
