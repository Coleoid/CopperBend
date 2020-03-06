using System;
using System.Collections.Generic;
using System.Text;
using GoRogue;
using Troschuetz.Random.Generators;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;

namespace CopperBend.Logic
{
    public partial class Engine
    {
        public static Compendium Compendium { get; set; }
        public static BeingCreator BeingCreator { get => Compendium.BeingCreator; }

        public static void Cosmogenesis(string topSeed, ISadConEntityFactory factory)
        {
            Compendium = new Compendium();

            var generator = InitIDGenerator();
            ConnectIDGenerator(generator);

            var tome = InitTome(topSeed);
            ConnectTome(tome);

            var herbal = InitHerbal();
            ConnectHerbal(herbal);

            var story = InitStory();
            ConnectStory(story);

            var creator = InitBeingCreator(factory);
            ConnectCreator(creator);
        }

        public static TomeOfChaos InitTome(string topSeed)
        {
            var tome = new TomeOfChaos(topSeed);

            //0.2:  Move from indexing on the Maps enum to loading from YAML
            tome.MapGenerators[Maps.TackerFarm] = new XorShift128Generator(tome.MapTopGenerator.Next());
            tome.MapGenerators[Maps.TownBarricade] = new XorShift128Generator(tome.MapTopGenerator.Next());

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
            CbEntity.SetIDGenerator(gen);
            Item.SetIDGenerator(gen);
            Space.SetIDGenerator(gen);
            AreaRot.SetIDGenerator(gen);
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
                GrowthTime = 400,
            };
            herbal.PlantByID[plant.ID] = plant;
            herbal.PlantByName[plant.MainName] = plant;

            plant = new PlantDetails
            {
                ID = 2,
                MainName = "Healer",
                GrowthTime = 400,
            };
            herbal.PlantByID[plant.ID] = plant;
            herbal.PlantByName[plant.MainName] = plant;

            plant = new PlantDetails
            {
                ID = 3,
                MainName = "Thornfriend",
                GrowthTime = 400,
            };
            herbal.PlantByID[plant.ID] = plant;
            herbal.PlantByName[plant.MainName] = plant;

            return herbal;
        }

        public static void ConnectHerbal(Herbal herbal)
        {
            Compendium.Herbal = herbal;
            Describer.Herbal = herbal;
        }

        public static Dramaticon InitStory()
        {
            return new Dramaticon();
        }

        public static void ConnectStory(Dramaticon story)
        {
            Compendium.Dramaticon = story;
        }

        public static BeingCreator InitBeingCreator(ISadConEntityFactory factory)
        {
            return new BeingCreator(factory);
        }

        public static void ConnectCreator(BeingCreator creator)
        {
            Compendium.BeingCreator = creator;
        }

        private static string GenerateSimpleTopSeed()
        {
            string clearLetters = "bcdefghjkmnpqrstvwxyz";
            var r = new Random();
            var b = new StringBuilder();
            b.Append(clearLetters[r.Next(0, 20)]);
            b.Append(clearLetters[r.Next(0, 20)]);
            b.Append('-');
            b.Append(clearLetters[r.Next(0, 20)]);
            b.Append(clearLetters[r.Next(0, 20)]);
            b.Append('-');
            b.Append(clearLetters[r.Next(0, 20)]);
            b.Append(clearLetters[r.Next(0, 20)]);

            return b.ToString();
        }
    }
}
