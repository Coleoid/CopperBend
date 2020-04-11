using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SadConsole.Input;
using log4net;
using NSubstitute;
using NUnit.Framework;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using SadConsole.Entities;
using SadConsole.Components;

namespace CopperBend.Logic.Tests
{
    [TestFixture]
    public class ICS_TestBase
    {
        protected ILog __log;
        protected InputCommandSource _source;
        protected GameState _gameState = null;
        protected Queue<AsciiKey> _inQ;
        protected IBeing __being;
        protected IControlPanel __controls;
        protected IMessager __messager;

        #region OTSU
        protected TerrainType ttDoorOpen;
        protected TerrainType ttDoorClosed;
        protected TerrainType ttWall;
        protected TerrainType ttFloor;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            __log = Substitute.For<ILog>();

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

            SpaceMap.TerrainTypes[ttWall.Name] = ttWall;
            SpaceMap.TerrainTypes[ttFloor.Name] = ttFloor;
            SpaceMap.TerrainTypes[ttDoorOpen.Name] = ttDoorOpen;
            SpaceMap.TerrainTypes[ttDoorClosed.Name] = ttDoorClosed;

            var sef = Substitute.For<ISadConEntityFactory>();
            sef.GetSadCon(Arg.Any<ISadConInitData>())
                .Returns(ctx => {
                    var ie = Substitute.For<IEntity>();
                    var cs = new ObservableCollection<IConsoleComponent>();
                    ie.Components.Returns(cs);
                    return ie;
                });

            Engine.Cosmogenesis("bang", sef);
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
                    RotMap = new RotMap(),
                    SpaceMap = CreateSmallTestMap(),
                    ItemMap = new ItemMap(),
                },
            };
            __controls = Substitute.For<IControlPanel>();
            __messager = Substitute.For<IMessager>();
            _inQ = new Queue<AsciiKey>();
            __messager.GetNextKeyPress()
                .Returns((ci) => _inQ.Count > 0 ? _inQ.Dequeue() : new AsciiKey { Key = Keys.None });
            __messager.IsInputReady()
                .Returns((ci) => _inQ.Count > 0);

            _source = new InputCommandSource(__log, new Describer(), _gameState, __controls, new ModeNode(__log), __messager);
            __being = Substitute.For<IBeing>();
            __being.IsPlayer = true;
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

        public (Item knife, Item fruit, Item hoe) Fill_pack()
        {
            var knife = Equipper.BuildItem("knife");
            var fruit = Equipper.BuildItem("fruit:Healer");
            var hoe = Equipper.BuildItem("hoe");
            __being.Inventory.Returns(new List<IItem> { knife, fruit, hoe });

            return (knife, fruit, hoe);
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

        protected void Queue(params Keys[] xnaKeys)
        {
            var state = new KeyboardState(xnaKeys, false, true);
            foreach (var xnaKey in xnaKeys)
            {
                var key = AsciiKey.Get(xnaKey, state);
                //var key = new AsciiKey { Character = (char)xnaKey, Key = xnaKey };
                _inQ.Enqueue(key);
            }
        }
    }
}
