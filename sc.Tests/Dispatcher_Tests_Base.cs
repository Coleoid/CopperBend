using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using log4net;
using log4net.Config;
using log4net.Repository;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using NSubstitute;
using NUnit.Framework;

namespace CopperBend.Engine.Tests
{
    public class Dispatcher_Tests_Base
    {
        protected CommandDispatcher _dispatcher = null;
        protected ISchedule __schedule = null;
        protected GameState _gameState = null;
        protected IDescriber __describer = null;
        protected IMessageLogWindow __messageOutput = null;

        protected TerrainType ttDoorOpen;
        protected TerrainType ttDoorClosed;
        protected TerrainType ttWall;
        protected TerrainType ttFloor;

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
                Map = new CompoundMap
                {
                    BeingMap = new GoRogue.MultiSpatialMap<IBeing>(),
                    BlightMap = new BlightMap(),
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

    }
}
