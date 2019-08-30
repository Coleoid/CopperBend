using GoRogue;

namespace CopperBend.Contract
{
    public interface IAreaBlight : IHasID, IDestroyable
    {
        int Extent { get; set; }
        int Health { get; }
        uint ID { get; }
        int MaxHealth { get; set; }

        void Heal(int amount);
        void Hurt(int amount);
    }
}
