namespace CopperBend.App
{
    public interface IScheduler
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