using System;
using Microsoft.Xna.Framework;
using log4net;
using NSubstitute;
using NUnit.Framework;
using SadConsole.Entities;
using CopperBend.Contract;
using CopperBend.Fabric;
using System.Collections.ObjectModel;
using SadConsole.Components;

namespace CopperBend.Logic.Tests
{
    public class Dispatcher_Tests_Base
    {
        protected CommandDispatcher _dispatcher = null;

        protected ILog __log = null;
        protected ISchedule __schedule = null;
        protected GameState _gameState = null;
        protected IDescriber __describer = null;
        protected IMessageLogWindow __msgLogWindow = null;
        protected ISadConEntityFactory __factory = null;
        protected IMessager __messager = null;
        protected BeingCreator BeingCreator;

        protected TerrainType ttFloor;
        protected TerrainType ttWall;
        protected TerrainType ttDoorOpen;
        protected TerrainType ttDoorClosed;
        protected TerrainType ttSoil;
        protected TerrainType ttSoilTilled;
        protected TerrainType ttSoilPlanted;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            __log = Substitute.For<ILog>();

            ttFloor = new TerrainType
            {
                CanSeeThrough = true,
                CanWalkThrough = true,
                Looks = new SadConsole.Cell(Color.White, Color.Black, '.'),
                Name = "floor"
            };
            ttWall = new TerrainType
            {
                CanSeeThrough = false,
                CanWalkThrough = false,
                Looks = new SadConsole.Cell(Color.White, Color.Black, '#'),
                Name = "wall"
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

            ttSoil = new TerrainType
            {
                CanSeeThrough = true,
                CanWalkThrough = true,
                Looks = new SadConsole.Cell(Color.Brown, Color.Black, '.'),
                Name = Terrains.Soil,
                CanPlant = true,
            };
            ttSoilTilled = new TerrainType
            {
                CanSeeThrough = true,
                CanWalkThrough = true,
                Looks = new SadConsole.Cell(Color.Brown, Color.Black, '~'),
                Name = Terrains.SoilTilled,
                CanPlant = true,
            };
            ttSoilPlanted = new TerrainType
            {
                CanSeeThrough = true,
                CanWalkThrough = true,
                Looks = new SadConsole.Cell(Color.Green, Color.Black, '~'),
                Name = Terrains.SoilPlanted,
                CanPlant = false,
            };

            SpaceMap.TerrainTypes[ttFloor.Name] = ttFloor;
            SpaceMap.TerrainTypes[ttWall.Name] = ttWall;
            SpaceMap.TerrainTypes[ttDoorOpen.Name] = ttDoorOpen;
            SpaceMap.TerrainTypes[ttDoorClosed.Name] = ttDoorClosed;
            SpaceMap.TerrainTypes[ttSoil.Name] = ttSoil;
            SpaceMap.TerrainTypes[ttSoilTilled.Name] = ttSoilTilled;
            SpaceMap.TerrainTypes[ttSoilPlanted.Name] = ttSoilPlanted;

            __factory = Substitute.For<ISadConEntityFactory>();
            __factory.GetSadCon(Arg.Any<ISadConInitData>())
                .Returns(ctx => {
                    var ie = Substitute.For<IEntity>();
                    var cs = new ObservableCollection<IConsoleComponent>();
                    ie.Components.Returns(cs);
                    return ie;
                });

            Engine.Cosmogenesis("bang", __factory);
            BeingCreator = Engine.BeingCreator;
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
                    RotMap = new RotMap(),
                    SpaceMap = CreateSmallTestMap(),
                    ItemMap = new ItemMap(),
                },
            };
            __describer = Substitute.For<IDescriber>();
            __msgLogWindow = Substitute.For<IMessageLogWindow>();
            __messager = Substitute.For<IMessager>();

            _dispatcher = new CommandDispatcher(__log, __schedule, _gameState, __describer, __messager);
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
                    spaceMap.Add(s, (x, y));
                }
            }
            var sp = spaceMap.GetItem((3, 4));
            sp.Terrain = ttDoorClosed;

            return spaceMap;
        }
    }
}
