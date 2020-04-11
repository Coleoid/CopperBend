using CopperBend.Contract;
using NSubstitute;
using SadConsole.Components;
using SadConsole.Entities;
using System.Collections.ObjectModel;

namespace CopperBend.Fabric.Tests
{
    public static class UTHelp
    {
        public static ISadConEntityFactory GetSubstituteFactory()
        {
            var factory = Substitute.For<ISadConEntityFactory>();
            factory
                .GetSadCon(Arg.Any<ISadConInitData>())
                .Returns((ci) => {
                    var sce = Substitute.For<IEntity>();
                    var comps = Substitute.For<ObservableCollection<IConsoleComponent>>();
                    sce.Components.Returns(comps);
                    return sce;
                });
            return factory;
        }
    }
}
