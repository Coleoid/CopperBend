using System;
using CopperBend.Contract;

namespace CopperBend.Fabric
{
    public class Compendium : IBook
    {
        public string BookType { get; set; } = "Compendium";
        public TomeOfChaos TomeOfChaos { get; set; }
        public Herbal Herbal { get; set; }
        public SocialRegister SocialRegister { get; set; }
        public Dramaticon Dramaticon { get; set; }
    }

    // The Herbal contains plant types, and what the player knows about them
    // in this run.  Also tracks changes as plants become malleable.
    public class Herbal : IBook
    {
        public string BookType { get; set; } = "Herbal";
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

    //  ===  Second wave below, current plans don't immediately need these

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
}
