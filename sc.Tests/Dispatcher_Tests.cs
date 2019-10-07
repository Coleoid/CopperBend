using System;
using System.Linq;
using Microsoft.Xna.Framework;
using log4net.Config;
using log4net;
using log4net.Repository;
using NUnit.Framework;
using NSubstitute;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;

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

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            bool foundRepo = false;
            var repos = LogManager.GetAllRepositories();
            foreach (var r in repos)
            {
                if (r.Name == "CB") foundRepo = true;
            }

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
        }

        [SetUp]
        public void SetUp()
        {
            Engine.Cosmogenesis("bang");

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

            //_gameState.Map = CreateSmallTestMap();

            _dispatcher = new CommandDispatcher(__schedule, _gameState, __describer, __messageOutput);
            _dispatcher.ClearPendingInput = () => { };
            var idGen = new GoRogue.IDGenerator();
            CbEntity.IDGenerator = idGen;
            Item.IDGenerator = idGen;

            Being.EntityFactory = Substitute.For<IEntityFactory>();
        }

        public SpaceMap CreateSmallTestMap()
        {
            var ttWall = new TerrainType
            {
                CanSeeThrough = false,
                CanWalkThrough = false,
                Looks = new SadConsole.Cell(Color.White, Color.Black, '#'),
                Name = "wall"
            };
            var ttFloor = new TerrainType
            {
                CanSeeThrough = true,
                CanWalkThrough = true,
                Looks = new SadConsole.Cell(Color.White, Color.Black, '.'),
                Name = "floor"
            };
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

            return spaceMap;
        }

        //[Explicit, Property("Context", "ConsoleRunning")]
        //[TestCase(CmdAction.Wait, CmdDirection.None, 6)]
        //[TestCase(CmdAction.Consume, CmdDirection.None, 2)]
        //[TestCase(CmdAction.Drop, CmdDirection.None, 1)]
        //[TestCase(CmdAction.Wield, CmdDirection.None, 6)]
        //public void Commands_take_time(CmdAction action, CmdDirection direction, int tickOff)
        //{
        //    //TODO: Mock this factory
        //    // needs IConsole plugged into the SadConsole project
        //    //IEntityFactory mockEntityFactory = Substitute.For<IEntityFactory>();
        //    //Being.EntityFactory = mockEntityFactory;

        //    Being.EntityFactory = new EntityFactory();

        //    var actor = new Player(Color.AliceBlue, Color.Black);
        //    var item = new Fruit((0, 0), 1, Fruit.Herbal.PlantByName["Healer"]);
        //    actor.AddToInventory(item);

        //    var cmd = new Command(action, direction, item);
        //    _dispatcher.CommandBeing(actor, cmd);
        //    __schedule.Received().AddAgent(actor, tickOff);
        //}


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
            item.Components.AddComponent(consumable);
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
            item.Components.AddComponent(consumable);
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
        public void Consume_wielded()
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
            item.Components.AddComponent(consumable);
            being.Wield(item);

            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item);
            _dispatcher.CommandBeing(being, cmd);

            Assert.That(being.Inventory.ElementAt(0) is Seed);
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
            //var tile = new Tile(2, 1, new TileType { Name = "closed door", Symbol = '+' });
            //_gameState.Map.SetTile(tile);
            //_gameState.Map.TileTypes["open door"] = new TileType  //WART: map should preload tile types req'd by code
            //{
            //    Name = "open door",
            //    Symbol = '=',
            //    IsTransparent = true,
            //    IsWalkable = true
            //};
            //Point startingPoint = new Point(2, 2);
            //var actor = new Actor(startingPoint);
            //_gameState.Map.ViewpointActor = actor;  //WART: shouldn't need this, should just mark dirty

            //var cmd = new Command(CmdAction.Direction, CmdDirection.North, null);
            //_dispatcher.CommandActor(actor, cmd);

            //Assert.That(actor.Point, Is.EqualTo(startingPoint));
            //__schedule.Received().AddActor(actor, 4);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Moving_onto_item_notifies_if_player(bool isPlayer)
        {
            //Point startingPoint = new Point(2, 2);
            //var actor = new Actor(startingPoint);
            //actor.IsPlayer = isPlayer;

            //var item = new Knife(new Point(2, 1));
            //_gameState.Map.Items.Add(item);

            //var cmd = new Command(CmdAction.Direction, CmdDirection.North, null);
            //_dispatcher.CommandActor(actor, cmd);

            //var outputOrNot = isPlayer
            //    ? (Func<IMessageOutput>)__messageOutput.Received
            //    : __messageOutput.DidNotReceive;
            //outputOrNot().WriteLine("There is a knife here.");
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
            //Point startingPoint = new Point(2, 2);
            //var actor = new Actor(startingPoint);
            //Fruit thisFruit = new Fruit(startingPoint, 1, PlantType.Healer);
            //_gameState.Map.Items.Add(thisFruit);
            //var cmd = new Command(CmdAction.PickUp, CmdDirection.None, null);
            //_dispatcher.CommandActor(actor, cmd);

            //Assert.That(_gameState.Map.Items, Does.Not.Contain(thisFruit));
            //Assert.That(actor.Inventory, Does.Contain(thisFruit));
        }
        #endregion

        #region Wield
        [Test]
        public void Wield_sets_actor_WieldedTool()
        {
            //Point startingPoint = new Point(2, 2);
            //var actor = new Actor(startingPoint);
            //Knife knife = new Knife(startingPoint);
            //actor.AddToInventory(knife);
            //var cmd = new Command(CmdAction.Wield, CmdDirection.None, knife);
            //_dispatcher.CommandActor(actor, cmd);

            //Assert.That(actor.WieldedTool, Is.SameAs(knife));
        }
        #endregion
    }
}
