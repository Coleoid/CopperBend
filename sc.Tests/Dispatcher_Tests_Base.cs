using CopperBend.Contract;
using CopperBend.Creation;
using CopperBend.Fabric;
using NSubstitute;
using NUnit.Framework;

namespace CopperBend.Logic.Tests
{
    public class Dispatcher_Tests_Base : Tests_Base
    {
        protected IControlPanel _controls = null;
        protected IGameState _gameState = null;

        protected IBeingCreator BeingCreator;

        protected Terrain ttFloor;
        protected Terrain ttWall;
        protected Terrain ttDoorOpen;
        protected Terrain ttDoorClosed;
        protected Terrain ttSoil;
        protected Terrain ttSoilTilled;
        protected Terrain ttSoilPlanted;

        protected override MockableServices GetServicesToMock()
        {
            return MockableServices.EntityFactory
                | MockableServices.MessageLogWindow
                | base.GetServicesToMock();
        }
        protected override bool ShouldPrepDI => true;

        [SetUp]
        public void Dispatcher_Tests_Base_SetUp()
        {
            Engine.Cosmogenesis("bang", __factory);

            var atlas = SourceMe.The<Atlas>();
            var legend = atlas.Legend;
            ttFloor = legend[TerrainEnum.Floor];
            ttWall = legend[TerrainEnum.Wall];
            ttDoorOpen = legend[TerrainEnum.DoorOpen];
            ttDoorClosed = legend[TerrainEnum.DoorClosed];
            ttSoil = legend[TerrainEnum.Soil];
            ttSoilTilled = legend[TerrainEnum.SoilTilled];
            ttSoilPlanted = legend[TerrainEnum.SoilPlanted];

            _gameState = SourceMe.The<IGameState>();
            _gameState.Map = new CompoundMap
            {
                BeingMap = new BeingMap(),
                RotMap = new RotMap(),
                SpaceMap = CreateSmallTestMap(),
                ItemMap = new ItemMap(),
            };

            _controls = SourceMe.The<IControlPanel>();
            BeingCreator = SourceMe.The<IBeingCreator>();
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

        protected void Assert_MLW_WL_Iff_Player(bool isPlayer, string message)
        {
            if (isPlayer)
                __msgLogWindow.Received().WriteLine(message);
            else
                __msgLogWindow.DidNotReceive().WriteLine(message);
        }

        protected void Assert_Messager_WL_Iff_Player(bool isPlayer, string message)
        {
            if (isPlayer)
                __messager.Received().WriteLine(message);
            else
                __messager.DidNotReceive().WriteLine(message);
        }
    }
}
