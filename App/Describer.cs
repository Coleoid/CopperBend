using CopperBend.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CopperBend.App
{
    [Flags]
    public enum DescMods
    {
        None = 0,
        DefiniteArticle = 1,
        IndefiniteArticle = 2,  // definite and indefinite don't fit Flags model perfectly.  Le oh well.
        Quantity = 4,
        LeadingCapital = 8,
        NoAdjective = 16,
    }

    public class Describer
    {
        private Random rnd;
        public Describer(int randomSeed = 88)  //  larva
        {
            //TODO: Game-wide randomizer seed management
            rnd = new Random(randomSeed);

            ScrambleSeeds();
            ScrambleFruit();
        }

        #region Seeds
        public List<string> SeedAdjectives = new List<string>
        {
            "burred",
            "rough",
            "smooth",
        };

        public Dictionary<PlantType, string> SeedDescriptionFrom;
        public Dictionary<string, PlantType> SeedTypeFrom;
        public Dictionary<PlantType, bool> SeedLearned;

        private void ScrambleSeeds()
        {
            var shuffled = SeedAdjectives
                .OrderBy(d => rnd.Next()).ToList();

            SeedTypeFrom = new Dictionary<string, PlantType>();
            SeedDescriptionFrom = new Dictionary<PlantType, string>();
            SeedLearned = new Dictionary<PlantType, bool>();

            //  I am broot
            SeedTypeFrom[shuffled[0]] = PlantType.Boomer;
            SeedTypeFrom[shuffled[1]] = PlantType.Healer;
            SeedTypeFrom[shuffled[2]] = PlantType.Thornfriend;
            SeedDescriptionFrom[PlantType.Boomer] = shuffled[0];
            SeedDescriptionFrom[PlantType.Healer] = shuffled[1];
            SeedDescriptionFrom[PlantType.Thornfriend] = shuffled[2];
            SeedLearned[PlantType.Boomer] = false;
            SeedLearned[PlantType.Healer] = false;
            SeedLearned[PlantType.Thornfriend] = false;
        }
        #endregion

        #region Fruit
        public List<string> FruitAdjectives = new List<string>
        {
            "knobby",
            "star-shaped",
            "smooth",
        };

        public Dictionary<PlantType, string> FruitDescriptionFrom;
        public Dictionary<string, PlantType> FruitTypeFrom;
        public Dictionary<PlantType, bool> FruitLearned;

        private void ScrambleFruit()
        {
            //TODO: Take randomizer seed, so that Save/Load
            //  just needs that number to recreate the shuffles.
            var shuffled = FruitAdjectives
                .OrderBy(d => rnd.Next()).ToList();

            FruitTypeFrom = new Dictionary<string, PlantType>();
            FruitDescriptionFrom = new Dictionary<PlantType, string>();
            FruitLearned = new Dictionary<PlantType, bool>();

            //  I am broot
            FruitTypeFrom[shuffled[0]] = PlantType.Boomer;
            FruitTypeFrom[shuffled[1]] = PlantType.Healer;
            FruitTypeFrom[shuffled[2]] = PlantType.Thornfriend;
            FruitDescriptionFrom[PlantType.Boomer] = shuffled[0];
            FruitDescriptionFrom[PlantType.Healer] = shuffled[1];
            FruitDescriptionFrom[PlantType.Thornfriend] = shuffled[2];
            FruitLearned[PlantType.Boomer] = false;
            FruitLearned[PlantType.Healer] = false;
            FruitLearned[PlantType.Thornfriend] = false;
        }
        #endregion

        public void Learn(Seed seed)
        {
            SeedLearned[seed.PlantType] = true;
        }

        public void Learn(Fruit fruit)
        {
            FruitLearned[fruit.PlantType] = true;
            SeedLearned[fruit.PlantType] = true;
        }

        public string Describe(IItem item, DescMods mods = DescMods.None)
        {
            string art = string.Empty;

            if (mods.HasFlag(DescMods.Quantity))
                art = item.Quantity.ToString();

            var adj = mods.HasFlag(DescMods.NoAdjective)? "" : AdjectiveFor(item);
            if (adj.Length > 0) adj = adj + " ";

            var s = (item.Quantity == 1) ? "" : "s";


            if (mods.HasFlag(DescMods.DefiniteArticle))
            {
                art = "the";
            }
            else if (mods.HasFlag(DescMods.IndefiniteArticle))
            {
                if (item.Quantity == 1)
                {
                    bool vowelSound = Regex.Match(adj, "^[aeiouy]", RegexOptions.IgnoreCase).Success;
                    art = vowelSound ? "an" : "a";
                }
                else
                {
                    if (!mods.HasFlag(DescMods.Quantity))
                        art = "some";
                }
            }

            if (art.Length > 0) art = art + " ";

            var description = $"{art}{adj}{item.Name}{s}";
            if (mods.HasFlag(DescMods.LeadingCapital))
            {
                description = description.Substring(0, 1).ToUpper()
                    + description.Substring(1);
            }

            return description;
        }

        public string AdjectiveFor(IItem item)
        {
            if (item is Seed seed)
            {
                return AdjectiveFor(seed);
            }
            if (item is Fruit fruit)
            {
                return AdjectiveFor(fruit);
            }
            else  //  potions, wands, scrolls are the genre staples...
            {
                return item.Adjective;
            }
        }

        //TODO:  Fix the smell of these two methods.
        public string AdjectiveFor(Seed seed)
        {
            var type = seed.PlantType;
            string adj = SeedLearned[type]
                ? type.ToString().ToLower()
                : SeedDescriptionFrom[type];

            return adj;
        }

        public string AdjectiveFor(Fruit fruit)
        {
            var type = fruit.PlantType;
            string adj = FruitLearned[type]
                ? type.ToString().ToLower()
                : FruitDescriptionFrom[type];

            return adj;
        }

    }
}
