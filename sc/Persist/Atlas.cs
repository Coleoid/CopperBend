using System;
using System.Collections.Generic;
using CopperBend.Contract;

namespace CopperBend.Persist
{
    public class Atlas
    {
        public Dictionary<string, TerrainType> Legend { get; set; }

        public Atlas()
        {
            Legend = new Dictionary<string, TerrainType>();
        }

        public void StoreTerrainType(TerrainType type)
        {
            if (Legend.ContainsKey(type.Name))
                throw new Exception($"Already have type {type.Name} stored.");

            Legend[type.Name] = type;
        }
    }
}
