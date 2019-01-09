namespace CopperBend.App
{
    public interface IComponent
    {
        IActor Entity { get; }
    }

    public interface IHealAndHurt : IComponent
    {
        int Health { get; }
        void Heal(int amount);
        void Hurt(int amount);
    }
}
