using GoRogue;

namespace CopperBend.Contract
{
    public interface ISpace : IHasID
    {
        uint ID { get; }
        bool CanPlant { get; }
        bool CanSeeThrough { get; }
        bool CanTill { get; }
        bool CanWalkThrough { get; }
        bool IsKnown { get; set; }
        bool IsSown { get; set; }
        bool IsTilled { get; set; }
        TerrainType Terrain { get; set; }
    }
}
