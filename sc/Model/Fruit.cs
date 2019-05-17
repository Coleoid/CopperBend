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

        public Fruit(Coord location, int quantity, uint typeID)
            : base(location, quantity, false)
        {
            PlantDetails = PlantByID[typeID];
            Glyph = '%';
            Foreground = Color.LightPink;
            Name = "fruit";
        }

        public override bool IsConsumable => true;
    }
}
