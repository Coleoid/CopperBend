namespace CopperBend.App
{
    public interface IScheduleable
    {
        int TicksUntilNextAction { get; }
    }
}
