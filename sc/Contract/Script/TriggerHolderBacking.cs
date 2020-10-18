using System.Collections.Generic;
using System.Collections.ObjectModel;

#pragma warning disable CA2227 // YAML s'zn wants collection setters
namespace CopperBend.Contract
{
    public class TriggerHolderBacking : ITriggerHolder
    {
        protected internal List<Trigger> Triggers { get; set; }
        public ReadOnlyCollection<Trigger> ListTriggers() => Triggers.AsReadOnly();

        public TriggerHolderBacking()
        {
            Triggers = new List<Trigger>();
        }

        public void AddTrigger(Trigger trigger)
        {
            Triggers.Add(trigger);
        }

        public void RemoveTrigger(Trigger trigger)
        {
            Triggers.Remove(trigger);
        }

    }
}
#pragma warning restore CA2227 // YAML s'zn wants collection setters
