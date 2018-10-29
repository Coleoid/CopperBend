﻿using System;
using RLNET;
using RogueSharp;

namespace CopperBend.App.Model
{
    public class Fruit : Item
    {
        public PlantType PlantType;

        public Fruit(Coord coord, int quantity, PlantType olantType)
            : base(coord, quantity, true)
        {
            PlantType = olantType;
            Symbol = '%';
            ColorForeground = RLColor.LightRed;
        }

        //  This may grow into enough difference to justify subclassing
        public virtual void Consume(IControlPanel controls)
        {
            base.Consumed(controls);

            switch (PlantType)
            {
            case PlantType.Healer:
                controls.HealPlayer(4);
                controls.GiveToPlayer(new Seed(new Coord(0, 0), 2, PlantType.Healer));
                //update knowledge
                //messages
                break;

            default:
                throw new Exception($"Don't have eating written for fruit of {PlantType}.");
            }
        }

        public override bool IsConsumable => true;
    }
}
