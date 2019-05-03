//0.2:  Later, collect everything which is saved/loaded, and stateful.

using CopperBend.Engine;

namespace CopperBend.Contract
{
    public interface IGameState
    {
        ICompoundMap Map { get; }
        IBeing Player { get; }
    }

    public class GameState : IGameState
    {
        public ICompoundMap Map { get; set; }
        public IBeing Player { get; set; }
    }
}
