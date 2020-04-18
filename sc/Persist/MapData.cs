using System.Collections.Generic;

namespace CopperBend.Persist
{
#pragma warning disable CA2227 // YAML s'zn wants collection setters
    public class MapData
    {
        public string Name { get; set; }
        public Dictionary<string, string> Legend { get; set; } = new Dictionary<string, string>();
        public List<string> Terrain { get; set; } = new List<string>();
        public List<RotOverlayData> Rot { get; set; } = new List<RotOverlayData>();
        public List<string> FirstSightMessage { get; set; } = new List<string>();
    }
#pragma warning restore CA2227
}
