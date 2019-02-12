using NSubstitute;
using NUnit.Framework;
using RLNET;
using System.Collections.Generic;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class InputCommandSourceTestsBase
    {
        protected Queue<RLKeyPress> _inQ;
        protected IGameWindow _gameWindow;
        protected InputCommandSource _source;
        protected IActor _actor;
        protected EventBus _bus;

        [SetUp]
        public virtual void SetUp()
        {
            _inQ = new Queue<RLKeyPress>();
            _gameWindow = Substitute.For<IGameWindow>();
            _bus = new EventBus();
            _source = new InputCommandSource(_inQ, new Describer(), _gameWindow, _bus);
            _actor = Substitute.For<IActor>();
        }

        [TearDown]
        public virtual void TearDown()
        {
            _inQ = null;
            _gameWindow = null;
            _source = null;
            _actor = null;
        }

        protected Command Cmd = new Command(CmdAction.Unset, CmdDirection.None);
        protected static readonly Command CommandNone = new Command(CmdAction.None, CmdDirection.None);
        protected static readonly RLKeyPress KP_Question = KeyPressFrom(RLKey.Slash, shift: true);

        protected static RLKeyPress KeyPressFrom(RLKey key, bool alt = false, bool shift = false, bool control = false, bool repeating = false, bool numLock = false, bool capsLock = false, bool scrollLock = false)
        {
            return new RLKeyPress(key, alt, shift, control, repeating, numLock, capsLock, scrollLock);
        }

        protected void Queue(RLKey key)
        {
            RLKeyPress press = KeyPressFrom(key);
            Queue(press);
        }
        protected void Queue(RLKeyPress press)
        {
            _inQ.Enqueue(press);
        }
    }
}
