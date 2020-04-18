using System;

namespace CopperBend.Contract
{
    public interface IGameMode
    {
        EngineMode CurrentMode { get; }
        Action CurrentCallback { get; }
        void PushEngineMode(EngineMode newMode, Action callback);
        void PopEngineMode();
    }
}
