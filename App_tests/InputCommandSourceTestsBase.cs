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
        protected IGameWindow __gameWindow;
        protected InputCommandSource _source;
        protected IActor __actor;
        protected EventBus _bus;
        protected IControlPanel __controls;

        [SetUp]
        public virtual void SetUp()
        {
            _inQ = new Queue<RLKeyPress>();
            __gameWindow = Substitute.For<IGameWindow>();
            _bus = new EventBus();
            __controls = Substitute.For<IControlPanel>();
            _source = new InputCommandSource(_inQ, new Describer(), __gameWindow, _bus, __controls);
            __actor = Substitute.For<IActor>();
        }

        [TearDown]
        public virtual void TearDown()
        {
            _inQ = null;
            __gameWindow = null;
            _source = null;
            __actor = null;
        }

        protected Command Cmd = new Command(CmdAction.Unset, CmdDirection.None);
        protected static readonly Command CommandIncomplete = new Command(CmdAction.Incomplete, CmdDirection.None);
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
