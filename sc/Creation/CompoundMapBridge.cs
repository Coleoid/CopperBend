using System.Collections.Generic;
using CopperBend.Contract;
using GoRogue;

#pragma warning disable CA2227 // Allow collection setters for YAML s'zn
namespace CopperBend.Creation
{
    /// <summary>
    /// CompoundMapBridge is an easy-to-YAML version of a whole map.
    /// Class Cartographer builds CMBs from CompoundMaps and vice versa.
    /// MapLoader coordinates the whole process.
    /// The current main difference (Sep 2020) is Multibeings/MultiItems.
    /// </summary>
    public class CompoundMapBridge
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Dictionary<string, string> Legend { get; set; }
        public List<string> Terrain { get; set; }
        public Dictionary<Coord, IAreaRot> AreaRots { get; set; }

        public Dictionary<uint, Coord_IBeing> MultiBeings { get; set; }
        public Dictionary<uint, Coord_IItem> MultiItems { get; set; }
        public List<Trigger> Triggers { get; set; }


        public CompoundMapBridge()
        {
            Legend = new Dictionary<string, string>();
            Terrain = new List<string>();
            AreaRots = new Dictionary<Coord, IAreaRot>();

            MultiBeings = new Dictionary<uint, Coord_IBeing>();
            MultiItems = new Dictionary<uint, Coord_IItem>();

            Triggers = new List<Trigger>();
        }
    }
}
#pragma warning restore CA2227 // Allow collection setters for YAML s'zn
