using CopperBend.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CopperBend.App
{
    public class PartialDescriber
    {
        public PartialDescriber()
        {
            ScrambleSeeds();
            ScrambleFruit();
        }

        #region Seeds
        public List<string> SeedAdjectives = new List<string>
        {
            "Burred",
            "Rough",
            "Smooth",
        };

        public Dictionary<PlantType, string> SeedDescriptionFrom;
        public Dictionary<string, PlantType> SeedTypeFrom;
        public Dictionary<PlantType, bool> SeedLearned;

        private void ScrambleSeeds()
        {
            //TODO: Take randomizer seed, so that Save/Load
            //  just needs that number to recreate the shuffles.
            var rnd = new Random();
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
            "Knobby",
            "Star-shaped",
            "Smooth",
        };

        public Dictionary<PlantType, string> FruitDescriptionFrom;
        public Dictionary<string, PlantType> FruitTypeFrom;
        public Dictionary<PlantType, bool> FruitLearned;

        private void ScrambleFruit()
        {
            //TODO: Take randomizer seed, so that Save/Load
            //  just needs that number to recreate the shuffles.
            var rnd = new Random();
            var shuffled = FruitAdjectives
                .OrderBy(d => rnd.Next()).ToList();

            SeedTypeFrom = new Dictionary<string, PlantType>();
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
            SeedLearned[seed.SeedType] = true;
        }

        public void Learn(Fruit fruit)
        {
            FruitLearned[fruit.PlantType] = true;
        }

        public string Describe(PlantType type)
        {
            return SeedLearned[type]
                ? type.ToString()
                : SeedDescriptionFrom[type];
        }

        public string AdjectiveFor(IItem item)
        {
            if (item is Seed seed)
            {
                return Describe(seed.SeedType);
            }
            else  //  potions, wands, scrolls are the genre staples...
            {
                return "";
            }
        }

        public string Describe(IItem item)
        {
            var prefix = item.Quantity.ToString();
            var adj = AdjectiveFor(item);
            if (adj.Length > 0) adj = adj + " ";

            var s = "s";
            if (item.Quantity == 1)
            {
                //WAIT: ('til we have exceptions) check list of exceptions
                bool vowelSound = Regex.Match(adj, "^[aeiouy]", RegexOptions.IgnoreCase).Success;
                prefix = vowelSound ? "an" : "a";
                s = "";
            }

            return $"{prefix} {adj}{item.Name}{s}";
        }
    }
}
