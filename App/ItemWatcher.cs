using CopperBend.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CopperBend.App
{
    public class ItemWatcher
    {
        public List<string> SeedAdjectives = new List<string>
        {
            "Burred",
            "Rough",
            "Smooth",
        };

        public Dictionary<SeedType, string> DescriptionFrom;
        public Dictionary<string, SeedType> TypeFrom;
        public Dictionary<SeedType, bool> Learned;

        public ItemWatcher()
        {
            //TODO: Take randomizer seed, so that Save/Load
            //  just needs that number to recreate the shuffles.
            var rnd = new Random();
            var shuffled = SeedAdjectives
                .OrderBy(d => rnd.Next()).ToList();

            TypeFrom = new Dictionary<string, SeedType>();
            DescriptionFrom = new Dictionary<SeedType, string>();
            Learned = new Dictionary<SeedType, bool>();

            //  I am broot
            TypeFrom[shuffled[0]] = SeedType.Boomer;
            TypeFrom[shuffled[1]] = SeedType.Healer;
            TypeFrom[shuffled[2]] = SeedType.Thornfriend;
            DescriptionFrom[SeedType.Boomer] = shuffled[0];
            DescriptionFrom[SeedType.Healer] = shuffled[1];
            DescriptionFrom[SeedType.Thornfriend] = shuffled[2];
            Learned[SeedType.Boomer] = false;
            Learned[SeedType.Healer] = false;
            Learned[SeedType.Thornfriend] = false;
        }

        public void Learn(SeedType type)
        {
            Learned[type] = true;
        }

        public string Describe(SeedType type)
        {
            return Learned[type]
                ? type.ToString()
                : DescriptionFrom[type];
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
