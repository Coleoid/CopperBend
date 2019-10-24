using System;
using System.Diagnostics;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;

namespace CopperBend.Persist
{
    public class YConv_IBook_two : Persistence_util, IYamlTypeConverter
    {
        #region IYamlTypeConverter
        public bool Accepts(Type type)
        {
            return typeof(IBook).IsAssignableFrom(type);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            IBook book = (IBook)value;

            //emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
            //EmitKVP(emitter, "BookType", book.BookType);

            switch (book.BookType)
            {
            case "Compendium":
                //emitter.Emit(new Scalar(null, "Compendium"));
                EmitCompendium(emitter, book);
                break;

            case "TomeOfChaos":
                EmitTome(emitter, book);
                break;

            case "Herbal":
                EmitHerbal(emitter, book);
                break;

            case "SocialRegister":
                EmitSocialRegister(emitter, book);
                break;

            case "Dramaticon":
                EmitDramaticon(emitter, book);
                break;

            default:
                throw new NotImplementedException($"Not ready to Write book type [{book.BookType}].");
            }

            emitter.Emit(new MappingEnd());
        }

        public object ReadYaml(IParser parser, Type type)
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            IBook book = null;

            _ = parser.Consume<MappingStart>();
            var bookType = GetValueNext(parser, "BookType");

            switch (bookType)
            {
            case "Compendium":
                book = ParseCompendium(parser);
                break;

            case "TomeOfChaos":
                book = ParseTome(parser);
                break;

            case "Herbal":
                book = ParseHerbal(parser);
                break;

            case "SocialRegister":
                book = ParseSocialRegister(parser);
                break;

            case "Dramaticon":
                book = ParseDramaticon(parser);
                break;

            //0.1.SAVE:  Read remainder of Compendium

            default:
                throw new NotImplementedException($"Not ready to Read book type [{bookType}].");
            }

            parser.Consume<MappingEnd>();
            return book;
        }
        #endregion

        #region Compendium
        private void EmitCompendium(IEmitter emitter, IBook book)
        {
            emitter.Emit(new Scalar(null, "Compendium"));
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            var compendium = (Compendium)book;

            EmitTome(emitter, compendium.TomeOfChaos);
            EmitHerbal(emitter, compendium.Herbal);
            EmitSocialRegister(emitter, compendium.SocialRegister);
            EmitDramaticon(emitter, compendium.Dramaticon);

            emitter.Emit(new MappingEnd());
        }

        private IBook ParseCompendium(IParser parser)
        {
            Compendium compendium = new Compendium();
            //while (parser.Peek<Scalar>() != null)
            while (parser.Accept<Scalar>(out var next))
            {
                switch (next.Value)
                {
                case "TomeOfChaos":
                    compendium.TomeOfChaos = (TomeOfChaos)ReadYaml(parser, typeof(TomeOfChaos));
                    break;

                case "Herbal":
                    compendium.Herbal = (Herbal)ReadYaml(parser, typeof(Herbal));
                    break;

                case "SocialRegister":
                    compendium.SocialRegister = (SocialRegister)ReadYaml(parser, typeof(SocialRegister));
                    break;

                case "Dramaticon":
                    compendium.Dramaticon = (Dramaticon)ReadYaml(parser, typeof(Dramaticon));
                    break;

                default:
                    throw new NotImplementedException($"Not ready to parse [{next}] into Compendium.");
                }
            }

            return compendium;
        }
        #endregion

        #region TomeOfChaos
        private void EmitTome(IEmitter emitter, IBook book)
        {
            if (book == null) return;
            var tome = (TomeOfChaos)book;

            emitter.Emit(new Scalar(null, "TomeOfChaos"));
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            EmitKVP(emitter, "TopSeed", tome.TopSeed);
            EmitKVP(emitter, "TopGenerator", SerializedRNG(tome.TopGenerator));
            EmitKVP(emitter, "LearnableGenerator", SerializedRNG(tome.LearnableGenerator));
            EmitKVP(emitter, "MapTopGenerator", SerializedRNG(tome.MapTopGenerator));

            //0.1.SAVE:  Write remainder of Tome, these named sets are scaling... iffily.

            emitter.Emit(new MappingEnd());
        }

        private TomeOfChaos ParseTome(IParser parser)
        {
            TomeOfChaos tome = null;

            var topSeed = GetValueNext(parser, "TopSeed");
            tome = new TomeOfChaos(topSeed);

            var rng_b64 = GetValueNext(parser, "TopGenerator");
            tome.TopGenerator = RngFromBase64(rng_b64);

            rng_b64 = GetValueNext(parser, "LearnableGenerator");
            tome.LearnableGenerator = RngFromBase64(rng_b64);

            rng_b64 = GetValueNext(parser, "MapTopGenerator");
            tome.MapTopGenerator = RngFromBase64(rng_b64);

            //0.1.SAVE: Parse out the remaining RNGs

            return tome;
        }
        #endregion

        #region Herbal
        private void EmitHerbal(IEmitter emitter, IBook book)
        {
            if (book == null) return;
            var herbal = (Herbal)book;

            foreach (var key in herbal.PlantByID.Keys)
            {
                EmitPlantDetails(emitter, herbal.PlantByID[key]);
            }
        }

        private IBook ParseHerbal(IParser parser)
        {
            Herbal herbal = new Herbal();

            while (parser.Accept<Scalar>(out var evt) && evt.Value == "Plant")
            {
                parser.Consume<Scalar>();
                parser.Consume<MappingStart>();

                herbal.AddPlant(ParsePlantDetails(parser));

                parser.Consume<MappingEnd>();
            }

            return herbal;
        }

        private void EmitPlantDetails(IEmitter emitter, PlantDetails plantDetails)
        {
            emitter.Emit(new Scalar("Plant"));
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            EmitKVP(emitter, "ID", plantDetails.ID.ToString());
            EmitKVP(emitter, "MainName", plantDetails.MainName);
            EmitKVP(emitter, "FruitAdjective", plantDetails.FruitAdjective);
            EmitKVP(emitter, "FruitKnown", plantDetails.FruitKnown.ToString());
            EmitKVP(emitter, "SeedAdjective", plantDetails.SeedAdjective);
            EmitKVP(emitter, "SeedKnown", plantDetails.SeedKnown.ToString());
            EmitKVP(emitter, "GrowthTime", plantDetails.GrowthTime.ToString());
            //EmitKVP(emitter, "Uses", plantDetails.Uses);

            emitter.Emit(new MappingEnd());
        }

        private PlantDetails ParsePlantDetails(IParser parser)
        {
            var details = new PlantDetails();

            details.ID = uint.Parse(GetValueNext(parser, "ID"));
            details.MainName = GetValueNext(parser, "MainName");
            details.FruitAdjective = GetValueNext(parser, "FruitAdjective");
            details.FruitKnown = bool.Parse(GetValueNext(parser, "FruitKnown"));
            details.SeedAdjective = GetValueNext(parser, "SeedAdjective");
            details.SeedKnown = bool.Parse(GetValueNext(parser, "SeedKnown"));
            details.GrowthTime = int.Parse(GetValueNext(parser, "GrowthTime"));

            return details;
        }
        #endregion

        #region ...remainder...
        private void EmitSocialRegister(IEmitter emitter, IBook book)
        {
            if (book == null) return;
        }

        private IBook ParseSocialRegister(IParser parser)
        {
            SocialRegister socialRegister = new SocialRegister();
            return socialRegister;
        }

        private void EmitDramaticon(IEmitter emitter, IBook book)
        {
            if (book == null) return;
        }

        private IBook ParseDramaticon(IParser parser)
        {
            Dramaticon dramaticon = new Dramaticon();
            return dramaticon;
        }
        #endregion
    }


    public class YConv_IBook : Persistence_util, IYamlTypeConverter
    {
        #region IYamlTypeConverter
        public bool Accepts(Type type)
        {
            return typeof(IBook).IsAssignableFrom(type);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            IBook book = (IBook)value;

            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            EmitKVP(emitter, "BookType", book.BookType);

            switch (book.BookType)
            {
            case "Compendium":
                //emitter.Emit(new Scalar(null, "Compendium"));
                EmitCompendium(emitter, book);
                break;

            case "TomeOfChaos":
                EmitTome(emitter, book);
                break;

            case "Herbal":
                EmitHerbal(emitter, book);
                break;

            case "SocialRegister":
                EmitSocialRegister(emitter, book);
                break;

            case "Dramaticon":
                EmitDramaticon(emitter, book);
                break;

            default:
                throw new NotImplementedException($"Not ready to Write book type [{book.BookType}].");
            }

            emitter.Emit(new MappingEnd());
        }

        public object ReadYaml(IParser parser, Type type)
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            IBook book = null;

            _ = parser.Consume<MappingStart>();
            var bookType = GetValueNext(parser, "BookType");

            switch (bookType)
            {
            case "Compendium":
                book = ParseCompendium(parser);
                break;

            case "TomeOfChaos":
                book = ParseTome(parser);
                break;

            case "Herbal":
                book = ParseHerbal(parser);
                break;

            case "SocialRegister":
                book = ParseSocialRegister(parser);
                break;

            case "Dramaticon":
                book = ParseDramaticon(parser);
                break;

            //0.1.SAVE:  Read remainder of Compendium

            default:
                throw new NotImplementedException($"Not ready to Read book type [{bookType}].");
            }

            parser.Consume<MappingEnd>();
            return book;
        }
        #endregion

        #region Compendium
        private void EmitCompendium(IEmitter emitter, IBook book)
        {
            var compendium = (Compendium)book;

            if (compendium.TomeOfChaos != null)
            {
                emitter.Emit(new Scalar(null, "TomeOfChaos"));
                WriteYaml(emitter, compendium.TomeOfChaos, typeof(TomeOfChaos));
            }

            if (compendium.Herbal != null)
            {
                emitter.Emit(new Scalar(null, "Herbal"));
                WriteYaml(emitter, compendium.Herbal, typeof(Herbal));
            }

            if (compendium.SocialRegister != null)
            {
                emitter.Emit(new Scalar(null, "SocialRegister"));
                WriteYaml(emitter, compendium.SocialRegister, typeof(SocialRegister));
            }

            if (compendium.Dramaticon != null)
            {
                emitter.Emit(new Scalar(null, "Dramaticon"));
                WriteYaml(emitter, compendium.Dramaticon, typeof(Dramaticon));
            }
        }

        private IBook ParseCompendium(IParser parser)
        {
            Compendium compendium = new Compendium();
            //while (parser.Peek<Scalar>() != null)
            while (parser.Accept<Scalar>(out var next))
            {
                switch (next.Value)
                {
                case "TomeOfChaos":
                    compendium.TomeOfChaos = (TomeOfChaos)ReadYaml(parser, typeof(TomeOfChaos));
                    break;

                case "Herbal":
                    compendium.Herbal = (Herbal)ReadYaml(parser, typeof(Herbal));
                    break;

                case "SocialRegister":
                    compendium.SocialRegister = (SocialRegister)ReadYaml(parser, typeof(SocialRegister));
                    break;

                case "Dramaticon":
                    compendium.Dramaticon = (Dramaticon)ReadYaml(parser, typeof(Dramaticon));
                    break;

                default:
                    throw new NotImplementedException($"Not ready to parse [{next}] into Compendium.");
                }
            }

            return compendium;
        }
        #endregion

        #region TomeOfChaos
        private void EmitTome(IEmitter emitter, IBook book)
        {
            var tome = (TomeOfChaos)book;

            EmitKVP(emitter, "TopSeed", tome.TopSeed);
            EmitKVP(emitter, "TopGenerator", SerializedRNG(tome.TopGenerator));
            EmitKVP(emitter, "LearnableGenerator", SerializedRNG(tome.LearnableGenerator));
            EmitKVP(emitter, "MapTopGenerator", SerializedRNG(tome.MapTopGenerator));

            //0.1.SAVE:  Write remainder of Tome, these named sets are scaling... iffily.
        }

        private TomeOfChaos ParseTome(IParser parser)
        {
            TomeOfChaos tome = null;

            var topSeed = GetValueNext(parser, "TopSeed");
            tome = new TomeOfChaos(topSeed);

            var rng_b64 = GetValueNext(parser, "TopGenerator");
            tome.TopGenerator = RngFromBase64(rng_b64);

            rng_b64 = GetValueNext(parser, "LearnableGenerator");
            tome.LearnableGenerator = RngFromBase64(rng_b64);

            rng_b64 = GetValueNext(parser, "MapTopGenerator");
            tome.MapTopGenerator = RngFromBase64(rng_b64);

            //0.1.SAVE: Parse out the remaining RNGs

            return tome;
        }
        #endregion

        #region Herbal
        private void EmitHerbal(IEmitter emitter, IBook book)
        {
            var herbal = (Herbal)book;

            foreach (var key in herbal.PlantByID.Keys)
            {
                emitter.Emit(new Scalar("Plant"));
                emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

                EmitPlantDetails(emitter, herbal.PlantByID[key]);

                emitter.Emit(new MappingEnd());
            }
        }

        private IBook ParseHerbal(IParser parser)
        {
            Herbal herbal = new Herbal();

            while (parser.Accept<Scalar>(out var evt) && evt.Value == "Plant")
            {
                parser.Consume<Scalar>();
                parser.Consume<MappingStart>();

                herbal.AddPlant(ParsePlantDetails(parser));

                parser.Consume<MappingEnd>();
            }

            return herbal;
        }

        private void EmitPlantDetails(IEmitter emitter, PlantDetails plantDetails)
        {
            EmitKVP(emitter, "ID", plantDetails.ID.ToString());
            EmitKVP(emitter, "MainName", plantDetails.MainName);
            EmitKVP(emitter, "FruitAdjective", plantDetails.FruitAdjective);
            EmitKVP(emitter, "FruitKnown", plantDetails.FruitKnown.ToString());
            EmitKVP(emitter, "SeedAdjective", plantDetails.SeedAdjective);
            EmitKVP(emitter, "SeedKnown", plantDetails.SeedKnown.ToString());
            EmitKVP(emitter, "GrowthTime", plantDetails.GrowthTime.ToString());
            //EmitKVP(emitter, "Uses", plantDetails.Uses);
        }

        private PlantDetails ParsePlantDetails(IParser parser)
        {
            var details = new PlantDetails();

            details.ID = uint.Parse(GetValueNext(parser, "ID"));
            details.MainName = GetValueNext(parser, "MainName");
            details.FruitAdjective = GetValueNext(parser, "FruitAdjective");
            details.FruitKnown = bool.Parse(GetValueNext(parser, "FruitKnown"));
            details.SeedAdjective = GetValueNext(parser, "SeedAdjective");
            details.SeedKnown = bool.Parse(GetValueNext(parser, "SeedKnown"));
            details.GrowthTime = int.Parse(GetValueNext(parser, "GrowthTime"));

            return details;
        }
        #endregion

        #region ...remainder...
        private void EmitSocialRegister(IEmitter emitter, IBook book)
        {
        }

        private IBook ParseSocialRegister(IParser parser)
        {
            SocialRegister socialRegister = new SocialRegister();
            return socialRegister;
        }

        private void EmitDramaticon(IEmitter emitter, IBook book)
        {
        }

        private IBook ParseDramaticon(IParser parser)
        {
            Dramaticon dramaticon = new Dramaticon();
            return dramaticon;
        }
        #endregion
    }
}
