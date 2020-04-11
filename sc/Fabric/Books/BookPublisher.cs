using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Troschuetz.Random.Generators;
using CopperBend.Contract;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using CopperBend.Model;
using GoRogue;

namespace CopperBend.Fabric
{
    public class BookPublisher
    {
        public BookPublisher(BeingCreator creator)
        {
            BeingCreator = creator;
        }
        public BeingCreator BeingCreator { get; }

        #region Compendium
        public void Compendium_ToYaml(IEmitter emitter, IBook book)
        {
            emitter.Emit(new Scalar(null, "Compendium"));
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            var compendium = (Compendium)book;

            emitter.EmitKVP("NextID", compendium.IDGenerator.UseID().ToString());

            Tome_ToYaml(emitter, compendium.TomeOfChaos);
            Herbal_ToYaml(emitter, compendium.Herbal);
            Register_ToYaml(emitter, compendium.SocialRegister, BeingCreator);
            Dramaticon_ToYaml(emitter, compendium.Dramaticon);

            emitter.Emit(new MappingEnd());
        }

        public IBook Compendium_FromYaml(IParser parser)
        {
            parser.Consume<MappingStart>();

            uint nextID = uint.Parse(parser.GetKVP_string("NextID"));
            IDGenerator idGen = new IDGenerator(nextID);

            TomeOfChaos tome = null;
            Herbal herbal = null;
            SocialRegister register = null;
            Dramaticon drama = null;

            while (parser.TryConsume<Scalar>(out var next))
            {
                switch (next.Value)
                {
                case "TomeOfChaos": tome = Tome_FromYaml(parser); break;
                case "Herbal": herbal = Herbal_FromYaml(parser); break;
                case "SocialRegister": register = Register_FromYaml(parser, BeingCreator); break;
                case "Dramaticon": drama = Dramaticon_FromYaml(parser); break;

                default:
                    throw new NotImplementedException($"Cannot yet parse a [{next.Value}] for Compendium.");
                }
            }

            var compendium = new Compendium(idGen, BeingCreator, tome, herbal, register, drama);

            parser.Consume<MappingEnd>();
            return compendium;
        }
        #endregion

        #region TomeOfChaos
        public TomeOfChaos Tome_FromNew(string topSeed)
        {
            var tome = new TomeOfChaos();
            tome.SetTopSeed(topSeed);
            AbstractGenerator topRNG;
            AbstractGenerator mapRNG;

            // Any time the sequence of .Next() calls to an RNG changes,
            // the same seed will no longer produce the same world.
            tome.Generators["Top"] = topRNG = new NR3Generator(tome.TopSeedInt);
            tome.Generators["MapTop"] = mapRNG = new NR3Generator(topRNG.Next());
            tome.Generators["Learnable"] = new NR3Generator(topRNG.Next());

            tome.MapGenerators[Maps.TackerFarm] = new NR3Generator(mapRNG.Next());
            tome.MapGenerators[Maps.TownBarricade] = new NR3Generator(mapRNG.Next());

            return tome;
        }

        public TomeOfChaos Tome_FromYaml(IParser parser)
        {
            parser.Consume<MappingStart>();

            (string k, string v) = parser.GetKVP();
            if (k != "TopSeed") throw new Exception($"Expected 'TopSeed', got '{k}'.");
            TomeOfChaos tome = new TomeOfChaos();
            tome.SetTopSeed(v);

            string label = parser.GetScalar();
            if (label != "Generators") throw new Exception($"Expected 'Generators', got '{label}'.");
            parser.Consume<MappingStart>();
            while (!parser.TryConsume<MappingEnd>(out _))
            {
                (k, v) = parser.GetKVP();
                tome.Generators[k] = RngFromBase64(v);
            }

            label = parser.GetScalar();
            if (label != "MapGenerators") throw new Exception($"Expected 'MapGenerators', got '{label}'.");
            parser.Consume<MappingStart>();
            while (!parser.TryConsume<MappingEnd>(out _))
            {
                (k, v) = parser.GetKVP();
                Maps key = Enum.Parse<Maps>(k);
                tome.MapGenerators[key] = RngFromBase64(v);
            }

            parser.Consume<MappingEnd>();
            return tome;
        }

        public void Tome_ToYaml(IEmitter emitter, IBook book)
        {
            if (book == null) return;
            var tome = (TomeOfChaos)book;

            emitter.Emit(new Scalar(null, "TomeOfChaos"));
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
            {
                emitter.EmitKVP("TopSeed", tome.TopSeed);

                emitter.Emit(new Scalar(null, "Generators"));
                emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
                {
                    foreach (var key in tome.Generators.Keys)
                    {
                        emitter.EmitKVP(key, SerializedRNG(tome.Generators[key]));
                    }
                }
                emitter.Emit(new MappingEnd());

                emitter.Emit(new Scalar(null, "MapGenerators"));
                emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
                {
                    foreach (var key in tome.MapGenerators.Keys)
                    {
                        emitter.EmitKVP(key.ToString(), SerializedRNG(tome.MapGenerators[key]));
                    }
                }
                emitter.Emit(new MappingEnd());
            }
            emitter.Emit(new MappingEnd());
        }

        public static string SerializedRNG(AbstractGenerator generator)
        {
            byte[] gen_bytes = ObjectToByteArray(generator);
            string gen_b64 = Convert.ToBase64String(gen_bytes);
            return gen_b64;
        }

        private static byte[] ObjectToByteArray(object target)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using var ms = new MemoryStream();
            bf.Serialize(ms, target);
            return ms.ToArray();
        }

        private static AbstractGenerator RngFromBase64(string rng_b64)
        {
            byte[] rng_bytes = Convert.FromBase64String(rng_b64);
            var rng = (NR3Generator)ByteArrayToObject(rng_bytes);
            return rng;
        }

        private static object ByteArrayToObject(byte[] arrBytes)
        {
            using var memStream = new MemoryStream(arrBytes);
            var binForm = new BinaryFormatter();
            var obj = binForm.Deserialize(memStream);
            return obj;
        }
        #endregion

        #region Herbal
        public Herbal Herbal_FromYaml(IParser parser)
        {
            parser.Consume<MappingStart>();
            Herbal herbal = new Herbal();

            while (parser.TryConsume<Scalar>(out var evt) && evt.Value == "Plant")
            {
                herbal.AddPlant(ParsePlantDetails(parser));
            }

            parser.Consume<MappingEnd>();
            return herbal;
        }

        private PlantDetails ParsePlantDetails(IParser parser)
        {
            parser.Consume<MappingStart>();
            var details = new PlantDetails();

            details.ID = parser.GetKVP_uint("ID");
            details.MainName = parser.GetKVP_string("MainName");
            details.FruitAdjective = parser.GetKVP_string("FruitAdjective");
            details.FruitKnown = parser.GetKVP_bool("FruitKnown");
            details.SeedAdjective = parser.GetKVP_string("SeedAdjective");
            details.SeedKnown = parser.GetKVP_bool("SeedKnown");
            details.GrowthTime = parser.GetKVP_int("GrowthTime");

            parser.Consume<MappingEnd>();
            return details;
        }

        public void Herbal_ToYaml(IEmitter emitter, IBook book)
        {
            if (book == null) return;
            var herbal = (Herbal)book;

            emitter.StartNamedMapping("Herbal");
            foreach (var key in herbal.PlantByID.Keys)
            {
                EmitPlantDetails(emitter, herbal.PlantByID[key]);
            }
            emitter.EndMapping();
        }

        private void EmitPlantDetails(IEmitter emitter, PlantDetails plantDetails)
        {
            emitter.StartNamedMapping("Plant");

            emitter.EmitKVP("ID", plantDetails.ID.ToString(CultureInfo.InvariantCulture));
            emitter.EmitKVP("MainName", plantDetails.MainName);
            emitter.EmitKVP("FruitAdjective", plantDetails.FruitAdjective);
            emitter.EmitKVP("FruitKnown", plantDetails.FruitKnown.ToString(CultureInfo.InvariantCulture));
            emitter.EmitKVP("SeedAdjective", plantDetails.SeedAdjective);
            emitter.EmitKVP("SeedKnown", plantDetails.SeedKnown.ToString(CultureInfo.InvariantCulture));
            emitter.EmitKVP("GrowthTime", plantDetails.GrowthTime.ToString(CultureInfo.InvariantCulture));

            emitter.EndMapping();
        }

        public Herbal Herbal_FromNew()
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
        #endregion

        #region SocialRegister
        public SocialRegister Register_FromNew(BeingCreator creator)
        {
            var register = new SocialRegister(creator);

            return register;
        }

        public void Register_ToYaml(IEmitter emitter, IBook book, BeingCreator creator)
        {
            if (book == null) return;
            var reg = (SocialRegister)book;

            emitter.StartNamedMapping("SocialRegister");

            foreach (var key in reg.WellKnownBeings.Keys)
            {
                emitter.StartNamedMapping("Being");

                creator.Being_ToYaml(emitter, reg.WellKnownBeings[key]);

                emitter.EndMapping();
            }

            emitter.EndMapping();
        }

        public SocialRegister Register_FromYaml(IParser parser, BeingCreator creator)
        {
            parser.Consume<MappingStart>();
            var register = new SocialRegister(creator);

            while (parser.TryConsume<Scalar>(out var evt) && evt.Value == "Being")
            {
                parser.Consume<MappingStart>();
                IBeing being = creator.Being_FromYaml(parser);
                parser.Consume<MappingEnd>();

                register.WellKnownBeings[being.Name] = being;
            }

            parser.Consume<MappingEnd>();
            return register;
        }
        #endregion

        #region Dramaticon
        public void Dramaticon_ToYaml(IEmitter emitter, IBook book)
        {
            if (book == null) return;
            var drama = (Dramaticon)book;

            emitter.StartNamedMapping("Dramaticon");
            emitter.EmitKVP("HasClearedRot", drama.HasClearedRot.ToString());
            emitter.EndMapping();
        }

        public Dramaticon Dramaticon_FromYaml(IParser parser)
        {
            parser.Consume<MappingStart>();

            Dramaticon dramaticon = new Dramaticon();
            dramaticon.HasClearedRot = parser.GetKVP_bool("HasClearedRot");

            parser.Consume<MappingEnd>();
            return dramaticon;
        }

        public Dramaticon Dramaticon_FromNew()
        {
            Dramaticon dramaticon = new Dramaticon();
            dramaticon.HasClearedRot = false;
            return dramaticon;
        }
        #endregion
    }
}
