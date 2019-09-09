using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CopperBend.Contract;
using CopperBend.Fabric;
using Troschuetz.Random.Generators;

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

    public class Describer : IDescriber
    {
        public static Herbal Herbal { get; set; }
        public static TomeOfChaos TomeOfChaos { get; set; }

        private AbstractGenerator rnd;

        public Describer()
        {
            rnd = TomeOfChaos.LearnableGenerator;

            ScrambleSeeds();
            ScrambleFruit();
        }

        public List<string> SeedAdjectives = new List<string>
        {
            "abrasive",
            "blistered",
            "bumpy",
            "burred",
            "barbed",
            "bulging",
            "blemished",
            "chunky",
            "cratered",
            "dense",
            "dented",
            "dusty",
            "elastic",
            "flat",
            "fuzzy",
            "gritty",
            "glossy",
            "hard",
            "irregular",
            "jagged",
            "knobbed",
            "lustrous",
            "metallic",
            "matte",
            "pitted",
            "pointy",
            "pockmarked",
            "prickly",
            "ragged",
            "ridged",
            "rough",
            "sharp-edged",
            "scaly",
            "smooth",
            "shiny",
            "slippery",
            "spiny",
            "thorny",
        };

        private void ScrambleSeeds()
        {
            var shuffled = SeedAdjectives
                .OrderBy(d => rnd.Next()).ToList();

            foreach (var key in Herbal.PlantByID.Keys)
            {
                Herbal.PlantByID[key].SeedAdjective = shuffled[0];
                shuffled.RemoveAt(0);
            }
        }

        public List<string> FruitAdjectives = new List<string>
        {
            "abrasive",
            "aromatic",
            "barbed",
            "bloated",
            "blunt",
            "bulging",
            "cushioned",
            "elastic",
            "firm",
            "fuzzy",
            "flat",
            "gelatinous",
            "hard",
            "knobby",
            "mushy",
            "prickly",
            "pulpy",
            "rough",
            "sharp-smelling",
            "silky",
            "smooth",
            "spotted",
            "springy",
            "star-shaped",
            "streaky",
            "striped",
            "tough",
            "velvety",
        };

        private void ScrambleFruit()
        {
            var shuffled = FruitAdjectives
                .OrderBy(d => rnd.Next()).ToList();

            foreach (var key in Herbal.PlantByID.Keys)
            {
                Herbal.PlantByID[key].FruitAdjective = shuffled[0];
                shuffled.RemoveAt(0);
            }
        }

        public string Describe(IItem item, DescMods mods = DescMods.None)
        {
            return Describe(item.Name, mods, item.Quantity, item.Adjective);
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
    }
}
