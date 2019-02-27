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

        private IMessageOutput __output = null;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            __gameWindow.Run(
                Arg.Do<UpdateEventHandler>(x => _onUpdate = x), 
                Arg.Do<UpdateEventHandler>(x => _onRender = x));

            _schedule = new Schedule();
            _gameState = new GameState { Player = __actor };
            _describer = new Describer();
            _mapLoader = new MapLoader();

            __output = Substitute.For<IMessageOutput>();

            _dispatcher = new CommandDispatcher(_schedule, _gameState, _describer, _bus, __output);
            _engine = new GameEngine(_bus, _schedule, __gameWindow, _inQ, _mapLoader, _gameState, _dispatcher);
            //  Those are some chunky boys.
            //  Perhaps more of these (InputQueue, Describer, Schedule) belong in GameState
            //  The Bus and the ControlPanel are still fighting it out...
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

            var calls = __gameWindow.ReceivedCalls();
            var args = calls.Single(c => c.GetMethodInfo().Name == "Run").GetArguments();
            Assert.That(args[0], Is.SameAs(_onUpdate));
            Assert.That(args[1], Is.SameAs(_onRender));

            //  So after SetUp(), calling _onUpdate(foo, bar)
            // will look like a callback "from the GameWindow"
        }

        [Test]
        public void OnUpdate_queues_all_input()
        {
            _engine.PushMode(EngineMode.Schedule, null);
            _schedule.Add(icp => icp.EnterMode(null, EngineMode.Pause, null), 1);
            __gameWindow.GetKeyPress().Returns(KeyPressFrom(RLKey.Left), KeyPressFrom(RLKey.Up), (RLKeyPress)null);

            _engine.Run();
            _onUpdate(null, null);

            Assert.That(_inQ.Count, Is.EqualTo(2));
        }

        [Test]
        public void OnUpdate_runs_actions_until_mode_blocks()
        {
            _engine.PushMode(EngineMode.Schedule, null);

            int scheduledActionsCalled = 0;
            _schedule.Add(icp => scheduledActionsCalled++, 12);
            _schedule.Add(icp => scheduledActionsCalled++, 12);
            _schedule.Add(icp => icp.EnterMode(null, EngineMode.Pause, null), 12);

            _engine.Run();
            _onUpdate(null, null);

            Assert.That(scheduledActionsCalled, Is.EqualTo(2));
            Assert.That(_engine.CurrentMode, Is.EqualTo(EngineMode.Pause));
        }

        [Test]
        public void PauseMode_stops_engine_from_running_further_actions()
        {
            _engine.PushMode(EngineMode.Schedule, null);

            int scheduledActionsCalled = 0;
            _schedule.Add(icp => icp.EnterMode(this, EngineMode.Pause, () => false), 1);
            _schedule.Add(icp => scheduledActionsCalled++, 12);

            _engine.Run();
            _onUpdate(null, null);

            Assert.That(scheduledActionsCalled, Is.EqualTo(0));
            Assert.That(_engine.CurrentMode, Is.EqualTo(EngineMode.Pause));
        }

        [Test]
        public void Leaving_PauseMode_will_not_run_schedule_until_next_update()
        {
            Func<bool> pauseForever = () => false;
            _engine.PushMode(EngineMode.Schedule, null);
            _engine.PushMode(EngineMode.Pause, pauseForever);

            int scheduledActionsCalled = 0;
            _schedule.Add(icp => scheduledActionsCalled++, 12);
            _schedule.Add(icp => icp.EnterMode(null, EngineMode.Pause, null), 999);

            _engine.Run();
            _onUpdate(null, null);
            _engine.PopMode();
            Assert.That(scheduledActionsCalled, Is.EqualTo(0));
            Assert.That(_engine.CurrentMode, Is.EqualTo(EngineMode.Schedule));

            _onUpdate(null, null);
            Assert.That(scheduledActionsCalled, Is.EqualTo(1));
        }


        [Test]
        public void ICS_changes_engine_mode_and_sets_callback_when_command_unfinished()
        {
            _engine.PushMode(EngineMode.Schedule, null);
            __actor.Inventory.Returns(new List<IItem> {new Fruit(new Point(0, 0), 1, PlantType.Boomer)});
            Queue(RLKey.D);
            var ics = new InputCommandSource(_inQ, _describer, __gameWindow, _bus, __controls);
            ics.GiveCommand(__actor);

            Assert.That(_engine.CurrentMode, Is.EqualTo(EngineMode.InputBound));
            Assert.That(_engine.CurrentCallback, Is.Not.Null);

            // this half may be a later test...
            __controls.CommandActor(null, CommandIncomplete).ReturnsForAnyArgs(true);
            __controls.DidNotReceive().CommandActor(Arg.Any<IActor>(), Arg.Any<Command>());
            Queue(RLKey.A);
            _engine.ActOnMode();

            __controls.Received().CommandActor(Arg.Any<IActor>(), Arg.Any<Command>());
            Assert.That(_engine.CurrentMode, Is.EqualTo(EngineMode.Schedule));
            Assert.That(_engine.CurrentCallback, Is.Null);
        }

        [Test]
        public void ICS_does_not_change_engine_mode_or_set_callback_when_command_completed()
        {
            _engine.PushMode(EngineMode.Schedule, null);
            Queue(RLKey.Left);
            var ics = new InputCommandSource(_inQ, _describer, __gameWindow, _bus, __controls);
            __controls.CommandActor(null, CommandIncomplete).ReturnsForAnyArgs(true);
            ics.GiveCommand(__actor);

            __controls.Received().CommandActor(Arg.Any<IActor>(), Arg.Any<Command>());
            Assert.That(_engine.CurrentMode, Is.EqualTo(EngineMode.Schedule));
            Assert.That(_engine.CurrentCallback, Is.Null);
        }

    }
}
