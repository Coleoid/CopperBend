using System;
using CopperBend.MapUtil;
using RLNET;

namespace CopperBend.App.Model
{
    public class Fruit : Item
    {
        public PlantType PlantType;

        public Fruit(Point point, int quantity, PlantType olantType)
            : base(point, quantity, true)
        {
            PlantType = olantType;
            Symbol = '%';
            ColorForeground = RLColor.LightRed;
            Name = "fruit";
        }

        //  This may grow into enough difference to justify subclassing
        public override void Consume(IControlPanel controls)
        {
            base.Consume(controls);

            switch (PlantType)
            {
            case PlantType.Healer:
                controls.HealPlayer(4);
                controls.GiveToPlayer(new Seed(new Point(0, 0), 2, PlantType.Healer));
                controls.Learn(this);
                break;

            default:
                throw new Exception($"Don't have eating written for fruit of {PlantType}.");
            }
        }

        public override bool IsConsumable => true;
    }
}
