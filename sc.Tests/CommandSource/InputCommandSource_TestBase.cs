﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SadConsole.Input;
using log4net;
using log4net.Config;
using log4net.Repository;
using CopperBend.Contract;
using CopperBend.Fabric;
using NSubstitute;
using NUnit.Framework;

namespace CopperBend.Engine.Tests
{
    [TestFixture]
    public class InputCommandSource_TestBase
    {
        protected InputCommandSource _source;
        protected GameState _gameState = null;
        protected Queue<AsciiKey> _inQ;
        protected IBeing __being;
        protected IControlPanel __controls;
        protected IMessageLogWindow __messageOutput = null;

        #region OTSU
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
        #endregion

        [SetUp]
        public virtual void SetUp()
        {
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
            __controls = Substitute.For<IControlPanel>();
            __controls.IsInputReady = () => _inQ.Count() > 0;
            __controls.GetNextInput = () => _inQ.Dequeue();
            __controls.ClearPendingInput = () => _inQ.Clear();
            //__messageOutput = Substitute.For<IMessageLogWindow>();
            _source = new InputCommandSource(new Describer(), _gameState, __controls);
            __being = Substitute.For<IBeing>();
            _inQ = new Queue<AsciiKey>();
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

        [TearDown]
        public virtual void TearDown()
        {
            _inQ = null;
            _source = null;
            __being = null;
        }

        protected Command Cmd = new Command(CmdAction.Unset, CmdDirection.None);
        protected static readonly Command CommandIncomplete = new Command(CmdAction.Incomplete, CmdDirection.None);

        protected void Queue(Keys xnaKey)
        {
            var key = new AsciiKey { Character = (char)xnaKey, Key = xnaKey };
            _inQ.Enqueue(key);
        }

        protected void Queue(AsciiKey key)
        {
            _inQ.Enqueue(key);
        }
    }
}
