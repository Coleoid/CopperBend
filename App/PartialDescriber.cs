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

        public Dictionary<SeedType, string> SeedDescriptionFrom;
        public Dictionary<string, SeedType> SeedTypeFrom;
        public Dictionary<SeedType, bool> SeedLearned;

        private void ScrambleSeeds()
        {
            //TODO: Take randomizer seed, so that Save/Load
            //  just needs that number to recreate the shuffles.
            var rnd = new Random();
            var shuffled = SeedAdjectives
                .OrderBy(d => rnd.Next()).ToList();

            SeedTypeFrom = new Dictionary<string, SeedType>();
            SeedDescriptionFrom = new Dictionary<SeedType, string>();
            SeedLearned = new Dictionary<SeedType, bool>();

            //  I am broot
            SeedTypeFrom[shuffled[0]] = SeedType.Boomer;
            SeedTypeFrom[shuffled[1]] = SeedType.Healer;
            SeedTypeFrom[shuffled[2]] = SeedType.Thornfriend;
            SeedDescriptionFrom[SeedType.Boomer] = shuffled[0];
            SeedDescriptionFrom[SeedType.Healer] = shuffled[1];
            SeedDescriptionFrom[SeedType.Thornfriend] = shuffled[2];
            SeedLearned[SeedType.Boomer] = false;
            SeedLearned[SeedType.Healer] = false;
            SeedLearned[SeedType.Thornfriend] = false;
        }
        #endregion

        #region Fruit
        public List<string> FruitAdjectives = new List<string>
        {
            "Knobby",
            "Star-shaped",
            "Smooth",
        };

        public Dictionary<SeedType, string> FruitDescriptionFrom;
        public Dictionary<string, SeedType> FruitTypeFrom;
        public Dictionary<SeedType, bool> FruitLearned;

        private void ScrambleFruit()
        {
            //TODO: Take randomizer seed, so that Save/Load
            //  just needs that number to recreate the shuffles.
            var rnd = new Random();
            var shuffled = FruitAdjectives
                .OrderBy(d => rnd.Next()).ToList();

            SeedTypeFrom = new Dictionary<string, SeedType>();
            FruitDescriptionFrom = new Dictionary<SeedType, string>();
            FruitLearned = new Dictionary<SeedType, bool>();

            //  I am broot
            FruitTypeFrom[shuffled[0]] = SeedType.Boomer;
            FruitTypeFrom[shuffled[1]] = SeedType.Healer;
            FruitTypeFrom[shuffled[2]] = SeedType.Thornfriend;
            FruitDescriptionFrom[SeedType.Boomer] = shuffled[0];
            FruitDescriptionFrom[SeedType.Healer] = shuffled[1];
            FruitDescriptionFrom[SeedType.Thornfriend] = shuffled[2];
            FruitLearned[SeedType.Boomer] = false;
            FruitLearned[SeedType.Healer] = false;
            FruitLearned[SeedType.Thornfriend] = false;
        }
        #endregion

        public void Learn(SeedType type)
        {
            SeedLearned[type] = true;
        }

        public string Describe(SeedType type)
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
