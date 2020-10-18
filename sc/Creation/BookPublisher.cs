using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using Troschuetz.Random.Generators;
using CopperBend.Contract;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using CopperBend.Model;
using GoRogue;
using SadConsole;
using CopperBend.Fabric;

namespace CopperBend.Creation
{
    public class BookPublisher
    {
        public IBeingCreator BeingCreator { get; }
        public BookPublisher(IBeingCreator creator)
        {
            BeingCreator = creator;
        }

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
            Atlas_ToYaml(emitter, compendium.Atlas);

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
            Atlas atlas = null;

            while (parser.TryConsume<Scalar>(out var next))
            {
                switch (next.Value)
                {
                case "TomeOfChaos": tome = Tome_FromYaml(parser); break;
                case "Herbal": herbal = Herbal_FromYaml(parser); break;
                case "SocialRegister": register = Register_FromYaml(parser, BeingCreator); break;
                case "Dramaticon": drama = Dramaticon_FromYaml(parser); break;
                case "Atlas": atlas = Atlas_FromYaml(parser); break;

                default:
                    throw new NotImplementedException($"Cannot yet parse a [{next.Value}] for Compendium.");
                }
            }

            var compendium = new Compendium(idGen, BeingCreator, tome, herbal, register, drama, atlas);

            parser.Consume<MappingEnd>();
            return compendium;
        }
        #endregion

        #region TomeOfChaos
        public TomeOfChaos Tome_FromNew(string topSeed)
        {
            var tome = new TomeOfChaos(topSeed);
            AbstractGenerator topRNG;
            AbstractGenerator mapRNG;

            // Any time the sequence of .Next() calls to an RNG changes,
            // the same seed will no longer produce the same world.
            tome.Generators["Top"] = topRNG = new NR3Generator(tome.TopSeedInt);
            tome.Generators["MapTop"] = mapRNG = new NR3Generator(topRNG.Next());
            tome.Generators["Learnable"] = new NR3Generator(topRNG.Next());

            tome.MapGenerators[MapEnum.TackerFarm] = new NR3Generator(mapRNG.Next());
            tome.MapGenerators[MapEnum.TownBarricade] = new NR3Generator(mapRNG.Next());

            return tome;
        }

        public TomeOfChaos Tome_FromYaml(IParser parser)
        {
            parser.Consume<MappingStart>();

            (string k, string v) = parser.GetKVP();
            if (k != "TopSeed") throw new Exception($"Expected 'TopSeed', got '{k}'.");

            var topSeed = v;
            var mapGenerators = new Dictionary<MapEnum, AbstractGenerator>();
            var generators = new Dictionary<string, AbstractGenerator>();

            string label = parser.GetScalar();
            if (label != "Generators") throw new Exception($"Expected 'Generators', got '{label}'.");
            parser.Consume<MappingStart>();
            while (!parser.TryConsume<MappingEnd>(out _))
            {
                (k, v) = parser.GetKVP();
                generators[k] = RngFromBase64(v);
            }

            label = parser.GetScalar();
            if (label != "MapGenerators") throw new Exception($"Expected 'MapGenerators', got '{label}'.");
            parser.Consume<MappingStart>();
            while (!parser.TryConsume<MappingEnd>(out _))
            {
                (k, v) = parser.GetKVP();
                MapEnum key = Enum.Parse<MapEnum>(k);
                mapGenerators[key] = RngFromBase64(v);
            }

            parser.Consume<MappingEnd>();

            var tome = new TomeOfChaos(topSeed, generators, mapGenerators);
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
        public SocialRegister Register_FromNew(IBeingCreator creator)
        {
            var register = new SocialRegister(creator);

            return register;
        }

        public void Register_ToYaml(IEmitter emitter, IBook book, IBeingCreator creator)
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

        public SocialRegister Register_FromYaml(IParser parser, IBeingCreator creator)
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

        #region Atlas
        public void Atlas_ToYaml(IEmitter emitter, IBook book)
        {
            if (book == null) return;
            var atlas = (Atlas)book;

            emitter.StartNamedMapping("Atlas");

            foreach (var key in atlas.Legend.Keys)
            {
                Terrain_ToYaml(emitter, atlas.Legend[key]);
            }

            emitter.EndMapping();
        }

        public void Terrain_ToYaml(IEmitter emitter, Terrain terrain)
        {
            if (terrain == null) return;

            emitter.StartNamedMapping("Terrain");
            emitter.EmitKVP("Name", terrain.Name);

            emitter.StartNamedMapping("Cell");
            emitter.EmitKVP("FG", terrain.Cell.Foreground.ToString());
            emitter.EmitKVP("BG", terrain.Cell.Background.ToString());
            emitter.EmitKVP("Glyph", terrain.Cell.Glyph);
            emitter.EndMapping();

            emitter.EmitKVP("Transparent", terrain.CanSeeThrough);
            emitter.EmitKVP("Walkable", terrain.CanWalkThrough);
            emitter.EmitKVP("Plantable", terrain.CanPlant);
            emitter.EndMapping();
        }

        public Atlas Atlas_FromYaml(IParser parser)
        {
            Atlas atlas = new Atlas();

            parser.Consume<MappingStart>();
            while (parser.TryConsume<Scalar>(out var evt) && evt.Value == "Terrain")
            {
                var terrain = Terrain_FromYaml(parser);
                atlas.StoreTerrain(terrain);
            }
            parser.Consume<MappingEnd>();

            return atlas;
        }

        public Terrain Terrain_FromYaml(IParser parser)
        {
            parser.Consume<MappingStart>();
            Terrain terrain = new Terrain
            {
                Name = parser.GetKVP_string("Name"),
                Cell = Cell_FromYaml(parser),
                CanSeeThrough = parser.GetKVP_bool("Transparent"),
                CanWalkThrough = parser.GetKVP_bool("Walkable"),
                CanPlant = parser.GetKVP_bool("Plantable"),
            };
            parser.Consume<MappingEnd>();

            return terrain;
        }

        public Cell Cell_FromYaml(IParser parser)
        {
            if (!parser.TryConsume<Scalar>(out var evt) || evt.Value != "Cell")
                throw new Exception("Not a Cell at this location");

            parser.Consume<MappingStart>();
            var fg = parser.GetKVP_Color("FG");
            var bg = parser.GetKVP_Color("BG");
            var glyph = parser.GetKVP_int("Glyph");
            parser.Consume<MappingEnd>();

            return new Cell(fg, bg, glyph);
        }

        public Atlas Atlas_FromNew()
        {
            Atlas atlas = new Atlas();

            InitLegend(atlas);
            return atlas;
        }


        private void InitLegend(Atlas atlas)
        {
            var dirtBG = new Color(50, 30, 13);
            var growingBG = new Color(28, 54, 22);
            var stoneBG = new Color(28, 30, 22);

            var terrain = new Terrain
            {
                Name = "Unknown",
                CanWalkThrough = true,
                CanSeeThrough = true,
                Cell = new Cell(Color.DarkRed, Color.DarkRed, '?'),
            };
            atlas.StoreTerrain(terrain);

            terrain = new Terrain
            {
                Name = TerrainEnum.Soil,
                CanWalkThrough = true,
                CanSeeThrough = true,
                CanPlant = true,
                Cell = new Cell(Color.DarkGray, dirtBG, '.'),
            };
            atlas.StoreTerrain(terrain);


            terrain = new Terrain
            {
                Name = TerrainEnum.SoilTilled,
                CanWalkThrough = true,
                CanSeeThrough = true,
                CanPlant = true,
                Cell = new Cell(Color.SaddleBrown, dirtBG, '~'),
            };
            atlas.StoreTerrain(terrain);

            terrain = new Terrain
            {
                Name = TerrainEnum.SoilPlanted,
                CanWalkThrough = true,
                CanSeeThrough = true,
                CanPlant = false,
                Cell = new Cell(Color.ForestGreen, dirtBG, '~'),
            };
            atlas.StoreTerrain(terrain);

            terrain = new Terrain
            {
                Name = "stone wall",
                CanWalkThrough = false,
                CanSeeThrough = false,
                Cell = new Cell(Color.DarkGray, stoneBG, '#'),
            };
            atlas.StoreTerrain(terrain);

            terrain = new Terrain
            {
                Name = TerrainEnum.DoorClosed,
                CanWalkThrough = false,
                CanSeeThrough = false,
                Cell = new Cell(Color.DarkGray, stoneBG, '+'),
            };
            atlas.StoreTerrain(terrain);

            terrain = new Terrain
            {
                Name = TerrainEnum.DoorOpen,
                CanWalkThrough = true,
                CanSeeThrough = true,
                Cell = new Cell(Color.DarkGray, stoneBG, '-'),
            };
            atlas.StoreTerrain(terrain);

            terrain = new Terrain
            {
                Name = "wooden fence",
                CanWalkThrough = false,
                CanSeeThrough = false,
                Cell = new Cell(Color.SaddleBrown, dirtBG, 'X'),
            };
            atlas.StoreTerrain(terrain);

            terrain = new Terrain
            {
                Name = "wall",
                CanWalkThrough = false,
                CanSeeThrough = false,
                Cell = new Cell(Color.DarkGray, stoneBG, '='),
            };
            atlas.StoreTerrain(terrain);

            terrain = new Terrain
            {
                Name = TerrainEnum.Floor,
                CanSeeThrough = true,
                CanWalkThrough = true,
                Cell = new SadConsole.Cell(Color.White, stoneBG, '.'),
            };
            atlas.StoreTerrain(terrain);

            terrain = new Terrain
            {
                Name = "gate",
                CanWalkThrough = false,
                CanSeeThrough = true,
                Cell = new Cell(Color.DarkGray, stoneBG, '%'),
            };
            atlas.StoreTerrain(terrain);

            terrain = new Terrain
            {
                Name = "grass",
                CanWalkThrough = true,
                CanSeeThrough = true,
                CanPlant = true,
                Cell = new Cell(Color.ForestGreen, growingBG, ','),
            };
            atlas.StoreTerrain(terrain);

            terrain = new Terrain
            {
                Name = "tall weeds",
                CanWalkThrough = true,
                CanSeeThrough = true,
                Cell = new Cell(Color.ForestGreen, growingBG, 'w'),
            };
            atlas.StoreTerrain(terrain);

            terrain = new Terrain
            {
                Name = "table",
                CanWalkThrough = false,
                CanSeeThrough = true,
                Cell = new Cell(Color.BurlyWood, stoneBG, 'T'),
            };
            atlas.StoreTerrain(terrain);

            terrain = new Terrain()
            {
                Name = "stairs down",
                CanWalkThrough = true,
                CanSeeThrough = true,
                Cell = new Cell(Color.AliceBlue, stoneBG, '>'),
            };
            atlas.StoreTerrain(terrain);
        }

        #endregion
    }
}
