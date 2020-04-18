using System.Collections.Generic;

namespace CopperBend.Persist
{
#pragma warning disable CA2227 // YAML s'zn wants collection setters
    public class RotOverlayData
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public List<string> Terrain { get; set; } = new List<string>();
    }
#pragma warning restore CA2227
}
