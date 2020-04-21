using System;
using System.Collections.Generic;
using CopperBend.Contract;

namespace CopperBend.Fabric
{
    public class Atlas : IBook
    {
        public Dictionary<string, Terrain> Legend { get; private set; }

        public Atlas()
        {
            Legend = new Dictionary<string, Terrain>();
        }

        public void StoreTerrain(Terrain terrain)
        {
            if (Legend.ContainsKey(terrain.Name))
                throw new Exception($"Already have terrain {terrain.Name} stored.");

            Legend[terrain.Name] = terrain;
        }
    }
}
