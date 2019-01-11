namespace CopperBend.App
{
    public interface ISchedule
    {
        int CurrentTick { get; }

        void Add(ScheduleEntry toAct);
        void Clear();
        void DoNext(IControlPanel controls);
        ScheduleEntry GetNext();
        void Remove(ScheduleEntry scheduleEntry);
        void RemoveActor(IActor targetActor);
    }
}