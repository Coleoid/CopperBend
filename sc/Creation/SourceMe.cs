using System.Drawing;
using System.IO;
using System.Linq;
using log4net;
using log4net.Config;
using Autofac;
using Autofac.Core;
using SadGlobal = SadConsole.Global;
using CopperBend.Fabric;
using CopperBend.Contract;
using CopperBend.Logic;
using GoRogue;
using SadConsole.Input;
using log4net.Repository;

namespace CopperBend.Creation
{
    public static class SourceMe
    {
        public static ContainerBuilder Builder { get; set; }
        public static IContainer Container { get; set; }
        private static ILoggerRepository LogRepo { get; set; }

        public static T The<T>()
        {
            if (Container == null)
                throw new System.Exception($"Cannot Resolve {typeof(T).Name} before building Container.");

            return Container.Resolve<T>();
        }

        public static void Build(Size gameSize)
        {
            PreBuild(gameSize);

            //  When unit testing needs injected mocks:
            // SourceMe.PreBuild(),
            // Register mocks with SourceMe.Builder,
            // SourceMe.FinishBuild().
            //  Later Registrations override earlier ones in Autofac.

            FinishBuild();
        }

        public static void PreBuild(Size gameSize)
        {
            Container = null;
            Builder = GetConfiguredBuilder();

            RegisterLogger(Builder);
            RegisterGameLogic(Builder);
            RegisterFactories(Builder);
            RegisterBooks(Builder);
            RegisterSadConsole(Builder, gameSize);
        }

        public static void FinishBuild()
        {
            Container = Builder.Build();
        }

        private static void RegisterLogger(ContainerBuilder builder)
        {
            // One of the few things Log4Net is touchy about.
            if (LogRepo == null)
                LogRepo = LogManager.CreateRepository("CB");

            XmlConfigurator.Configure(LogRepo, new FileInfo("sc.log.config"));
            var logger = LogManager.GetLogger("CB", "CB");
            builder.RegisterInstance(logger).As<ILog>();
        }

        private static void RegisterGameLogic(ContainerBuilder builder)
        {
            builder.RegisterType<Schedule>().As<ISchedule>().SingleInstance();
            builder.RegisterType<Describer>().As<IDescriber>().SingleInstance();
            builder.RegisterType<GameState>().As<IGameState>().SingleInstance();
            builder.RegisterType<CommandDispatcher>().As<IControlPanel>().SingleInstance();
            builder.RegisterType<AttackSystem>().As<IAttackSystem>().As<AttackSystem>().SingleInstance();
            builder.RegisterType<GameMode>().As<IGameMode>().SingleInstance();
            builder.RegisterType<TriggerPuller>().As<ITriggerPuller>().SingleInstance();
            builder.RegisterType<Director>().SingleInstance();

            builder.RegisterType<BeingStrategy_UserInput>().SingleInstance();
        }

        private static void RegisterFactories(ContainerBuilder builder)
        {
            builder.RegisterType<IDGenerator>().SingleInstance();
            builder.RegisterType<BeingCreator>().As<IBeingCreator>().SingleInstance();
            builder.RegisterType<MapLoader>().SingleInstance();
            builder.RegisterType<Equipper>().SingleInstance();
        }

        private static void RegisterBooks(ContainerBuilder builder)
        {
            // TomeOfChaos is the breath of each world, so it is built later.
            //builder.RegisterType<TomeOfChaos>().SingleInstance();
            builder.RegisterType<BookPublisher>().SingleInstance();
            builder.RegisterType<SocialRegister>().SingleInstance();
            builder.RegisterType<Dramaticon>().SingleInstance();

            builder.Register((c, p) => {
                var publisher = c.Resolve<BookPublisher>();
                return publisher.Herbal_FromNew();
            })
            .As<Herbal>().SingleInstance();

            builder.Register((c, p) => {
                var publisher = c.Resolve<BookPublisher>();
                return publisher.Atlas_FromNew();
            })
            .As<Atlas>().SingleInstance();

            builder.Register((c, p) => {
                var publisher = c.Resolve<BookPublisher>();
                return publisher.Register_FromNew(publisher.BeingCreator);
            })
            .As<SocialRegister>().SingleInstance();
        }

        private static void RegisterSadConsole(ContainerBuilder builder, Size gameSize)
        {
            builder.Register<Keyboard>((c, p) => {
                return SadGlobal.KeyboardState;
            }).SingleInstance();

            //  I can't fire up a FontMaster until the game is running.
            //builder.Register<FontMaster>((c, p) => {
            //    var fontName = p.Named<string>("font");
            //    return SadGlobal.LoadFont(fontName);
            //});
            //var fontMaster = SadGlobal.LoadFont("Cheepicus_14x14.font");
            //  ...so I'm building two, below.  Meh.
            //  Could do a static lazy-init prop, which is also meh.

            builder.Register<SadConEntityFactory>((c, p) =>
            {
                var fm = SadGlobal.LoadFont("Cheepicus_14x14.font");
                SadConEntityFactory factory = new SadConEntityFactory(fm);
                return factory;
            }).As<ISadConEntityFactory>().SingleInstance();

            //  It's never been clearer that the Engine shouldn't inherit from Console directly.
            builder.Register<Engine>((c, p) => {
                var fm = SadGlobal.LoadFont("Cheepicus_14x14.font");
                var uiBuilder = new UIBuilder(gameSize, fm, c.Resolve<ILog>());
                var engine = new Engine(uiBuilder);
                return engine;
            }).SingleInstance();

            builder.RegisterType<Messager>().As<IMessager>().SingleInstance();
        }

        private static ContainerBuilder GetConfiguredBuilder()
        {
            var builder = new ContainerBuilder();

            //  Is the property marked with [InjectProperty], and is it null?
            IsPropertyMarked = new DelegatePropertySelector((pi, obj) => {
                var shouldInject = pi.CustomAttributes.Any(
                    cad => cad.AttributeType.Name == nameof(InjectPropertyAttribute)
                ) && pi.GetValue(obj) == null;
                return shouldInject;
            });

            //  As each instance/type is activated, also inject marked properties.
            builder.RegisterCallback(crb => {
                crb.Registered += (_, regdArgs) => {
                    regdArgs.ComponentRegistration.Activated += (_, actdArgs) =>
                        actdArgs.Context.InjectProperties(actdArgs.Instance, IsPropertyMarked);
                };
            });

            return builder;
        }

        private static DelegatePropertySelector IsPropertyMarked { get; set; }

        public static void InjectProperties(object instance)
        {
            Container.InjectProperties(instance, IsPropertyMarked);
        }
    }
}
