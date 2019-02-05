using DryIoc;
using NSubstitute;
using NUnit.Framework;
using RLNET;
using System;
using System.Collections.Generic;

namespace CopperBend.App.tests
{
    [TestFixture]
    public class EngineTests
    {
        private Container Container = null;
        private IResolverContext Scope = null;

        [SetUp]
        public void SetUp()
        {
            Container = new Container(rules => rules
                .WithDefaultReuse(Reuse.InCurrentScope)
                .WithUnknownServiceResolvers(request => {
                    var serviceType = request.ServiceType;
                    if (!serviceType.IsAbstract)
                        return null; // Mock interface or abstract class only.

                    return new ReflectionFactory(made: Made.Of(
                        () => Substitute.For(
                            DryIoc.Arg.Index<Type[]>(0),
                            DryIoc.Arg.Index<object[]>(1)
                        ),
                        _ => new[] { serviceType }, _ => (object[])null));
                })
            );

            Container.Register<EventBus, EventBus>();
            Container.Register<Describer, Describer>();
            Container.Register<Schedule, Schedule>();

            Container.Register<Queue<GameCommand>, Queue<GameCommand>>(Made.Of(() => new Queue<GameCommand>()));
            Container.Register<Queue<RLKeyPress>, Queue<RLKeyPress>>(Made.Of(() => new Queue<RLKeyPress>()));
            Container.Register<MapLoader, MapLoader>();
            Container.Register<IGameState, GameState>();
            //let IGameWindow hit the 'unknown service resolvers' rule
            Container.Register<CommandDispatcher, CommandDispatcher>();

            Container.Register<GameEngine, GameEngine>();

            Scope = Container.OpenScope();
        }

        [TearDown]
        public void TearDown()
        {
            Scope.Dispose();
        }

        
        [Test]
        public void CreateEngine()
        {
            GameEngine engine = Scope.Resolve<GameEngine>();

            Assert.That(engine, Is.Not.Null);
        }
    }
}
