//  Should grow to be complete, for save/load.

namespace CopperBend.App
{
    public interface IGameState
    {
        IAreaMap Map { get; }
        IActor Player { get; }
        EngineMode Mode { get; set; }
    }

    public class GameState : IGameState
    {
        public IAreaMap Map { get; set; }
        public IActor Player { get; set; }
        public EngineMode Mode { get; set; }
    }
}
