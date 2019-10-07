namespace CopperBend.Contract
{
    public interface IScheduleAgent
    {
        ScheduleEntry GetNextEntry(int offset);
    }
}
