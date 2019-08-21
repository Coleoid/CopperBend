using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using NSubstitute;
using log4net.Config;
using log4net;
using log4net.Repository;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;

namespace CopperBend.Engine.tests
{

    [TestFixture]
    public class DispatcherTests
    {
        private CommandDispatcher _dispatcher = null;
        private ISchedule __schedule = null;
        private GameState _gameState = null;
        private Describer __describer = null;
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
            __schedule = Substitute.For<ISchedule>();
            _gameState = new GameState();
            __describer = Substitute.For<Describer>(88);
            __messageOutput = Substitute.For<IMessageLogWindow>();

            //_gameState.Map = CreateSmallTestMap();

            _dispatcher = new CommandDispatcher(__schedule, _gameState, __describer, __messageOutput);
        }

        //public AreaMap CreateSmallTestMap()
        //{
        //    var ttWall = new TileType
        //    {
        //        IsTransparent = false,
        //        IsWalkable = false,
        //        Symbol = '#',
        //        Name = "wall"
        //    };
        //    var ttFloor = new TileType
        //    {
        //        IsTransparent = true,
        //        IsWalkable = true,
        //        Symbol = '.',
        //        Name = "floor"
        //    };
        //    AreaMap areaMap = new AreaMap(5, 5);
        //    for (int x = 0; x < 5; x++)
        //    {
        //        for (int y = 0; y < 5; y++)
        //        {
        //            bool isEdge = x == 0 || y == 0 || x == 4 || y == 4;
        //            var t = new Tile(x, y, isEdge ? ttWall : ttFloor);
        //            areaMap.SetTile(t);
        //        }
        //    }

        //    return areaMap;
        //}

        //[Explicit, Property("Context", "ConsoleRunning")]
        [TestCase(CmdAction.Wait, CmdDirection.None, 6)]
        [TestCase(CmdAction.Consume, CmdDirection.None, 2)]
        [TestCase(CmdAction.Drop, CmdDirection.None, 1)]
        [TestCase(CmdAction.Wield, CmdDirection.None, 6)]
        public void Commands_take_time(CmdAction action, CmdDirection direction, int tickOff)
        {
            ScEntityFactory.ReturnNull = true;
            //if (!Debugger.IsAttached)
            //    Debugger.Launch();
            //Assert.That(SadConsole.Global.GraphicsDevice, Is.Not.Null);
            //Assert.That(SadConsole.Game.Instance.GraphicsDevice, Is.Not.Null);

            var actor = new Player(Color.AliceBlue, Color.Black);
            var item = new Fruit((0, 0), 1, Fruit.PlantByName["Healer"]);
            actor.AddToInventory(item);

            var cmd = new Command(action, direction, item);
            _dispatcher.CommandBeing(actor, cmd);
            __schedule.Received().AddAgent(actor, tickOff);
        }


        #region Consume
        [Test]
        public void Consume_throws_on_item_not_in_inventory()
        {
            //var actor = new Actor();
            //var item = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            //var cmd = new Command(CmdAction.Consume, CmdDirection.None, item);

            //var ex = Assert.Throws<Exception>(() => _dispatcher.CommandActor(actor, cmd));
            //Assert.That(ex.Message, Is.EqualTo("Item to consume not found in inventory"));
        }

        [Test]
        public void Consume_fruit_adds_seeds_and_removes_fruit_from_inventory()
        {
            //_gameState.Map = new AreaMap(5, 5);
            //var actor = new Actor();
            //var item = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            //actor.AddToInventory(item);

            //Assert.That(actor.Inventory.Count(), Is.EqualTo(1));

            //var cmd = new Command(CmdAction.Consume, CmdDirection.None, item);
            //_dispatcher.CommandActor(actor, cmd);

            //Assert.That(actor.Inventory.Count(), Is.EqualTo(1));
            //if (actor.Inventory.ElementAt(0) is Seed seed)
            //    Assert.That(seed.PlantType, Is.EqualTo(PlantType.Healer));
            //else
            //    Assert.Fail("Item wasn't seeds");
        }

        [Test]
        public void Consume_wielded()  //  Wield food?  That's allowed.
        {
            //_gameState.Map = new AreaMap(5, 5);
            //var actor = new Player(Color.AntiqueWhite, Color.Black, '?');
            //var item = new Fruit(new Point(0, 0), 1, PlantType.Healer);
            //actor.Wield(item);

            //Assert.That(actor.Inventory.Count(), Is.EqualTo(1));
            //Assert.That(actor.WieldedTool, Is.SameAs(item));

            //var cmd = new Command(CmdAction.Consume, CmdDirection.None, item);
            //_dispatcher.CommandBeing(actor, cmd);

            //Assert.That(actor.WieldedTool, Is.Null);
            //Assert.That(actor.Inventory.Count(), Is.EqualTo(1));
            //Assert.That(actor.Inventory.ElementAt(0), Is.Not.SameAs(item));
        }
        #endregion

        #region Direction
        [TestCase(CmdDirection.East, 12)]
        [TestCase(CmdDirection.Northeast, 17)]
        public void Direction_commands_take_time(CmdDirection direction, int tickOff)
        {
            //var actor = new Actor(new Point(2, 2));

            //var cmd = new Command(CmdAction.Direction, direction, null);
            //_dispatcher.CommandActor(actor, cmd);
            //__schedule.Received().AddActor(actor, tickOff);
        }

        [TestCase(CmdDirection.East, 3, 2)]
        [TestCase(CmdDirection.Northeast, 3, 1)]
        public void Direction_commands_change_location(CmdDirection direction, int newX, int newY)
        {
            //var actor = new Actor(new Point(2, 2));

            //var cmd = new Command(CmdAction.Direction, direction, null);
            //_dispatcher.CommandActor(actor, cmd);
            //Assert.That(actor.Point, Is.EqualTo(new Point(newX, newY)));
        }

        [TestCase(CmdDirection.East, true)]
        [TestCase(CmdDirection.Northeast, false)]
        public void Moving_to_unwalkable_tile_does_nothing(CmdDirection direction, bool isPlayer)
        {
            //var actor = new Actor(new Point(3, 2));
            //actor.IsPlayer = isPlayer;

            //var cmd = new Command(CmdAction.Direction, direction, null);
            //_dispatcher.CommandActor(actor, cmd);
            //Assert.That(actor.Point, Is.EqualTo(new Point(3, 2)));
            //__schedule.DidNotReceive().AddActor(actor, Arg.Any<int>());

            //var outputOrNot = isPlayer
            //    ? (Func<IMessageOutput>)__messageOutput.Received
            //    : __messageOutput.DidNotReceive;
            //outputOrNot().WriteLine("I can't walk through a wall.");
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
            //var actor = new Actor();
            //var item = new HealerSeed(new Point(0, 0), 1);
            //var drop = new Command(CmdAction.Drop, CmdDirection.None, item);

            //var ex = Assert.Throws<Exception>(() => _dispatcher.CommandActor(actor, drop));
            //Assert.That(ex.Message, Is.EqualTo("Item to drop not found in inventory"));
        }

        [Test]
        public void Drop_moves_item_from_inventory_to_map()
        {
            //Point actorlocation = new Point(2, 3);
            //var actor = new Actor(actorlocation);
            //var item = new HealerSeed(new Point(0, 0), 1);
            //actor.AddToInventory(item);

            //Assert.That(actor.Inventory.Count(), Is.EqualTo(1));

            //var drop = new Command(CmdAction.Drop, CmdDirection.None, item);
            //_dispatcher.CommandActor(actor, drop);

            //Assert.That(actor.Inventory.Count(), Is.EqualTo(0));
            //Assert.That(_gameState.Map.Items.Count, Is.EqualTo(1));
            //var droppedItem = _gameState.Map.Items[0];
            //Assert.That(droppedItem.Point, Is.EqualTo(actorlocation));
        }

        [Test]
        public void Drop_wielded()
        {
            //var actor = new Actor();
            //var item = new Hoe(new Point(0, 0));
            //actor.Wield(item);

            //Assert.That(actor.Inventory.Count(), Is.EqualTo(1));
            //Assert.That(actor.WieldedTool, Is.SameAs(item));

            //var drop = new Command(CmdAction.Drop, CmdDirection.None, item);
            //_dispatcher.CommandActor(actor, drop);

            //Assert.That(actor.WieldedTool, Is.Null);
            //Assert.That(actor.Inventory.Count(), Is.EqualTo(0));
        }

        //[Test] // defer until 0.3+
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
            //Point startingPoint = new Point(2, 2);
            //var actor = new Actor(startingPoint);

            //var cmd = new Command(CmdAction.PickUp, CmdDirection.None, null);
            //_dispatcher.CommandActor(actor, cmd);

            //__schedule.DidNotReceive().AddActor(actor, Arg.Any<int>());
        }

        [Test]
        public void PickUp_takes_time()
        {
            //Point startingPoint = new Point(2, 2);
            //var actor = new Actor(startingPoint);
            //_gameState.Map.Items.Add(new Fruit(startingPoint, 1, PlantType.Healer));
            //var cmd = new Command(CmdAction.PickUp, CmdDirection.None, null);
            //_dispatcher.CommandActor(actor, cmd);

            //__schedule.Received().AddActor(actor, 4);
        }

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
