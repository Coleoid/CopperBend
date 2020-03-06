using System;
using System.Text.RegularExpressions;
using CopperBend.Contract;
using CopperBend.Model;
using CopperBend.Model.Aspects;

namespace CopperBend.Fabric
{
    public static class Equipper
    {
        public static Herbal Herbal { get; set; }

        public static Item BuildItem(string itemName, int quantity = 1)
        {
            Guard.AgainstNullArgument(itemName);

            var wantedPlant = Regex.Match(itemName, "^(seed|fruit):(.*)");
            if (wantedPlant.Success) return BuildPlant(wantedPlant);

            var item = new Item((0, 0)) { Name = itemName, ItemType = itemName, Quantity = quantity };
            switch (itemName)
            {
            case "hoe":
                item.AddAspect(
                    new Usable("till ground with", UseTargetFlags.Direction)
                        .AddEffect("till", 1)
                        .AddCosts(("time", 24), ("energy", 20))
                //1.+: Selecting between multiple uses in a single tool?
                //.AddAspect(new Usable("remove weeds with", UseTargetFlags.Direction)
                //    .AddEffect("weed", 1)
                //    .AddCosts(("time", 24), ("energy", 5))
                );
                break;

            case "knife":
                item.AddAspect(
                    new Weapon()
                        .AddAttackEffect("physical.impact.edge", "1d4+2")
                        .AddCosts(("time", 8), ("energy", 12))
                );
                break;

            case "gadget":
            case "widget":
                //  These items intentionally left useless
                break;

            case "gloves":
                //  They affect the incident 'My Hands', is that rep'd here?
                break;

            default:
                throw new Exception($"Don't know how to build a [{itemName}].");
            }

            return item;
        }

        private static Item BuildPlant(Match wantedPlant)
        {
            string plantPart = wantedPlant.Groups[1].Value;
            string plantName = wantedPlant.Groups[2].Value;
            return BuildPlant(plantPart, plantName);
        }

        public static Item BuildPlant(string plantPart, string plantName)
        {
            var byName = Herbal.PlantByName;
            if (!byName.ContainsKey(plantName))
                throw new Exception($"Don't know the plant [{plantName}].");

            return BuildPlant(plantPart, byName[plantName]);
        }

        public static Item BuildPlant(string plantPart, PlantDetails details)
        {
            var item = new Item((0, 0)) { Name = plantPart };

            switch (plantPart)
            {
            case "fruit":
                item.AddAspect(
                    new Ingestible(foodValue: 210)  //0.2: draw from data
                    {
                        IsFruit = true,
                        PlantID = details.ID,
                    });
                break;

            case "seed":
                item.AddAspect(
                    new Usable("plant", UseTargetFlags.Direction)
                        .AddEffect("plant", 1)  //0.1: add growth time and plant type
                        .AddCosts(("time", 6), ("energy", 5))
                );
                break;

            default:
                throw new Exception($"Can't handle plant part [{plantPart}].");
            }

            item.AddAspect(new Plant(details));
            return item;
        }

    }
}
