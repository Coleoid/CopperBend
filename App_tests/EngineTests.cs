using NSubstitute;
using NUnit.Framework;
using RLNET;
using System.Collections.Generic;
using CopperBend.App.Model;
using CopperBend.MapUtil;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class EngineTests
    {
        private IGameWindow _gameWindow = null;
        private GameState _gameState = null;

        private Queue<RLKeyPress> _inQ = null;
        private Queue<GameCommand> _cmdQ = null;
        private MapLoader _mapLoader = null;
        private CommandDispatcher _dispatcher = null;
        private GameEngine _engine = null;
        private Schedule _schedule = null;
        private Describer _describer = null;
        private EventBus _bus = null;
        private UpdateEventHandler _onUpdate = null;
        private UpdateEventHandler _onRender = null;
        private IActor _player = null;

        [SetUp]
        public void SetUp()
        {
            _gameWindow = Substitute.For<IGameWindow>();
            _gameWindow.Run(Arg.Do<UpdateEventHandler>(x => _onUpdate = x), Arg.Do<UpdateEventHandler>(x => _onRender = x));

            _gameState = new GameState();
            _player = Substitute.For<IActor>();
            _gameState.Player = _player;

            _inQ = new Queue<RLKeyPress>();
            _cmdQ = new Queue<GameCommand>();
            _mapLoader = new MapLoader();
            _schedule = new Schedule();
            _describer = new Describer();
            _bus = new EventBus();
            _dispatcher = new CommandDispatcher(_schedule, _gameWindow, _gameState, _describer, _cmdQ);
            _engine = new GameEngine(_bus, _schedule, _gameWindow, _cmdQ, _inQ, _mapLoader, _gameState, _dispatcher);
            //  Ai yi yi.
        }
        
        [Test]
        public void CreateEngine()
        {
            Assert.That(_engine, Is.Not.Null);
        }

        [Test]
        public void Run_connects_update_and_render_to_GameWindow()
        {
            _engine.Run();

            Assert.That(_onUpdate, Is.Not.Null);
            Assert.That(_onRender, Is.Not.Null);

            //  So now I can do _onUpdate(foo, bar) to send events "from the GameWindow"
        }

        RLKeyPress QuickKey(RLKey key)
        {
            return  new RLKeyPress(key, false, false, false, false, false, false, false);
        }

        [Test]
        public void OnUpdate_queues_input()
        {
            bool calledBack = false;
            _schedule.Add(icp => calledBack = true, 10);
            _engine.EnterMode(EngineMode.Schedule);

            _engine.Run();

            Command cmd = default(Command);
            _player.Command(Arg.Do<Command>(c => cmd = c));

            //_gameWindow.GetKeyPress().Returns(QuickKey(RLKey.Left), (RLKeyPress)null);

            _onUpdate(null, null);

            _player.Received().Command(Arg.Any<Command>());
            Assert.That(cmd.Action, Is.EqualTo(CmdAction.Move));

            Assert.That(_inQ.Count, Is.EqualTo(0)); // pause mode to see queued input?
        }

        [Test]
        public void ActorBuild()
        {
            IActor actor = Substitute.For<IActor>();

            var ics = new InputCommandSource(_inQ, _describer, _gameWindow);
            //actor.CommandSource = ics;

            Assert.That(actor.CommandSource, Is.Not.Null);
            _inQ.Enqueue(QuickKey(RLKey.Left));
            ics.GiveCommand(actor);

            actor.Received().Command(Arg.Any<Command>());
        }
    }
}
