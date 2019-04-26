using CopperBend.Contract;
using Microsoft.Xna.Framework;

namespace CopperBend.Model
{
    public class Fruit : Item
    {
        public PlantType PlantType;

        public Fruit(Point point, int quantity, PlantType plantType)
            : base(point, quantity, false)
        {
            PlantType = plantType;
            Symbol = '%';
            ColorForeground = Color.LightPink;
            Name = "fruit";
        }

        public override bool IsConsumable => true;
    }
}
