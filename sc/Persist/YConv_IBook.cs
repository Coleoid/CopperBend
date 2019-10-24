using System;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;

namespace CopperBend.Persist
{
    public class YConv_IBook : Persistence_util, IYamlTypeConverter
    {
        #region IYamlTypeConverter
        public bool Accepts(Type type)
        {
            return typeof(IBook).IsAssignableFrom(type);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            emitter.Emit(new MappingStart());

            switch ((IBook)value)
            {
            case Compendium compendium:
                EmitCompendium(emitter, compendium);
                break;

            case TomeOfChaos tome:
                EmitTome(emitter, tome);
                break;

            case Herbal herbal:
                EmitHerbal(emitter, herbal);
                break;

            case SocialRegister socialRegister:
                EmitSocialRegister(emitter, socialRegister);
                break;

            case Dramaticon dramaticon:
                EmitDramaticon(emitter, dramaticon);
                break;

            default:
                throw new NotImplementedException($"Not ready to Write book type [{value.GetType().Name}].");
            }

            emitter.Emit(new MappingEnd());
        }

        public object ReadYaml(IParser parser, Type type)
        {
            parser.Consume<MappingStart>();
            var bookType = parser.Consume<Scalar>();
            var parsed = DispatchParse(parser, bookType.Value);
            parser.Consume<MappingEnd>();

            return parsed;
        }

        public IBook DispatchParse(IParser parser, string type)
        {
            IBook book = null;
            switch (type)
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
                throw new NotImplementedException($"NI: Dispatch parse of book type [{type}].");
            }
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
            parser.Consume<MappingStart>();
            var compendium = new Compendium();
            while (parser.TryConsume<Scalar>(out var next))
            {
                var book = DispatchParse(parser, next.Value);
                switch (next.Value)
                {
                case "TomeOfChaos": compendium.TomeOfChaos = (TomeOfChaos)book; break;
                case "Herbal": compendium.Herbal = (Herbal)book; break;
                case "SocialRegister": compendium.SocialRegister = (SocialRegister)book; break;
                case "Dramaticon": compendium.Dramaticon = (Dramaticon)book; break;

                default:
                    throw new NotImplementedException($"NI: attach [{next.Value}] to Compendium.");
                }
            }

            parser.Consume<MappingEnd>();
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
            parser.Consume<MappingStart>();
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

            parser.Consume<MappingEnd>();
            return tome;
        }
        #endregion

        #region Herbal
        private void EmitHerbal(IEmitter emitter, IBook book)
        {
            if (book == null) return;
            var herbal = (Herbal)book;

            emitter.Emit(new Scalar(null, "Herbal"));
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            foreach (var key in herbal.PlantByID.Keys)
            {
                EmitPlantDetails(emitter, herbal.PlantByID[key]);
            }

            emitter.Emit(new MappingEnd());
        }

        private IBook ParseHerbal(IParser parser)
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

            emitter.Emit(new MappingEnd());
        }

        private PlantDetails ParsePlantDetails(IParser parser)
        {
            parser.Consume<MappingStart>();
            var details = new PlantDetails();

            details.ID = uint.Parse(GetValueNext(parser, "ID"));
            details.MainName = GetValueNext(parser, "MainName");
            details.FruitAdjective = GetValueNext(parser, "FruitAdjective");
            details.FruitKnown = bool.Parse(GetValueNext(parser, "FruitKnown"));
            details.SeedAdjective = GetValueNext(parser, "SeedAdjective");
            details.SeedKnown = bool.Parse(GetValueNext(parser, "SeedKnown"));
            details.GrowthTime = int.Parse(GetValueNext(parser, "GrowthTime"));

            parser.Consume<MappingEnd>();
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
}
