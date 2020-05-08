using System.Collections.Generic;
using GoRogue;

namespace CopperBend.Persist
{
#pragma warning disable CA2227 // YAML s'zn wants collection setters
    public class RotOverlayData
    {
        public string Name { get; set; }
        public Coord Location { get; set; }
        public List<string> Terrain { get; set; } = new List<string>();
    }
#pragma warning restore CA2227
}
