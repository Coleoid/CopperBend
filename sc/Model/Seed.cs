﻿using System;
using System.Collections.Generic;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Fabric;

namespace CopperBend.Model
{
    public class Seed : Item, ISeed, IScheduleAgent
    {
        public static Dictionary<uint, PlantDetails> PlantByID { get; set; }
        public static Dictionary<string, PlantDetails> PlantByName { get; set; }

        public PlantDetails PlantDetails;

        public override string Name
        {
            get => $"{PlantDetails.MainName} seed";
        }

        public override bool StacksWith(IItem item)
        {
            if (item is Seed seed)
            {
                return PlantDetails.ID == seed.PlantDetails.ID;
            }

            return false;
        }

        public Seed()
            : base((0,0), 1, true)
        {
        }

        public Seed(Coord position, int quantity, uint typeID)
            : base(position, quantity, true)
        {
            PlantDetails = PlantByID[typeID];
        }

        internal Seed GetSeedFromStack()
        {
            Guard.Against(Quantity < 1, "Somehow there's no seed here");
            return new Seed(this.Location, 1, this.PlantDetails.ID);
        }

        public override string Adjective
        {
            get => PlantDetails.SeedKnown ? PlantDetails.MainName : PlantDetails.SeedAdjective;
        }

        private int growthRound = 0;

        public void SeedGrows(IControlPanel controls)
        {
            controls.AddMessage($"The seed is growing... Round {growthRound++}");
            if (growthRound > 3)
                SeedMatures(controls);
            else
                controls.ScheduleAgent(this, growthRound > 2 ? 10 : 40);
        }

        protected virtual void SeedMatures(IControlPanel controls)
        {
            //for now, insta-auto-harvest.  Two fruit drop to the ground, plant disappears.
            IItem fruit = new Fruit(this.Location, 2, this.PlantDetails.ID);
            controls.PutItemOnMap(fruit);
            controls.RemovePlantAt(this.Location);
        }

        public ScheduleEntry GetNextEntry()
        {
            return GetNextEntry(100);
        }

        public ScheduleEntry GetNextEntry(int offset)
        {
            return new ScheduleEntry
            {
                Action = (cp) => SeedGrows(cp),
                Agent = this,
                Offset = offset
            };
        }
    }
}
