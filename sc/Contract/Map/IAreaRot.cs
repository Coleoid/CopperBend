using GoRogue;

namespace CopperBend.Contract
{
    //  Is there really a compelling reason to treat these differently than other
    //  beings/creatures?
    public interface IAreaRot : IHasID, IDelible, IAttacker, IDefender
    {
    }
}
