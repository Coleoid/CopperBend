using System.Collections.Generic;

namespace CopperBend.Contract
{
    public interface ITriggerPuller
    {
        IEnumerable<Trigger> TriggersInScope { get; }

        void AddTriggerHolderToScope(ITriggerHolder holder);
        void Check(TriggerCategories categories);
    }
}
