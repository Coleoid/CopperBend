//using CopperBend.Contract;
//using CopperBend.Fabric;
//using NUnit.Framework;

//namespace CopperBend.Engine.tests
//{
//    [TestFixture]
//    public class Dispatcher_Use_Tests
//    {
//        private CommandDispatcher _dispatcher = null;
//        private ISchedule __schedule = null;
//        private GameState _gameState = null;
//        private Describer __describer = null;

//        [SetUp]
//        public void SetUp()
//        {
//            __schedule = Substitute.For<ISchedule>();
//            _gameState = new GameState();
//            __describer = Substitute.For<Describer>(88);
//            __eventBus = new EventBus();
//            __messageOutput = Substitute.For<IMessageOutput>();

//            _dispatcher = new CommandDispatcher(__schedule, _gameState, __describer, __eventBus, __messageOutput);
//            _gameState.Map = CreateSmallTestMap();
//            _gameState.Map.TileTypes["tilled dirt"] = new TileType
//            {
//                Name = "tilled dirt",
//                Symbol = '~',
//                IsTransparent = true,
//                IsWalkable = true,
//            };
//        }
//        private AreaMap CreateSmallTestMap()
//        {
//            var ttWall = new TileType
//            {
//                IsTransparent = false,
//                IsWalkable = false,
//                Symbol = '#',
//                Name = "wall"
//            };
//            var ttFloor = new TileType
//            {
//                IsTransparent = true,
//                IsWalkable = true,
//                Symbol = '.',
//                Name = "floor"
//            };
//            AreaMap areaMap = new AreaMap(5, 5);
//            for (int x = 0; x < 5; x++)
//            {
//                for (int y = 0; y < 5; y++)
//                {
//                    bool isEdge = x == 0 || y == 0 || x == 4 || y == 4;
//                    var t = new Tile(x, y, isEdge ? ttWall : ttFloor);
//                    areaMap.SetTile(t);
//                }
//            }

//            return areaMap;
//        }

//        private (Actor, Point) SU_actor_at_point(int x, int y)
//        {
//            Point startingPoint = new Point(x, y);
//            var actor = new Actor(startingPoint);
//            return (actor, startingPoint);
//        }

//        #region Hoe/Tilling
//        [Test]
//        public void Use_hoe_on_untillable_tile()
//        {
//            (var actor, var startingPoint) = SU_actor_at_point(2, 2);
//            var hoe = new Hoe(startingPoint);
//            actor.AddToInventory(hoe);
//            actor.WieldedTool = hoe;

//            var cmd = new Command(CmdAction.Use, CmdDirection.North, hoe);
//            _dispatcher.CommandActor(actor, cmd);

//            __schedule.DidNotReceive().AddActor(actor, Arg.Any<int>());
//            __messageOutput.Received().WriteLine("Cannot till the floor.");
//        }

//        [Test]
//        public void Use_hoe_on_tilled_tile()
//        {
//            (var actor, var startingPoint) = SU_actor_at_point(2, 2);
//            var hoe = new Hoe(startingPoint);
//            actor.AddToInventory(hoe);
//            actor.WieldedTool = hoe;

//            Tile soil = new Tile(2, 1, new TileType { IsTillable = true, IsTransparent = true, IsWalkable = true, Name = "soil", Symbol = '.' });
//            _dispatcher.Till(soil);
//            _gameState.Map.SetTile(soil);

//            var cmd = new Command(CmdAction.Use, CmdDirection.North, hoe);
//            _dispatcher.CommandActor(actor, cmd);

//            __schedule.DidNotReceive().AddActor(actor, Arg.Any<int>());
//            __messageOutput.Received().WriteLine("Already tilled.");
//        }

//        [Test]
//        public void Use_hoe_tills_ground_in_direction()
//        {
//            (var actor, var startingPoint) = SU_actor_at_point(2, 2);
//            var hoe = new Hoe(startingPoint);
//            actor.AddToInventory(hoe);
//            actor.WieldedTool = hoe;

//            Tile soil = new Tile(2, 1, new TileType { IsTillable = true, IsTransparent = true, IsWalkable = true, Name = "soil", Symbol = '.' });
//            _gameState.Map.SetTile(soil);

//            var cmd = new Command(CmdAction.Use, CmdDirection.North, hoe);
//            _dispatcher.CommandActor(actor, cmd);

//            Assert.That(soil.IsTilled);
//        }

//        [TestCase(true, 15)]
//        [TestCase(false, 21)]
//        public void Use_unwielded_hoe_takes_longer_and_wields_it(bool startsWielded, int tickOff)
//        {
//            (var actor, var startingPoint) = SU_actor_at_point(2, 2);
//            var hoe = new Hoe(startingPoint);
//            actor.AddToInventory(hoe);
//            if (startsWielded)
//                actor.WieldedTool = hoe;

//            Tile soil = new Tile(2, 1, new TileType { IsTillable = true, IsTransparent = true, IsWalkable = true, Name = "soil", Symbol = '.' });
//            _gameState.Map.SetTile(soil);

//            var cmd = new Command(CmdAction.Use, CmdDirection.North, hoe);
//            _dispatcher.CommandActor(actor, cmd);

//            Assert.That(soil.IsTilled);
//            Assert.That(actor.WieldedTool, Is.SameAs(hoe));
//            __schedule.Received().AddActor(actor, tickOff);
//        }
//        #endregion

//        #region Seed/Planting
//        [Test]
//        public void Use_seed_on_untilled_tile()
//        {
//            (var actor, var startingPoint) = SU_actor_at_point(2, 2);
//            var seed = new HealerSeed(startingPoint, 1);
//            actor.AddToInventory(seed);

//            var cmd = new Command(CmdAction.Use, CmdDirection.North, seed);
//            _dispatcher.CommandActor(actor, cmd);

//            __schedule.DidNotReceive().AddActor(actor, Arg.Any<int>());
//            __messageOutput.Received().WriteLine("Cannot sow floor.");
//        }
//        #endregion
//    }
//}
