﻿using System;
using System.Globalization;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using CopperBend.Logic;

namespace CopperBend.Persist
{
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable CA1801 // Remove unused parameter
    public class YConv_IBook : Persistence_util, IYamlTypeConverter
    {
        public Compendium Compendium { get; set; }

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
            IBook book = type switch
            {
                "Compendium" => ParseCompendium(parser),
                "TomeOfChaos" => ParseTome(parser),
                "Herbal" => ParseHerbal(parser),
                "SocialRegister" => ParseSocialRegister(parser),
                "Dramaticon" => ParseDramaticon(parser),
                //0.1.SAVE:  Read remainder of Compendium
                _ => throw new NotImplementedException($"NI: Dispatch parse of book type [{type}]."),
            };
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
            //var Compendium = new Compendium();
            while (parser.TryConsume<Scalar>(out var next))
            {
                var book = DispatchParse(parser, next.Value);
                switch (next.Value)
                {
                case "TomeOfChaos": Compendium.TomeOfChaos = (TomeOfChaos)book; break;
                case "Herbal": Compendium.Herbal = (Herbal)book; break;
                case "SocialRegister": Compendium.SocialRegister = (SocialRegister)book; break;
                case "Dramaticon": Compendium.Dramaticon = (Dramaticon)book; break;

                default:
                    throw new NotImplementedException($"NI: attach [{next.Value}] to Compendium.");
                }
            }

            parser.Consume<MappingEnd>();
            return Compendium;
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

            var topSeed = GetValueNext(parser, "TopSeed");
            var tome = new TomeOfChaos(topSeed);

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

            EmitKVP(emitter, "ID", plantDetails.ID.ToString(CultureInfo.InvariantCulture));
            EmitKVP(emitter, "MainName", plantDetails.MainName);
            EmitKVP(emitter, "FruitAdjective", plantDetails.FruitAdjective);
            EmitKVP(emitter, "FruitKnown", plantDetails.FruitKnown.ToString(CultureInfo.InvariantCulture));
            EmitKVP(emitter, "SeedAdjective", plantDetails.SeedAdjective);
            EmitKVP(emitter, "SeedKnown", plantDetails.SeedKnown.ToString(CultureInfo.InvariantCulture));
            EmitKVP(emitter, "GrowthTime", plantDetails.GrowthTime.ToString(CultureInfo.InvariantCulture));

            emitter.Emit(new MappingEnd());
        }

        private PlantDetails ParsePlantDetails(IParser parser)
        {
            parser.Consume<MappingStart>();
            var details = new PlantDetails();

            details.ID = uint.Parse(GetValueNext(parser, "ID"), CultureInfo.InvariantCulture);
            details.MainName = GetValueNext(parser, "MainName");
            details.FruitAdjective = GetValueNext(parser, "FruitAdjective");
            details.FruitKnown = bool.Parse(GetValueNext(parser, "FruitKnown"));
            details.SeedAdjective = GetValueNext(parser, "SeedAdjective");
            details.SeedKnown = bool.Parse(GetValueNext(parser, "SeedKnown"));
            details.GrowthTime = int.Parse(GetValueNext(parser, "GrowthTime"), CultureInfo.InvariantCulture);

            parser.Consume<MappingEnd>();
            return details;
        }
        #endregion

        #region ...remainder...
        private void EmitSocialRegister(IEmitter emitter, IBook book)
        {
            if (book == null) return;
            var reg = (SocialRegister)book;

            var yBeing = new YConv_IBeing();
            yBeing.BeingCreator = Engine.BeingCreator;

            emitter.Emit(new Scalar(null, "SocialRegister"));
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            foreach (var key in reg.WellKnownBeings.Keys)
            {
                emitter.Emit(new Scalar("Being"));
                emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

                yBeing.EmitBeing(emitter, reg.WellKnownBeings[key]);

                emitter.Emit(new MappingEnd());
            }

            emitter.Emit(new MappingEnd());
        }

        private IBook ParseSocialRegister(IParser parser)
        {
            parser.Consume<MappingStart>();
            SocialRegister socialRegister = new SocialRegister();

            var yBeing = new YConv_IBeing();
            yBeing.BeingCreator = Engine.BeingCreator;

            while (parser.TryConsume<Scalar>(out var evt) && evt.Value == "Being")
            {
                parser.Consume<MappingStart>();
                IBeing being = yBeing.ParseBeing(parser);
                parser.Consume<MappingEnd>();

                socialRegister.WellKnownBeings[being.Name] = being;
            }

            parser.Consume<MappingEnd>();
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
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CA1801 // Remove unused parameter
}
