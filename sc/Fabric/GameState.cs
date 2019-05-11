using CopperBend.Contract;

namespace CopperBend.Fabric
{
    public class GameState : IGameState
    {
        public ICompoundMap Map { get; set; }
        public IBeing Player { get; set; }
    }
}
