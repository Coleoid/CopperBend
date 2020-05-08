using log4net;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Fabric.Tests;
using NSubstitute;
using NUnit.Framework;

namespace CopperBend.Logic.Tests
{
    [TestFixture]
    public class Tests_Base
    {
        protected ILog __log = null;
        protected ISchedule __schedule = null;
        protected IDescriber __describer = null;
        protected ISadConEntityFactory __factory = null;
        protected IMessager __messager = null;

        [SetUp]
        public void Tests_Base_SetUp()
        {
            __log = Substitute.For<ILog>();
            __schedule = Substitute.For<ISchedule, IPanelService>();
            __describer = Substitute.For<IDescriber, IPanelService>();
            __messager = Substitute.For<IMessager, IPanelService>();
            __factory = UTHelp.GetSubstituteFactory();
        }

        public IServicePanel StubServicePanel()
        {
            var isp = new ServicePanel()
                .Register(__describer as IPanelService)
                .Register(__messager as IPanelService)
                .Register(__schedule as IPanelService)
                .Register(__log);

            return isp;
        }
    }
}
