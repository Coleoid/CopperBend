using System;
using RLNET;
using RogueSharp;

namespace CopperBend.App.Model
{
    public class Fruit : Item
    {
        private readonly SeedType SeedType;

        public Fruit(Coord coord, int quantity, SeedType seedType)
            : base(coord, quantity, true)
        {
            SeedType = seedType;
            Symbol = '%';
            ColorForeground = RLColor.LightRed;
        }

        //  This may grow into enough difference to justify subclassing
        public virtual void Consume(IControlPanel controls)
        {
            base.Consumed(controls);

            switch (SeedType)
            {
            case SeedType.Healer:
                controls.HealPlayer(4);
                controls.GiveToPlayer(new Seed(new Coord(0, 0), 2, SeedType.Healer));
                //update knowledge
                //messages
                break;

            default:
                throw new Exception($"Don't have eating written for fruit of {SeedType}.");
            }
        }

        public override bool IsConsumable => true;
    }
}
