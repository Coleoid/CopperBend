//  Functional completeness levels:
//  0.0  ---  Code is needed, and doesn't exist or work at all
//  0.1  ---  Placeholder code that doesn't completely fail
//  0.2  ---  Code is less lame, yet not ready for initial release
//  0.K  ---  Ready for initial release
//  1.+  ---  Speculative thought for after initial release
//  +.+  ---  Quality beyond initial release needs, guess I was inspired

using System;
using CopperBend.Fabric;
using CopperBend.Logic;
using log4net;

namespace CopperBend.Contract
{
    public interface IServicePanel
    {
        event EventHandler GameEngine_Startup;
        event EventHandler GameEngine_Shutdown;
        event EventHandler<GameDataEventArgs> NewGame_Startup;
        event EventHandler<GameDataEventArgs> ExistingGame_Load;

        void Notify_GameEngine_Startup();
        void Notify_GameEngine_Shutdown();
        void Notify_NewGame_Startup(GameDataEventArgs args);
        void Notify_ExistingGame_Load();

        IDescriber Describer { get; }
        IMessager Messager { get; }
        ILog Log { get; }
        IGameMode GameMode { get; }
        ISchedule Schedule { get; }

        IServicePanel Register(IPanelService service);
        IServicePanel Register(ILog logService);
    }

    public interface IPanelService
    {
        void RegisterWithPanel(IServicePanel isp);
    }

    public class StartupEventArgs : EventArgs
    {
        // Should I create custom EventArgs classes?
        // Should I rely on subscribers knowing caller is IServicePanel?
    }

    public class GameDataEventArgs : EventArgs
    {
        public TomeOfChaos TomeOfChaos { get; }
        public Herbal Herbal { get; }
        public GameDataEventArgs(TomeOfChaos tome, Herbal herbal)
            : base()
        {
            this.TomeOfChaos = tome;
            this.Herbal = herbal;
        }
    }
}
