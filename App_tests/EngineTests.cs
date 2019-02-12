using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using RLNET;
using System.Linq;
using CopperBend.App.Model;
using CopperBend.MapUtil;
using System;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class EngineTests : InputCommandSourceTestsBase
    {
        private Schedule _schedule = null;
        private GameState _gameState = null;
        private MapLoader _mapLoader = null;
        private Describer _describer = null;
        private CommandDispatcher _dispatcher = null;
        private GameEngine _engine = null;

        private UpdateEventHandler _onUpdate = null;
        private UpdateEventHandler _onRender = null;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _gameWindow.Run(
                Arg.Do<UpdateEventHandler>(x => _onUpdate = x), 
                Arg.Do<UpdateEventHandler>(x => _onRender = x));

            _schedule = new Schedule();
            _gameState = new GameState { Player = _actor };
            _describer = new Describer();
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

            var calls = _gameWindow.ReceivedCalls();
            var args = calls.Single(c => c.GetMethodInfo().Name == "Run").GetArguments();
            Assert.That(args[0], Is.SameAs(_onUpdate));
            Assert.That(args[1], Is.SameAs(_onRender));

            //  So after SetUp(), calling _onUpdate(foo, bar)
            // will look like a callback "from the GameWindow"
        }

        [Test]
        public void OnUpdate_queues_all_input()
        {
            _engine.EnterMode(EngineMode.Schedule, null);
            _schedule.Add(icp => icp.EnterMode(null, EngineMode.Pause, null), 999);
            _gameWindow.GetKeyPress().Returns(KeyPressFrom(RLKey.Left), KeyPressFrom(RLKey.Up), (RLKeyPress)null);

            _engine.Run();
            _onUpdate(null, null);

            Assert.That(_inQ.Count, Is.EqualTo(2));
        }

        [Test]
        public void OnUpdate_runs_actions_until_mode_blocks()
        {
            _engine.EnterMode(EngineMode.Schedule, null);

            int scheduledActionsCalled = 0;
            _schedule.Add(icp => scheduledActionsCalled++, 12);
            _schedule.Add(icp => scheduledActionsCalled++, 12);
            _schedule.Add(icp => icp.EnterMode(null, EngineMode.Pause, null), 12);

            _engine.Run();
            _onUpdate(null, null);

            Assert.That(scheduledActionsCalled, Is.EqualTo(2));
            Assert.That(_engine.Mode, Is.EqualTo(EngineMode.Pause));
        }

        [Test]
        public void PauseMode_stops_engine_from_running_further_actions()
        {
            _engine.EnterMode(EngineMode.Schedule, null);

            int scheduledActionsCalled = 0;
            _schedule.Add(icp => icp.EnterMode(null, EngineMode.Pause, null), 11);
            _schedule.Add(icp => scheduledActionsCalled++, 12);

            _engine.Run();
            _onUpdate(null, null);

            Assert.That(scheduledActionsCalled, Is.EqualTo(0));
            Assert.That(_engine.Mode, Is.EqualTo(EngineMode.Pause));
        }

        [Test]
        public void Leaving_PauseMode_will_not_run_schedule_until_next_update()
        {
            Func<IControlPanel, bool> pauseForever = (icp) => false;
            _engine.EnterMode(EngineMode.Schedule, null);
            _engine.EnterMode(EngineMode.Pause, pauseForever);

            int scheduledActionsCalled = 0;
            _schedule.Add(icp => scheduledActionsCalled++, 12);
            _schedule.Add(icp => icp.EnterMode(null, EngineMode.Pause, null), 999);

            _engine.Run();
            _onUpdate(null, null);
            _engine.LeaveMode();
            Assert.That(scheduledActionsCalled, Is.EqualTo(0));

            _onUpdate(null, null);
            Assert.That(scheduledActionsCalled, Is.EqualTo(1));
        }


        [Test]
        public void ICS_changes_engine_mode_and_sets_callback_when_command_unfinished()
        {
            _engine.EnterMode(EngineMode.Schedule, null);
            Queue(RLKey.D);
            var ics = new InputCommandSource(_inQ, _describer, _gameWindow, _bus);
            ics.GiveCommand(_actor);

            Assert.That(_engine.Mode, Is.EqualTo(EngineMode.InputBound));
            Assert.That(_engine.ModeCallback, Is.Not.Null);

            // this half may be a later test...
            _actor.Inventory.Returns(new List<IItem> {new Fruit(new Point(0, 0), 1, PlantType.Boomer)});
            _actor.DidNotReceive().Command(Arg.Any<Command>());

            Queue(RLKey.A);
            _engine.ActOnMode();

            _actor.Received().Command(Arg.Any<Command>());
            Assert.That(_engine.Mode, Is.EqualTo(EngineMode.Schedule));
            Assert.That(_engine.ModeCallback, Is.Null);
        }

        [Test]
        public void ICS_does_not_change_engine_mode_or_set_callback_when_command_completed()
        {
            _engine.EnterMode(EngineMode.Schedule, null);
            Queue(RLKey.Left);
            var ics = new InputCommandSource(_inQ, _describer, _gameWindow, _bus);
            ics.GiveCommand(_actor);

            _actor.Received().Command(Arg.Any<Command>());
            Assert.That(_engine.Mode, Is.EqualTo(EngineMode.Schedule));
            Assert.That(_engine.ModeCallback, Is.Null);
        }

    }
}
