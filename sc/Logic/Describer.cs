using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CopperBend.Contract;
using CopperBend.Fabric;
using Troschuetz.Random.Generators;

namespace CopperBend.Logic
{
    public class Describer : IDescriber
    {
        public Herbal Herbal { get; set; }
        public TomeOfChaos TomeOfChaos { get; set; }

        public void Scramble()
        {
            ScrambleSeeds();
            ScrambleFruit();
        }

        public List<string> SeedAdjectives { get; } = new List<string>
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
                .OrderBy(d => TomeOfChaos.LearnableRndNext()).ToList();

            foreach (var key in Herbal.PlantByID.Keys)
            {
                Herbal.PlantByID[key].SeedAdjective = shuffled[0];
                shuffled.RemoveAt(0);
            }
        }

        public List<string> FruitAdjectives { get; } = new List<string>
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
                .OrderBy(d => TomeOfChaos.LearnableRndNext()).ToList();

            foreach (var key in Herbal.PlantByID.Keys)
            {
                Herbal.PlantByID[key].FruitAdjective = shuffled[0];
                shuffled.RemoveAt(0);
            }
        }

        public virtual string Describe(IItem item, DescMods mods = DescMods.None)
        {
            return Describe(item.Name, mods, item.Quantity, item.Adjective);
        }

        public string Describe(string name, DescMods mods = DescMods.None, int quantity = 1, string adj = "")
        {
            string art;

            adj = mods.HasFlag(DescMods.NoAdjective) ? string.Empty : adj;
            if (adj.Length > 0) adj += " ";

            if (mods.HasFlag(DescMods.Article))
            {
                if (mods.HasFlag(DescMods.Definite))
                {
                    art = "the ";
                }
                else
                {
                    if (quantity == 1)
                    {
                        var leadingWord = adj.Length > 0 ? adj : name;
                        bool leadingVowel = HasLeadingVowelSound(leadingWord);
                        art = leadingVowel ? "an " : "a ";
                    }
                    else
                    {
                        art = mods.HasFlag(DescMods.Quantity) ? $"{quantity} " : "some ";
                    }
                }
            }
            else
            {
                art = mods.HasFlag(DescMods.Quantity) ? $"{quantity} " : string.Empty;
            }

            var s = (quantity == 1) ? string.Empty : "s";
            var description = $"{art}{adj}{name}{s}";

            if (mods.HasFlag(DescMods.LeadingCapital) && !string.IsNullOrEmpty(description))
            {
                description = char.ToUpper(description[0], CultureInfo.InvariantCulture) + description.Substring(1);
            }

            return description;
        }

        public static bool HasLeadingVowelSound(string meetingWord)
        {
            //0.K:  Incomplete by design, trivial to extend as needed.
            var exceptions = new List<(string, bool)>
            {
                ("^h(erb|onor|our|onest|eir)", true),
                ("^uni(que|t|vers|form)", false),
                ("^u(tens|se|vers|form)", false),
                ("^e(we|uph)", false),
                ("^one", false),
                ("^y(tt|gg|mir)", true), // because norse elementalists
            };

            foreach (var (pattern, hasLVS) in exceptions)
                if (Regex.Match(meetingWord, pattern, RegexOptions.IgnoreCase).Success) return hasLVS;

            return Regex.Match(meetingWord, "^[aeiou]", RegexOptions.IgnoreCase).Success;
        }
    }
}
