using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CopperBend.Contract;
using CopperBend.Model;

namespace CopperBend.Engine
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
        public static Dictionary<uint, PlantDetails> PlantByID { get; set; }
        public static Dictionary<string, PlantDetails> PlantByName { get; set; }


        private Random rnd;

        public Describer(int randomSeed = 88) //0.1
        {
            //TODO: Game-wide randomizer seed management
            rnd = new Random(randomSeed);

            ScrambleSeeds();
            ScrambleFruit();
        }

        public List<string> SeedAdjectives = new List<string>
        {
            "burred",
            "rough",
            "smooth",
        };

        private void ScrambleSeeds()
        {
            var shuffled = SeedAdjectives
                .OrderBy(d => rnd.Next()).ToList();

            foreach (var key in PlantByID.Keys)
            {
                PlantByID[key].SeedAdjective = shuffled[0];
                shuffled.RemoveAt(0);
            }
        }

        public List<string> FruitAdjectives = new List<string>
        {
            "knobby",
            "star-shaped",
            "smooth",
        };

        private void ScrambleFruit()
        {
            var shuffled = FruitAdjectives
                .OrderBy(d => rnd.Next()).ToList();

            foreach (var key in PlantByID.Keys)
            {
                PlantByID[key].FruitAdjective = shuffled[0];
                shuffled.RemoveAt(0);
            }
        }

        public void Learn(Seed seed)
        {
            PlantByID[seed.PlantDetails.ID].SeedKnown = true;
        }

        public void Learn(Fruit fruit)
        {
            PlantByID[fruit.PlantDetails.ID].FruitKnown = true;
            PlantByID[fruit.PlantDetails.ID].SeedKnown = true;
        }

        public string Describe(string name, DescMods mods = DescMods.None, int quantity = 1, string adj = "")
        {
            string art = string.Empty;

            if (mods.HasFlag(DescMods.Quantity))
                art = quantity.ToString();

            adj = mods.HasFlag(DescMods.NoAdjective) ? "" : adj;
            if (adj.Length > 0) adj = adj + " ";

            var s = (quantity == 1) ? "" : "s";


            if (mods.HasFlag(DescMods.DefiniteArticle))
            {
                art = "the";
            }
            else if (mods.HasFlag(DescMods.IndefiniteArticle))
            {
                if (quantity == 1)
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

            var description = $"{art}{adj}{name}{s}";
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

        public string Describe(IItem item, DescMods mods = DescMods.None)
        {
            return Describe(item.Name, mods, item.Quantity, AdjectiveFor(item));
        }

        //TODO:  these belong in class Seed and class Fruit, right?
        public string AdjectiveFor(Seed seed)
        {
            return seed.PlantDetails.SeedKnown? seed.PlantDetails.MainName : seed.PlantDetails.SeedAdjective;
        }

        public string AdjectiveFor(Fruit fruit)
        {
            return fruit.PlantDetails.FruitKnown ? fruit.PlantDetails.MainName : fruit.PlantDetails.FruitAdjective;
        }

    }
}
