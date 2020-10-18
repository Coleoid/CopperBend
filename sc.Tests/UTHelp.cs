using System.Collections.ObjectModel;
using CopperBend.Contract;
using SadConsole.Components;
using SadConsole.Entities;
using NSubstitute;

namespace CopperBend.Fabric.Tests
{
    public static class UTHelp
    {
        // only needed for .Cosmogenesis() -> .ConnectSocialRegister() -> .CreatePlayer()
        public static ISadConEntityFactory GetSubstituteFactory()
        {
            var __factory = Substitute.For<ISadConEntityFactory>();
            __factory
               .When(f => f.SetIEntityOnPort(Arg.Any<IEntityInitPort>()))
               .Do(ci => {
                   var pi = ci.Arg<IEntityInitPort>();
                   var ie = Substitute.For<IEntity>();
                   var comps = Substitute.For<ObservableCollection<IConsoleComponent>>();
                   ie.Components.Returns(comps);
                   pi.SadConEntity = ie;
               });

            return __factory;
        }
    }
}
