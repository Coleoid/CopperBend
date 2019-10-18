using System;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using CopperBend.Model.Aspects;

namespace CopperBend.Fabric
{
    public class Equipper
    {
        public static Item BuildItem(string itemName)
        {
            Guard.AgainstNullArgument(itemName);
            var item = new Item((0, 0)) { Name = itemName };

            switch (itemName)
            {
            case "fruit":
                //  Amorphous plant edible for testing purposes
                item.AddAspect(
                    new Ingestible
                    {
                        IsFruit = true,
                        FoodValue = 210,
                        PlantID = Engine.Engine.Compendium.Herbal.PlantByName["Healer"].ID,
                    });
                break;

            case "hoe":
                item.AddAspect(
                    new Usable("till ground with", UseTargetFlags.Direction)
                        .AddEffect("till", 1)
                        .AddCosts(("time", 24), ("energy", 20))
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

            default:
                throw new Exception($"Don't know how to build a [{itemName}].");
            }

            return item;
        }

    }
}
