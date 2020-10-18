using System;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using CopperBend.Contract;
using CopperBend.Fabric;

namespace CopperBend.Creation
{
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable CA1801 // Remove unused parameter
    public class YConv_IBook : IYamlTypeConverter
    {
        public BookPublisher Publisher { get; set; }
        public IBeingCreator Creator { get; set; }

        public YConv_IBook(IBeingCreator creator)
        {
            Creator = creator;
            Publisher = new BookPublisher(creator);
        }

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
                Publisher.Compendium_ToYaml(emitter, compendium);
                break;

            case TomeOfChaos tome:
                Publisher.Tome_ToYaml(emitter, tome);
                break;

            case Herbal herbal:
                Publisher.Herbal_ToYaml(emitter, herbal);
                break;

            case SocialRegister socialRegister:
                Publisher.Register_ToYaml(emitter, socialRegister, Creator);
                break;

            case Dramaticon dramaticon:
                Publisher.Dramaticon_ToYaml(emitter, dramaticon);
                break;

            case Atlas atlas:
                Publisher.Atlas_ToYaml(emitter, atlas);
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

        private IBook DispatchParse(IParser parser, string type)
        {
            IBook book = type switch
            {
                "Compendium" => Publisher.Compendium_FromYaml(parser),
                "TomeOfChaos" => Publisher.Tome_FromYaml(parser),
                "Herbal" => Publisher.Herbal_FromYaml(parser),
                "SocialRegister" => Publisher.Register_FromYaml(parser, Creator),
                "Dramaticon" => Publisher.Dramaticon_FromYaml(parser),
                "Atlas" => Publisher.Atlas_FromYaml(parser),
                //0.1.SAVE:  Read remainder of Compendium
                _ => throw new NotImplementedException($"NI: Dispatch parse of book type [{type}]."),
            };
            return book;
        }

    }
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CA1801 // Remove unused parameter
}
