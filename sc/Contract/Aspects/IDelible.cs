namespace CopperBend.Contract
{
    public interface IDelible
    {
        string Name { get; }
        int MaxHealth { get; }
        int Health { get; }
        void Heal(int amount);
        void Hurt(int amount);
    }
}
