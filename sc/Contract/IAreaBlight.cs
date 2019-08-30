using GoRogue;

namespace CopperBend.Contract
{
    public interface IAreaBlight : IHasID, IDestroyable
    {
        int Extent { get; set; }
    }
}
