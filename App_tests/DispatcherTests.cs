using NSubstitute;
using NUnit.Framework;
using System;
using System.Linq;
using CopperBend.App.Model;
using CopperBend.MapUtil;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class DispatcherTests
    {
        private CommandDispatcher _dispatcher = null;
        private ISchedule __schedule = null;
        private GameState _gameState = null;
        private Describer __describer = null;
        private EventBus __eventBus = null;
        private IMessageOutput __messageOutput = null;

        [SetUp]
        public void SetUp()
        {
            __schedule = Substitute.For<ISchedule>();
            _gameState = new GameState();
            __describer = Substitute.For<Describer>(88);
            __eventBus = new EventBus();
            __messageOutput = Substitute.For<IMessageOutput>();

            _dispatcher = new CommandDispatcher(__schedule, _gameState, __describer, __eventBus, __messageOutput);
        }

        #region Consume
        [Test]
        public void Consume_throws_on_item_not_in_inventory()
        {
            var actor = new Actor();
            var item = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item);

            var ex = Assert.Throws<Exception>(() => _dispatcher.CommandActor(actor, cmd));
            Assert.That(ex.Message, Is.EqualTo("Item to consume not found in inventory"));
        }

        [Test]
        public void Consume_fruit_adds_seeds_and_removes_fruit_from_inventory()
        {
            _gameState.Map = new AreaMap(5, 5);
            var actor = new Actor();
            var item = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            actor.AddToInventory(item);

            Assert.That(actor.Inventory.Count(), Is.EqualTo(1));

            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item);
            _dispatcher.CommandActor(actor, cmd);

            Assert.That(actor.Inventory.Count(), Is.EqualTo(1));
            if (actor.Inventory.ElementAt(0) is Seed seed)
                Assert.That(seed.PlantType, Is.EqualTo(PlantType.Healer));
            else
                Assert.Fail("Item wasn't seeds");
        }

        [Test]
        public void Consume_wielded()  //  Wield food?  That's allowed.
        {
            _gameState.Map = new AreaMap(5, 5);
            var actor = new Actor();
            var item = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            actor.Wield(item);

            Assert.That(actor.Inventory.Count(), Is.EqualTo(1));
            Assert.That(actor.WieldedTool, Is.SameAs(item));

            var cmd = new Command(CmdAction.Consume, CmdDirection.None, item);
            _dispatcher.CommandActor(actor, cmd);

            Assert.That(actor.WieldedTool, Is.Null);
            Assert.That(actor.Inventory.Count(), Is.EqualTo(1));
            Assert.That(actor.Inventory.ElementAt(0), Is.Not.SameAs(item));
        }
        #endregion

        #region Direction
        [TestCase(CmdDirection.East, 12)]
        [TestCase(CmdDirection.Northeast, 17)]
        public void Direction_commands_take_time(CmdDirection direction, int tickOff)
        {
            var areaMap = CreateSmallTestMap();
            _gameState.Map = areaMap;
            var actor = new Actor(new Point(2,2));

            var cmd = new Command(CmdAction.Direction, direction, null);
            _dispatcher.CommandActor(actor, cmd);
            __schedule.Received().AddActor(actor, tickOff);
        }

        public AreaMap CreateSmallTestMap()
        {
            AreaMap areaMap = new AreaMap(5, 5);
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    bool isEdge = x == 0 || y == 0 || x == 4 || y == 4;
                    var tt = new TileType
                    {
                        IsTransparent = !isEdge,
                        IsWalkable = !isEdge,
                        Symbol = isEdge ? '#' : '.',
                        Name = isEdge ? "wall" : "floor"
                    };
                    var t = new Tile(x, y, tt);
                    areaMap.SetTile(t);
                }
            }

            return areaMap;
        }


        #endregion

        #region Drop
        [Test]
        public void Drop_throws_on_item_not_in_inventory()
        {
            var actor = new Actor();
            var item = new HealerSeed(new Point(0, 0), 1);
            var drop = new Command(CmdAction.Drop, CmdDirection.None, item);

            var ex = Assert.Throws<Exception>(() => _dispatcher.CommandActor(actor, drop));
            Assert.That(ex.Message, Is.EqualTo("Item to drop not found in inventory"));
        }

        [Test]
        public void Drop_removes_item_from_inventory()
        {
            _gameState.Map = new AreaMap(5, 5);
            var actor = new Actor();
            var item = new HealerSeed(new Point(0, 0), 1);
            actor.AddToInventory(item);

            Assert.That(actor.Inventory.Count(), Is.EqualTo(1));

            var drop = new Command(CmdAction.Drop, CmdDirection.None, item);
            _dispatcher.CommandActor(actor, drop);

            Assert.That(actor.Inventory.Count(), Is.EqualTo(0));
        }

        [Test]
        public void Drop_wielded()
        {
            _gameState.Map = new AreaMap(5, 5);
            var actor = new Actor();
            var item = new Hoe(new Point(0, 0));
            actor.Wield(item);

            Assert.That(actor.Inventory.Count(), Is.EqualTo(1));
            Assert.That(actor.WieldedTool, Is.SameAs(item));

            var drop = new Command(CmdAction.Drop, CmdDirection.None, item);
            _dispatcher.CommandActor(actor, drop);

            Assert.That(actor.WieldedTool, Is.Null);
            Assert.That(actor.Inventory.Count(), Is.EqualTo(0));
        }

        [Test]
        public void Drop_at_actor_location()
        {
            _gameState.Map = new AreaMap(5, 5);
            var actor = new Actor(new Point(2, 2));
            var item = new HealerSeed(new Point(0, 0), 1);
            actor.AddToInventory(item);

            var drop = new Command(CmdAction.Drop, CmdDirection.None, item);
            _dispatcher.CommandActor(actor, drop);

            var mapItems = _gameState.Map.Items;
            Assert.That(mapItems.Count, Is.EqualTo(1));
            Assert.That(mapItems[0], Is.SameAs(item));
            Assert.That(item.Point, Is.EqualTo(new Point(2, 2)));
        }

        //[Test] // defer until 0.3+
        private void Drop_one_from_stack()
        {
            _gameState.Map = new AreaMap(5, 5);
            var actor = new Actor(new Point(2, 2));
            var item = new HealerSeed(new Point(0, 0), 3);
            actor.AddToInventory(item);

            var drop = new Command(CmdAction.Drop, CmdDirection.None, item);
            _dispatcher.CommandActor(actor, drop);

            var mapItems = _gameState.Map.Items;
            Assert.That(mapItems.Count, Is.EqualTo(1));
            Assert.That(mapItems[0], Is.Not.SameAs(item));
            Assert.That(mapItems[0].Quantity, Is.EqualTo(1));
            Assert.That(item.Quantity, Is.EqualTo(2));
        }
        #endregion

        [TestCase(CmdAction.Consume, CmdDirection.None, 2)]
        [TestCase(CmdAction.Drop, CmdDirection.None, 1)]
        [TestCase(CmdAction.Wait, CmdDirection.None, 6)]
        public void Commands_take_time(CmdAction action, CmdDirection direction, int tickOff)
        {
            _gameState.Map = new AreaMap(5, 5);
            var actor = new Actor();
            var item = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            actor.AddToInventory(item);

            var cmd = new Command(action, direction, item);
            _dispatcher.CommandActor(actor, cmd);
            __schedule.Received().AddActor(actor, tickOff);
        }

        #region Pick Up
        #endregion

        #region Use
        #endregion

        #region Wait
        #endregion

        #region Wield
        #endregion
    }
}
