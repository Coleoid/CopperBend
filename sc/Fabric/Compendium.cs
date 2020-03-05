using System.Collections.Generic;
using CopperBend.Contract;
using CopperBend.Model;
using GoRogue;

#pragma warning disable SA1402 // File may only contain a single type
namespace CopperBend.Fabric
{
    public class Compendium : IBook
    {
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

    // All significant beings, their current state and relationships
    public class SocialRegister : IBook
    {
    }

    // Stories, scenes, quests, visions, dreams
    // No plan for this to change per run, so likely not in save file.
    public class Dramaticon : IBook
    {
        public bool HasClearedRot { get; internal set; }
    }

    #region ===  Second wave below, current plans don't immediately need these

    // Creatures
    public class Bestiary : IBook
    {
    }

    // Recipes and processes to create and transform
    public class Cookbook : IBook
    {
    }

    // Items, things, and stuff
    // Probably useful if and as items become defined by characteristics
    // and components instead of classes.
    public class ItemDictionary : IBook  // the lamest of names
    {
    }
    #endregion
}

#pragma warning restore SA1402 // File may only contain a single type
