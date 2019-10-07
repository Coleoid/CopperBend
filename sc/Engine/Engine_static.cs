using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using GoRogue;
using System.Collections.Generic;

namespace CopperBend.Engine
{
    public partial class Engine
    {
        public static Compendium Compendium { get; set; }

        public static void Cosmogenesis(string topSeed)
        {
            Compendium = new Compendium();
            Compendium.IDGenerator = ConnectIDGenerator();
            Compendium.TomeOfChaos = new TomeOfChaos(topSeed);
            Compendium.Herbal = ConnectHerbal();

            //0.1: Think:  Give clients just the books they care about?
            Describer.TomeOfChaos = Compendium.TomeOfChaos;
        }

        public static IDGenerator ConnectIDGenerator()
        {
            // The IDGenerator is below the Model
            var gen = new IDGenerator();

            CbEntity.IDGenerator = gen;
            Item.IDGenerator = gen;
            Space.IDGenerator = gen;
            AreaBlight.IDGenerator = gen;

            return gen;
        }

        public static Herbal ConnectHerbal()
        {
            Herbal herbal = new Herbal();

            herbal.PlantByID = new Dictionary<uint, PlantDetails>();
            herbal.PlantByName = new Dictionary<string, PlantDetails>();

            PlantDetails plant = null;

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

            Seed.Herbal = herbal;
            //Fruit.Herbal = herbal;
            Describer.Herbal = herbal;

            return herbal;
        }
    }
}
