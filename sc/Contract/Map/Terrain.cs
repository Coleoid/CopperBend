using SadConsole;

namespace CopperBend.Contract
{
    public class Terrain
    {
        public string Name { get; set; }
        public Cell Cell { get; set; }
        public bool CanSeeThrough { get; set; }
        public bool CanWalkThrough { get; set; }
        public bool CanPlant { get; set; }
    }
}
