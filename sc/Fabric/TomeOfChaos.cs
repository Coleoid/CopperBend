using CopperBend.Contract;

namespace CopperBend.Fabric
{
    public class Compendium : IBook
    {
        public TomeOfChaos TomeOfChaos { get; set; }
        public Herbal Herbal { get; set; }
        public SocialRegister SocialRegister { get; set; }
        public Dramaticon Dramaticon { get; set; }
    }
    
    // The Tome of Chaos contains all the RNGs for the game.
    // We have multiple RNGs with their own responsibilities, to create
    // essentially repeatable worlds from an initial seed, regardless of
    // the path the player takes in the world.  This should improve debug
    // work, and demotivate savescumming to re-spin the wheel of treasure.
    public class TomeOfChaos : IBook
    { }

    // The Herbal contains plant types, and what the player knows about them
    // in this run.  Also tracks changes as plants become malleable.
    public class Herbal : IBook
    { }

    // All significant beings, their current state and relationships
    public class SocialRegister : IBook
    { }

    // Stories, scenes, quests, visions, dreams
    // No plan for this to change per run, so likely not in save file.
    public class Dramaticon : IBook
    { }

    //  ===  Second wave below, current plans don't immediately need these

    // Creatures
    public class Bestiary : IBook
    { }

    // Recipes and processes to create and transform
    public class Cookbook : IBook
    { }

    // Items, things, and stuff
    // Probably useful if and as items become defined by characteristics
    // and components instead of classes.
    public class ItemDictionary : IBook  // the lamest of names
    { }
}
