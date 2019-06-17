namespace CopperBend.Contract
{
    public interface IDestroyable
    {
        int MaxHealth { get; }
        int Health { get; }
        void Heal(int amount);
        void Hurt(int amount);
    }
}
