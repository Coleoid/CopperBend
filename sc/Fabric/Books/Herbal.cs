using System.Collections.Generic;
using CopperBend.Contract;
using CopperBend.Model;

namespace CopperBend.Fabric
{
    // The Herbal contains plant types, and what the player knows about them
    // in this run.  Also tracks changes as plants become malleable.
    public class Herbal : IBook
    {
        public Herbal()
        {
            PlantByID = new Dictionary<uint, PlantDetails>();
            PlantByName = new Dictionary<string, PlantDetails>();
        }

        public Dictionary<uint, PlantDetails> PlantByID { get; internal set; }
        public Dictionary<string, PlantDetails> PlantByName { get; internal set; }

        public void AddPlant(PlantDetails plant)
        {
            PlantByID[plant.ID] = plant;
            PlantByName[plant.MainName] = plant;
        }
    }
}
