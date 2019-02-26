using System;
using CopperBend.MapUtil;
using RLNET;

namespace CopperBend.App.Model
{
    public class Fruit : Item
    {
        public PlantType PlantType;

        public Fruit(Point point, int quantity, PlantType plantType)
            : base(point, quantity, false)
        {
            PlantType = plantType;
            Symbol = '%';
            ColorForeground = RLColor.LightRed;
            Name = "fruit";
        }

        public override bool IsConsumable => true;
    }
}
