using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;

namespace CopperBend.Model
{
    public class Fruit : Item
    {
        public override string ItemType { get => "Fruit"; }
        public static Herbal Herbal { get; set; }

        public PlantDetails PlantDetails;

        public Fruit(Coord location, int quantity, PlantDetails details, uint id = uint.MaxValue)
            : base(location, quantity, false, id)
        {
            PlantDetails = details;
            Glyph = '%';
            Foreground = Color.LightPink;
            Name = "fruit";

            //0.1: Vary by plant, from data
            Components.AddComponent(new Consumable {
                IsFruit = true,
                FoodValue = 400,
                TicksToEat = 10,
                Effect = ("Heal", 4)
            });
        }

        public override string Name
        {
            get => PlantDetails.FruitDescriptionAsKnown;
        }
        public override bool IsConsumable => true;
    }
}
