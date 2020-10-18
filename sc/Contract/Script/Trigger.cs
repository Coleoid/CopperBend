using System.Collections.Generic;

#pragma warning disable CA2227 // YAML s'zn wants collection setters
namespace CopperBend.Contract
{
    public class Trigger
    {
        /// <summary> Unique, for serializing to catalog </summary>
        public string Name { get; set; }

        /// <summary> Broad sort of event(s) pull this trigger.
        /// To avoid checking triggers which cannot be pulled. </summary>
        public TriggerCategories Categories { get; set; }

        /// <summary> Precise description of what pulls this trigger. </summary>
        public string Condition { get; set; }

        /// <summary> Actions fired when this trigger is pulled.  Implementation will change. </summary>
        public List<string> Script { get; set; }

        public Trigger()
        {
            Script = new List<string>();
        }
    }
}
#pragma warning restore CA2227 // YAML s'zn wants collection setters
