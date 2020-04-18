using System;
using System.Collections.ObjectModel;
using log4net;
using NSubstitute;
using CopperBend.Contract;
using CopperBend.Fabric;
using NUnit.Framework;
using SadConsole.Entities;
using SadConsole.Components;

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
        public void BaseSetUp()
        {
            __log = Substitute.For<ILog>();
            __schedule = Substitute.For<ISchedule, IPanelService>();
            __describer = Substitute.For<IDescriber, IPanelService>();
            __messager = Substitute.For<IMessager, IPanelService>();
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

        public ISadConEntityFactory StubEntityFactory()
        {
            var fac = Substitute.For<ISadConEntityFactory>();
            fac.GetSadCon(Arg.Any<ISadConInitData>())
                .Returns(ctx => {
                    var ie = Substitute.For<IEntity>();
                    var cs = new ObservableCollection<IConsoleComponent>();
                    ie.Components.Returns(cs);
                    return ie;
                });

            return fac;
        }

    }
}
