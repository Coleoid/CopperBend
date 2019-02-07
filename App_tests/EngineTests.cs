using NSubstitute;
using NUnit.Framework;
using RLNET;
using System.Collections.Generic;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class EngineTests : InputCommandSourceTestsBase
    {
        private Schedule _schedule = null;
        private GameState _gameState = null;
        private MapLoader _mapLoader = null;
        private Describer _describer = null;
        private EventBus _bus = null;
        private CommandDispatcher _dispatcher = null;
        private GameEngine _engine = null;

        private UpdateEventHandler _onUpdate = null;
        private UpdateEventHandler _onRender = null;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _gameWindow.Run(Arg.Do<UpdateEventHandler>(x => _onUpdate = x), Arg.Do<UpdateEventHandler>(x => _onRender = x));

            _schedule = new Schedule();
            _gameState = new GameState { Player = _actor };
            _describer = new Describer();
            _bus = new EventBus();
            _mapLoader = new MapLoader();

            _dispatcher = new CommandDispatcher(_schedule, _gameWindow, _gameState, _describer, _bus);
            _engine = new GameEngine(_bus, _schedule, _gameWindow, _inQ, _mapLoader, _gameState, _dispatcher);
            //  Ai yi yi.
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            _schedule = null;
            _gameState = null;
            _mapLoader = null;
            _describer = null;
            _bus = null;
            _dispatcher = null;
            _engine = null;

            _onUpdate = null;
            _onRender = null;
        }

        [Test]
        public void CreateEngine()
        {
            Assert.That(_engine, Is.Not.Null);
        }

        [Test]
        public void Run_connects_update_and_render_to_GameWindow()
        {
            Assert.That(_onUpdate, Is.Null);
            Assert.That(_onRender, Is.Null);

            _engine.Run();

            Assert.That(_onUpdate, Is.Not.Null);
            Assert.That(_onRender, Is.Not.Null);
            //  So now I can do _onUpdate(foo, bar) to send events "from the GameWindow"
        }

        [Test]
        public void OnUpdate_queues_all_input()
        {
            _engine.EnterMode(EngineMode.Schedule);
            _schedule.Add(icp => icp.EnterMode(null, EngineMode.Pause), 999);
            _gameWindow.GetKeyPress().Returns(KeyPressFrom(RLKey.Left), KeyPressFrom(RLKey.Up), (RLKeyPress)null);

            _engine.Run();
            _onUpdate(null, null);

            Assert.That(_inQ.Count, Is.EqualTo(2));
        }

        [Test]
        public void OnUpdate_runs_actions_until_mode_blocks()
        {
            _engine.EnterMode(EngineMode.Schedule);

            int scheduledActionsCalled = 0;
            _schedule.Add(icp => scheduledActionsCalled++, 12);
            _schedule.Add(icp => scheduledActionsCalled++, 12);
            _schedule.Add(icp => icp.EnterMode(null, EngineMode.Pause), 12);

            _engine.Run();
            _onUpdate(null, null);

            Assert.That(scheduledActionsCalled, Is.EqualTo(2));
            Assert.That(_engine.Mode, Is.EqualTo(EngineMode.Pause));
        }
    }
}
