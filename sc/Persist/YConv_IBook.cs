using System;
using System.Diagnostics;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using CopperBend.Contract;
using CopperBend.Fabric;

namespace CopperBend.Persist
{
    public class YConv_IBook : Persistence_util, IYamlTypeConverter
    {
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

        private void EmitTome(IEmitter emitter, IBook book)
        {
            var tome = (TomeOfChaos)book;

            EmitKVP(emitter, "TopSeed", tome.TopSeed);
            EmitKVP(emitter, "TopGenerator", SerializedRNG(tome.TopGenerator));
            EmitKVP(emitter, "LearnableGenerator", SerializedRNG(tome.LearnableGenerator));
            EmitKVP(emitter, "MapTopGenerator", SerializedRNG(tome.MapTopGenerator));

            //0.1.SAVE:  Write remainder of Tome, these named sets are scaling... iffily.
        }

        private void EmitHerbal(IEmitter emitter, IBook book)
        {
        }

        private void EmitSocialRegister(IEmitter emitter, IBook book)
        {
        }

        private void EmitDramaticon(IEmitter emitter, IBook book)
        {
        }

        public object ReadYaml(IParser parser, Type type)
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            IBook book = null;

            parser.Expect<MappingStart>();
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
                book = ParseRegister(parser);
                break;

            case "Dramaticon":
                book = ParseDramaticon(parser);
                break;

            //0.1.SAVE:  Read remainder of Compendium

            default:
                throw new NotImplementedException($"Not ready to Read book type [{bookType}].");
            }

            parser.Expect<MappingEnd>();
            return book;
        }

        private IBook ParseHerbal(IParser parser)
        {
            Herbal herbal = new Herbal();
            return herbal;
        }

        private IBook ParseRegister(IParser parser)
        {
            SocialRegister socialRegister = new SocialRegister();
            return socialRegister;
        }

        private IBook ParseDramaticon(IParser parser)
        {
            Dramaticon dramaticon = new Dramaticon();
            return dramaticon;
        }

        private IBook ParseCompendium(IParser parser)
        {
            Compendium compendium = new Compendium();
            while (parser.Peek<Scalar>() != null)
            {
                string next = GetScalar(parser);
                switch (next)
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
    }
}
