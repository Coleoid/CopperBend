using Color = Microsoft.Xna.Framework.Color;
using GoRogue;
using CopperBend.Contract;
using System.Collections.Generic;

namespace CopperBend.Model
{
    public class Fruit : Item
    {
        public static Dictionary<uint, PlantDetails> PlantByID { get; set; }
        public static Dictionary<string, PlantDetails> PlantByName { get; set; }

        public PlantDetails PlantDetails;

        public Fruit(Coord location, int quantity, PlantDetails details)
            : base(location, quantity, false)
        {
            PlantDetails = details;
            Glyph = '%';
            Foreground = Color.LightPink;
            Name = "fruit";
        }

        public override string Name
        {
            get => PlantDetails.FruitDescriptionAsKnown;
        }
        public override bool IsConsumable => true;
    }
}
