namespace CopperBend.Contract
{
    public interface IScheduleAgent
    {
        ScheduleEntry GetNextEntry();
        ScheduleEntry GetNextEntry(int offset);
    }
}
