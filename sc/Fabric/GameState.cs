using CopperBend.Contract;
using GoRogue;

namespace CopperBend.Fabric
{
    public class GameState : IGameState
    {
        public ICompoundMap Map { get; set; }
        public Dramaticon Story { get; internal set; }

        public void MarkDirtyCoord(Coord newPosition)
        {
            Map.CoordsWithChanges.Add(newPosition);
        }
    }
}
