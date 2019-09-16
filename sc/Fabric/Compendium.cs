using System.Collections.Generic;
using CopperBend.Contract;
using GoRogue;

namespace CopperBend.Fabric
{
    public class Compendium : IBook
    {
        public string BookType { get; set; } = "Compendium";
        public TomeOfChaos TomeOfChaos { get; set; }
        public Herbal Herbal { get; set; }
        public SocialRegister SocialRegister { get; set; }
        public Dramaticon Dramaticon { get; set; }
        public IDGenerator IDGenerator { get; internal set; }
    }

    // The Herbal contains plant types, and what the player knows about them
    // in this run.  Also tracks changes as plants become malleable.
    public class Herbal : IBook
    {
        public string BookType { get; set; } = "Herbal";
        public Dictionary<uint, PlantDetails> PlantByID { get; internal set; }
        public Dictionary<string, PlantDetails> PlantByName { get; internal set; }

        public Herbal()
        {
            PlantByID = new Dictionary<uint, PlantDetails>();
            PlantByName = new Dictionary<string, PlantDetails>();
        }

        public void AddPlant(PlantDetails plant)
        {
            PlantByID[plant.ID] = plant;
            PlantByName[plant.MainName] = plant;
        }
    }

    // All significant beings, their current state and relationships
    public class SocialRegister : IBook
    {
        public string BookType { get; set; } = "SocialRegister";
    }

    // Stories, scenes, quests, visions, dreams
    // No plan for this to change per run, so likely not in save file.
    public class Dramaticon : IBook
    {
        public string BookType { get; set; } = "Dramaticon";
    }

    #region ===  Second wave below, current plans don't immediately need these

    // Creatures
    public class Bestiary : IBook
    {
        public string BookType { get; set; } = "Bestiary";
    }

    // Recipes and processes to create and transform
    public class Cookbook : IBook
    {
        public string BookType { get; set; } = "Cookbook";
    }

    // Items, things, and stuff
    // Probably useful if and as items become defined by characteristics
    // and components instead of classes.
    public class ItemDictionary : IBook  // the lamest of names
    {
        public string BookType { get; set; } = "NamestOfLames";
    }
    #endregion
}
