﻿using System.Collections.Generic;
using GoRogue;
using Troschuetz.Random.Generators;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;

namespace CopperBend.Engine
{
    public partial class Engine
    {
        public static Compendium Compendium { get; set; }

        public static void Cosmogenesis(string topSeed)
        {
            Compendium = new Compendium();

            var generator = InitIDGenerator();
            ConnectIDGenerator(generator);

            var tome = InitTome(topSeed);
            ConnectTome(tome);

            var herbal = InitHerbal();
            ConnectHerbal(herbal);
        }

        public static TomeOfChaos InitTome(string topSeed)
        {
            var tome = new TomeOfChaos(topSeed);

            //0.2:  Move from indexing on the Maps enum to loading from YAML
            tome.MapGenerators = new Dictionary<Maps, AbstractGenerator>
            {
                [Maps.TackerFarm] = new XorShift128Generator(tome.MapTopGenerator.Next()),
                [Maps.TownBastion] = new XorShift128Generator(tome.MapTopGenerator.Next())
            };

            return tome;
        }

        public static void ConnectTome(TomeOfChaos tome)
        {
            Compendium.TomeOfChaos = tome;
            Describer.TomeOfChaos = Compendium.TomeOfChaos;
        }

        public static IDGenerator InitIDGenerator()
        {
            // The IDGenerator is below the Model
            return new IDGenerator();
        }

        public static void ConnectIDGenerator(IDGenerator gen)
        {
            Compendium.IDGenerator = gen;
            CbEntity.IDGenerator = gen;
            Item.IDGenerator = gen;
            Space.IDGenerator = gen;
            AreaBlight.IDGenerator = gen;
        }

        public static Herbal InitHerbal()
        {
            Herbal herbal = new Herbal();

            herbal.PlantByID = new Dictionary<uint, PlantDetails>();
            herbal.PlantByName = new Dictionary<string, PlantDetails>();

            PlantDetails plant;

            //0.1.WORLD  Flesh out the plant list, and tuck it into YAML config.
            plant = new PlantDetails
            {
                ID = 1,
                MainName = "Boomer",
                GrowthTime = 400
            };
            herbal.PlantByID[plant.ID] = plant;
            herbal.PlantByName[plant.MainName] = plant;

            plant = new PlantDetails
            {
                ID = 2,
                MainName = "Healer",
                GrowthTime = 400
            };
            herbal.PlantByID[plant.ID] = plant;
            herbal.PlantByName[plant.MainName] = plant;

            plant = new PlantDetails
            {
                ID = 3,
                MainName = "Thornfriend",
                GrowthTime = 400
            };
            herbal.PlantByID[plant.ID] = plant;
            herbal.PlantByName[plant.MainName] = plant;
            
            return herbal;
        }

        public static void ConnectHerbal(Herbal herbal)
        {
            Compendium.Herbal = herbal;
            Seed.Herbal = herbal;
            Describer.Herbal = herbal;
        }
    }
}