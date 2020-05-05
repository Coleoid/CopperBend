using System.Collections.Generic;
using CopperBend.Contract;
using GoRogue;

#pragma warning disable CA2227 // YAML s'zn wants collection setters
namespace CopperBend.Persist
{
    public class CompoundMapData
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Dictionary<string, string> Legend { get; set; }
        public List<string> Terrain { get; set; }
        public Dictionary<Coord, IAreaRot> AreaRots { get; set; }
        public Dictionary<Coord, IBeing> Beings { get; set; }
        public Dictionary<Coord, IItem> Items { get; set; }
        public List<string> FirstSightMessage { get; set; }

        public CompoundMapData()
        {
            Legend = new Dictionary<string, string>();
            Terrain = new List<string>();
            AreaRots = new Dictionary<Coord, IAreaRot>();
            Beings = new Dictionary<Coord, IBeing>();
            Items = new Dictionary<Coord, IItem>();
            FirstSightMessage = new List<string>();
        }
    }

    //public class CompoundMapData
    //{
    //    public string Name { get; set; }
    //    public Dictionary<string, string> Legend { get; set; } = new Dictionary<string, string>();
    //    public List<string> Terrain { get; set; } = new List<string>();
    //    public List<RotOverlayData> Rot { get; set; } = new List<RotOverlayData>();
    //    public List<string> FirstSightMessage { get; set; } = new List<string>();
    //}
}
#pragma warning restore CA2227 // YAML s'zn wants collection setters
