//0.2:  Later, collect everything which is saved/loaded, and stateful.

using System;
using CopperBend.Engine;

namespace CopperBend.Contract
{
    public interface IGameState
    {
        ICompoundMap Map { get; }
        IBeing Player { get; }
        bool PlayerMoved { get; set; }
        Action<EngineMode, Func<bool>> PushEngineMode { get; }
        Action ClearPendingInput { get; }
    }

    public class GameState : IGameState
    {
        public ICompoundMap Map { get; set; }
        public IBeing Player { get; set; }
        public bool PlayerMoved { get; set; }

        public Action<EngineMode, Func<bool>> PushEngineMode { get; set; }
        public Action ClearPendingInput { get; set; }
    }
}
