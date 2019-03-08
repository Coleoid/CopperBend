using NSubstitute;
using NUnit.Framework;
using CopperBend.App.Model;
using CopperBend.MapUtil;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class Dispatcher_Use_Tests
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
            _gameState.Map = CreateSmallTestMap();
            _gameState.Map.TileTypes["tilled dirt"] = new TileType
            {
                Name = "tilled dirt", Symbol = '~', IsTransparent = true, IsWalkable = true,
            };
        }
        private AreaMap CreateSmallTestMap()
        {
            var ttWall = new TileType
            {
                IsTransparent = false,
                IsWalkable = false,
                Symbol = '#',
                Name = "wall"
            };
            var ttFloor = new TileType
            {
                IsTransparent = true,
                IsWalkable = true,
                Symbol = '.',
                Name = "floor"
            };
            AreaMap areaMap = new AreaMap(5, 5);
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    bool isEdge = x == 0 || y == 0 || x == 4 || y == 4;
                    var t = new Tile(x, y, isEdge ? ttWall : ttFloor);
                    areaMap.SetTile(t);
                }
            }

            return areaMap;
        }

        [Test]
        public void Use_hoe_on_untillable_tile()
        {
            Point startingPoint = new Point(2, 2);
            var actor = new Actor(startingPoint);
            var hoe = new Hoe(startingPoint);
            actor.AddToInventory(hoe);
            actor.WieldedTool = hoe;

            var cmd = new Command(CmdAction.Use, CmdDirection.North, hoe);
            _dispatcher.CommandActor(actor, cmd);

            __schedule.DidNotReceive().AddActor(actor, Arg.Any<int>());
            __messageOutput.Received().WriteLine("Cannot till the floor.");
        }

        [Test]
        public void Use_hoe_tills_ground_in_direction()
        {
            Point startingPoint = new Point(2, 2);
            var actor = new Actor(startingPoint);
            var hoe = new Hoe(startingPoint);
            actor.AddToInventory(hoe);
            actor.WieldedTool = hoe;

            Tile soil = new Tile(2, 1, new TileType { IsTillable = true, IsTransparent = true, IsWalkable = true, Name = "soil", Symbol = '.' });
            _gameState.Map.SetTile(soil);

            var cmd = new Command(CmdAction.Use, CmdDirection.North, hoe);
            _dispatcher.CommandActor(actor, cmd);

            Assert.That(soil.IsTilled);
            __schedule.Received().AddActor(actor, 15);
        }
    }
}
