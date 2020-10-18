using GoRogue;
using CopperBend.Contract;

namespace CopperBend.Fabric
{
    public class GameState : IGameState
    {
        public ICompoundMap Map { get; set; }
        public IBeing Player { get; set; }

        public void MarkDirtyCoord(Coord newPosition)
        {
            Map.CoordsWithChanges.Add(newPosition);
        }
    }
}
