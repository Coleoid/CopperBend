using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using log4net.Config;
using log4net;
using log4net.Repository;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using CopperBend.Model.Aspects;
using NUnit.Framework;
using NSubstitute;

namespace CopperBend.Engine.Tests
{
    [TestFixture]
    public class Dispatcher_Tests
    {
        private CommandDispatcher _dispatcher = null;
        private ISchedule __schedule = null;
        private GameState _gameState = null;
        private IDescriber __describer = null;
        private IMessageLogWindow __messageOutput = null;

        private TerrainType ttDoorOpen;
        private TerrainType ttDoorClosed;
        private TerrainType ttWall;
        private TerrainType ttFloor;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var repos = LogManager.GetAllRepositories();
            bool foundRepo = repos.Any(r => r.Name == "CB");

            ILoggerRepository repo = null;
            if (foundRepo)
            {
                repo = LogManager.GetRepository("CB");
            }
            else
            {
                repo = LogManager.CreateRepository("CB");
                BasicConfigurator.Configure(repo);
            }

            ttWall = new TerrainType
            {
                CanSeeThrough = false,
                CanWalkThrough = false,
                Looks = new SadConsole.Cell(Color.White, Color.Black, '#'),
                Name = "wall"
            };
            ttFloor = new TerrainType
            {
                CanSeeThrough = true,
                CanWalkThrough = true,
                Looks = new SadConsole.Cell(Color.White, Color.Black, '.'),
                Name = "floor"
            };

            ttDoorOpen = new TerrainType
            {
                CanSeeThrough = true,
                CanWalkThrough = true,
                Looks = new SadConsole.Cell(Color.White, Color.Black, '-'),
                Name = "open door"
            };
            ttDoorClosed = new TerrainType
            {
                CanSeeThrough = true,
                CanWalkThrough = true,
                Looks = new SadConsole.Cell(Color.White, Color.Black, '+'),
                Name = "closed door"
            };

            SpaceMap.TerrainTypes = new Dictionary<string, TerrainType>();
            SpaceMap.TerrainTypes[ttWall.Name] = ttWall;
            SpaceMap.TerrainTypes[ttFloor.Name] = ttFloor;
            SpaceMap.TerrainTypes[ttDoorOpen.Name] = ttDoorOpen;
            SpaceMap.TerrainTypes[ttDoorClosed.Name] = ttDoorClosed;

            Engine.Cosmogenesis("bang");
        }

        [SetUp]
        public void SetUp()
        {
            __schedule = Substitute.For<ISchedule>();
            _gameState = new GameState
            {
                Map = new CompoundMap {
                    BeingMap = new GoRogue.MultiSpatialMap<IBeing>(),
                    BlightMap = new BlightMap(42),
                    SpaceMap = CreateSmallTestMap(),
                    ItemMap = new ItemMap(),
                },
            };
            __describer = Substitute.For<IDescriber>();
            __messageOutput = Substitute.For<IMessageLogWindow>();

            _dispatcher = new CommandDispatcher(__schedule, _gameState, __describer, __messageOutput);
            _dispatcher.ClearPendingInput = () => { };

            Being.EntityFactory = Substitute.For<IEntityFactory>();
        }

        public SpaceMap CreateSmallTestMap()
        {
            SpaceMap spaceMap = new SpaceMap(5, 5);
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    bool isEdge = x == 0 || y == 0 || x == 4 || y == 4;
                    var s = new Space()
                    {
                        Terrain = isEdge ? ttWall : ttFloor
                    };
                    spaceMap.AddItem(s, (x, y));
                }
            }
            var sp = spaceMap.GetItem((3, 4));
            sp.Terrain = ttDoorClosed;

            return spaceMap;
        }

        void Assert_MessageSent_Iff_Player(bool isPlayer, string message)
        {
            if (isPlayer)
                __messageOutput.Received().WriteLine(message);
            else
                __messageOutput.DidNotReceive().WriteLine(message);
        }

        #region Consume
        [Test]
        public void Consume_throws_on_item_not_in_inventory()
        {
            __describer.Describe((Item)null).ReturnsForAnyArgs("Thing");

            var being = new Being(Color.White, Color.Black, '@');
            var item = new Item((0, 0), 1);
            var consumable = new Consumable
            {
                FoodValue = 22,
                PlantID = 2,
                IsFruit = true,
            };
            item.Aspects.AddComponent(consumable);
            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item);

            var ex = Assert.Throws<Exception>(() => _dispatcher.CommandBeing(being, cmd));
            Assert.That(ex.Message, Is.EqualTo("Thing to consume not found in inventory"));
        }

        [Test]
        public void Consume_fruit_adds_seeds_and_removes_fruit_from_inventory()
        {
            __describer.Describe((Item)null).ReturnsForAnyArgs("Thing");

            var being = new Being(Color.White, Color.Black, '@');
            var item = new Item((0, 0), 1);
            var consumable = new Consumable
            {
                FoodValue = 22,
                PlantID = 2,
                IsFruit = true,
                Effect = ("Heal", 4),
            };
            item.Aspects.AddComponent(consumable);
            being.AddToInventory(item);

            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item);
            _dispatcher.CommandBeing(being, cmd);

            Assert.That(being.Inventory.Count(), Is.EqualTo(1));
            if (being.Inventory.ElementAt(0) is Seed seed)
                Assert.That(seed.PlantDetails.MainName, Is.EqualTo("Healer"));
            else
                Assert.Fail("Item wasn't seeds");
        }

        [Test]
        public void Consume_all_of_wielded_empties_hands()
        {
            var being = new Being(Color.White, Color.Black, '@');
            var item = new Item((0, 0), 1);
            var consumable = new Consumable
            {
                FoodValue = 22,
                PlantID = 2,
                IsFruit = true,
            };
            item.Aspects.AddComponent(consumable);
            being.Wield(item);

            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item);
            _dispatcher.CommandBeing(being, cmd);

            Assert.That(being.Inventory.ElementAt(0) is Seed);
            Assert.That(being.WieldedTool, Is.Null);
        }

        [Test]
        public void Consume_part_of_wielded_leaves_remainder_wielded()
        {
            var being = new Being(Color.White, Color.Black, '@');
            var item = new Item((0, 0), 3);
            var consumable = new Consumable
            {
                FoodValue = 22,
                PlantID = 2,
                IsFruit = false,
            };
            item.Aspects.AddComponent(consumable);
            being.Wield(item);

            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item);
            _dispatcher.CommandBeing(being, cmd);

            Assert.That(being.Inventory.ElementAt(0), Is.SameAs(item));
            Assert.That(being.WieldedTool, Is.SameAs(item));
        }
        #endregion

        #region Direction
        [TestCase(CmdDirection.East, 12)]
        [TestCase(CmdDirection.Northeast, 17)]
        public void Direction_commands_take_time(CmdDirection direction, int tickOff)
        {
            var being = new Being(Color.White, Color.Black, '@');
            being.MoveTo((2, 2));
            var cmd = new Command(CmdAction.Direction, direction, null);
            _dispatcher.CommandBeing(being, cmd);

            __schedule.Received().AddAgent(being, tickOff);
        }

        [TestCase(CmdDirection.East, 3, 2)]
        [TestCase(CmdDirection.Northeast, 3, 1)]
        public void Direction_commands_change_location(CmdDirection direction, int newX, int newY)
        {
            var being = new Being(Color.White, Color.Black, '@');
            being.MoveTo((2, 2));
            var cmd = new Command(CmdAction.Direction, direction, null);
            _dispatcher.CommandBeing(being, cmd);

            Assert.That(being.Position, Is.EqualTo(new Point(newX, newY)));
        }

        [TestCase(CmdDirection.East)]
        [TestCase(CmdDirection.Northeast)]
        public void Moving_to_unwalkable_tile_does_nothing(CmdDirection direction)
        {
            __describer.Describe("").ReturnsForAnyArgs("a wall");
            var being = new Being(Color.White, Color.Black, '@');
            being.MoveTo((3, 2));
            being.IsPlayer = true;
            var cmd = new Command(CmdAction.Direction, direction, null);
            _dispatcher.CommandBeing(being, cmd);

            Assert.That(being.Position, Is.EqualTo(new Point(3, 2)));

            __messageOutput.Received().WriteLine("I can't walk through a wall.");
        }

        [Test]
        public void Moving_to_closed_door_opens_door_without_moving()
        {
            var startingPoint = new Point(3, 3);
            var being = new Being(Color.White, Color.Black, '@')
            {
                IsPlayer = true,
                Position = startingPoint,
            };
            var doorSpace = _gameState.Map.SpaceMap.GetItem((3, 4));

            Assert.That(being.Position, Is.EqualTo(startingPoint));
            Assert.That(doorSpace.Terrain.Name, Is.EqualTo("closed door"));

            var cmd = new Command(CmdAction.Direction, CmdDirection.South, null);
            _dispatcher.CommandBeing(being, cmd);

            Assert.That(being.Position, Is.EqualTo(startingPoint));
            Assert.That(doorSpace.Terrain.Name, Is.EqualTo("open door"));
            __schedule.Received().AddAgent(being, 4);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Moving_onto_item_notifies_if_player(bool isPlayer)
        {
            var startingPoint = new Point(2, 2);
            var being = new Being(Color.White, Color.Black, '@')
            {
                IsPlayer = isPlayer,
                Position = startingPoint,
            };

            var itemPoint = new Point(2, 1);
            var item = new Knife(itemPoint);
            _gameState.Map.ItemMap.Add(item, itemPoint);
            __describer.Describe(item, Arg.Any<DescMods>()).Returns("a knife");

            var cmd = new Command(CmdAction.Direction, CmdDirection.North, null);
            _dispatcher.CommandBeing(being, cmd);

            Assert.That(being.Position, Is.EqualTo(itemPoint));
            __schedule.Received().AddAgent(being, 12);

            Assert_MessageSent_Iff_Player(isPlayer, "There is a knife here.");
        }
        #endregion

        #region Drop
        [Test]
        public void Drop_throws_on_item_not_in_inventory()
        {
            var being = new Being(Color.White, Color.Black, '@');
            var item = new Item((0, 0), 1);
            var drop = new Command(CmdAction.Drop, CmdDirection.None, item);

            var ex = Assert.Throws<Exception>(() => _dispatcher.CommandBeing(being, drop));
            Assert.That(ex.Message, Is.EqualTo("Item to drop not found in inventory"));
        }

        [Test]
        public void Drop_moves_item_from_inventory_to_map()
        {
            var being = new Being(Color.White, Color.Black, '@');
            var item = new Item((0, 0), 1);
            being.AddToInventory(item);
            var mySpot = new GoRogue.Coord(2, 2);
            being.MoveTo((2, 2));
            var drop = new Command(CmdAction.Drop, CmdDirection.None, item);
            Assert.That(_gameState.Map.ItemMap.GetItems((2, 2)).Count(), Is.EqualTo(0));

            _dispatcher.CommandBeing(being, drop);

            Assert.That(being.Inventory.Count(), Is.EqualTo(0));
            var items = _gameState.Map.ItemMap.GetItems((2, 2));
            Assert.That(items.Count(), Is.EqualTo(1));
            var mapItem = items.First();

            Assert.That(mapItem.Location, Is.EqualTo(mySpot));
        }

        [Test]
        public void Drop_wielded()
        {
            var being = new Being(Color.White, Color.Black, '@');
            var item = new Item((0, 0), 1);
            being.Wield(item);
            Assert.That(being.WieldedTool, Is.SameAs(item));
            Assert.That(being.Inventory.Count(), Is.EqualTo(1));

            var drop = new Command(CmdAction.Drop, CmdDirection.None, item);
            _dispatcher.CommandBeing(being, drop);

            Assert.That(being.Inventory.Count(), Is.EqualTo(0));

            Assert.That(being.WieldedTool, Is.Null);
            Assert.That(being.Inventory.Count(), Is.EqualTo(0));
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
            var being = new Being(Color.White, Color.Black, '@');
            being.MoveTo((2, 2));
            var item = new Item((2,2), 1);

            var cmd = new Command(CmdAction.PickUp, CmdDirection.None, item);
            _dispatcher.CommandBeing(being, cmd);

            __schedule.DidNotReceive().AddAgent(being, Arg.Any<int>());
        }

        [Test]
        public void PickUp_takes_time()
        {
            var coord = new GoRogue.Coord(2, 2);
            var being = new Being(Color.White, Color.Black, '@');
            being.MoveTo(coord);
            var item = new Item(coord, 1);
            _gameState.Map.ItemMap.Add(item, coord);

            var cmd = new Command(CmdAction.PickUp, CmdDirection.None, item);
            _dispatcher.CommandBeing(being, cmd);

            __schedule.Received().AddAgent(being, 4);
        }

        // PickUp_fails_when_item_out_of_reach()  // or exception?  How'd this happen?

        [Test]
        public void PickUp_moves_item_from_map_to_actor()
        {
            var coord = new GoRogue.Coord(2, 2);
            var being = new Being(Color.White, Color.Black, '@');
            being.MoveTo(coord);
            var item = new Item(coord, 1);
            _gameState.Map.ItemMap.Add(item, coord);

            var cmd = new Command(CmdAction.PickUp, CmdDirection.None, item);
            _dispatcher.CommandBeing(being, cmd);

            Assert.That(_gameState.Map.ItemMap.GetItems(coord), Does.Not.Contain(item));
            Assert.That(being.Inventory.ElementAt(0), Is.SameAs(item));
        }
        #endregion

        #region Wield
        [Test]
        public void Wield_sets_actor_WieldedTool()
        {
            var coord = new GoRogue.Coord(2, 2);
            var being = new Being(Color.White, Color.Black, '@');
            being.MoveTo(coord);
            var item = new Item(coord, 1);
            being.AddToInventory(item);
            Assert.That(being.WieldedTool, Is.Null);

            var cmd = new Command(CmdAction.Wield, CmdDirection.None, item);
            _dispatcher.CommandBeing(being, cmd);

            Assert.That(being.WieldedTool, Is.SameAs(item));
        }
        #endregion
    }
}
