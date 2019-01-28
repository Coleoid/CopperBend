using NSubstitute;
using NUnit.Framework;
using RLNET;
using System.Collections.Generic;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class InputCommandSourceTestsBase
    {
        protected Queue<RLKeyPress> InQ;
        protected IGameWindow Window;
        protected InputCommandSource Source;
        protected IActor Actor;
        protected Command Cmd = new Command(CmdAction.Unset, CmdDirection.None);
        protected static readonly Command CommandNone = new Command(CmdAction.None, CmdDirection.None);
        protected static readonly RLKeyPress KP_Question = KeyPressFrom(RLKey.Slash, shift: true);

        [SetUp]
        public void SetUp()
        {
            InQ = new Queue<RLKeyPress>();
            Window = Substitute.For<IGameWindow>();

            Source = new InputCommandSource(InQ, new Describer(), Window);

            Actor = Substitute.For<IActor>();
            Source.SetActor(Actor);
        }

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
            InQ.Enqueue(press);
        }
    }
}
