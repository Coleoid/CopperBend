using System;
using CopperBend.Contract;
using log4net;

namespace CopperBend.Fabric
{
    public class ServicePanel : IServicePanel
    {
        public event EventHandler GameEngine_Startup;
        public event EventHandler GameEngine_Shutdown;
        public event EventHandler<GameDataEventArgs> NewGame_Startup;
        public event EventHandler<GameDataEventArgs> ExistingGame_Load;

        public void Notify_GameEngine_Startup()
        {
            GameEngine_Startup?.Invoke(this, new StartupEventArgs());
        }

        public void Notify_GameEngine_Shutdown()
        {
            GameEngine_Shutdown?.Invoke(this, new EventArgs());
        }

        public void Notify_NewGame_Startup(GameDataEventArgs args)
        {
            NewGame_Startup?.Invoke(this, args);
        }

        public void Notify_ExistingGame_Load()
        {
            ExistingGame_Load?.Invoke(this, new GameDataEventArgs(null, null));  //WRONG
        }


        public IDescriber Describer { get; private set; }
        public IMessager Messager { get; private set; }
        public ILog Log { get; private set; }
        public IGameMode GameMode { get; private set; }
        public ISchedule Schedule { get; private set; }

        public IServicePanel Register(IPanelService service)
        {
            service.RegisterWithPanel(this);

            switch (service)
            {
            case IDescriber desc:
                Describer = desc;
                break;

            case IMessager msgr:
                Messager = msgr;
                break;

            case IGameMode mode:
                GameMode = mode;
                break;

            case ISchedule sched:
                Schedule = sched;
                break;

            default:
                throw new Exception($"Not ready to register type {service.GetType().Name}");
            }

            return this;
        }

        public IServicePanel Register(ILog logService)
        {
            Log = logService;
            return this;
        }
    }
}
