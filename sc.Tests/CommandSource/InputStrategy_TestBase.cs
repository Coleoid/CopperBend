using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using SadConsole.Input;
using NSubstitute;
using NUnit.Framework;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using CopperBend.Creation;

namespace CopperBend.Logic.Tests
{
    [TestFixture]
    public class InputStrategy_TestBase : Tests_Base
    {
        protected override bool ShouldPrepDI => true;
        protected override MockableServices GetServicesToMock()
        {
            return MockableServices.Log
                | MockableServices.EntityFactory
                | MockableServices.ControlPanel
                | MockableServices.Messager
                | base.GetServicesToMock();
        }

        protected IBeing __being;

        protected BeingStrategy_UserInput _strategy { get; set; }
        protected GameState _gameState;
        protected Queue<AsciiKey> _inQ;

        [SetUp]
        public virtual void SetUp()
        {
            Basis.ConnectIDGenerator();
            Engine.Cosmogenesis("bang", __factory);
            _gameState = new GameState
            {
                Map = new CompoundMap
                {
                    BeingMap = new BeingMap(),
                    RotMap = new RotMap(),
                    SpaceMap = CreateSmallTestMap(),
                    ItemMap = new ItemMap(),
                },
            };

            _inQ = new Queue<AsciiKey>();
            __messager.GetNextKeyPress()
                .Returns((ci) => _inQ.Count > 0 ? _inQ.Dequeue() : new AsciiKey { Key = Keys.None });
            __messager.IsInputReady()
                .Returns((ci) => _inQ.Count > 0);

            _strategy = new BeingStrategy_UserInput(_gameState);
            SourceMe.InjectProperties(_strategy);

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
                    //TODO: fix my SmallTestMap
                    //bool isEdge = x == 0 || y == 0 || x == 4 || y == 4;
                    var s = new Space()
                    {
                        //Terrain = isEdge ? ttWall : ttFloor
                    };
                    spaceMap.Add(s, (x, y));
                }
            }
            //var sp = spaceMap.GetItem((3, 4));
            //sp.Terrain = ttDoorClosed;

            return spaceMap;
        }

        public (Item knife, Item fruit, Item hoe) Fill_pack()
        {
            var Equipper = SourceMe.The<Equipper>();
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
            _strategy = null;
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
