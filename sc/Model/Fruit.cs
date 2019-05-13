using Color = Microsoft.Xna.Framework.Color;
using GoRogue;
using CopperBend.Contract;

namespace CopperBend.Model
{
    public class Fruit : Item
    {
        public PlantType PlantType;

        public Fruit(Coord location, int quantity, PlantType plantType)
            : base(location, quantity, false)
        {
            PlantType = plantType;
            Glyph = '%';
            Foreground = Color.LightPink;
            Name = "fruit";
        }

        public override bool IsConsumable => true;
    }
}
