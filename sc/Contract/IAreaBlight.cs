using GoRogue;
using System.Collections.Generic;

namespace CopperBend.Contract
{
    public interface IAreaBlight : IHasID, IDestroyable
    {
        int Extent { get; set; }
    }
}
