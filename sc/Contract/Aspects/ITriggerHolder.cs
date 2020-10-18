using System.Collections.ObjectModel;

namespace CopperBend.Contract
{
    public interface ITriggerHolder
    {
        // Triggers will often be one-time events, removed when they fire.
        // This API allows removal during iteration.
        void AddTrigger(Trigger trigger);
        void RemoveTrigger(Trigger trigger);
        ReadOnlyCollection<Trigger> ListTriggers();
    }
}
