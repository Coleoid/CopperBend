using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using CopperBend.Contract;
using CopperBend.Fabric;

namespace CopperBend.Persist
{
    public class YConv_IBeing : IYamlTypeConverter
    {
        public BeingCreator BeingCreator { get; set; }

        public bool Accepts(Type type)
        {
            return typeof(IBeing).IsAssignableFrom(type);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            IBeing being = (IBeing)value;

            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            BeingCreator.Being_ToYaml(emitter, being);

            emitter.Emit(new MappingEnd());
        }

        public object ReadYaml(IParser parser, Type type)
        {
            //if (!Debugger.IsAttached) Debugger.Launch();

            parser.Consume<MappingStart>();
            IBeing being = BeingCreator.Being_FromYaml(parser);
            parser.Consume<MappingEnd>();

            return being;
        }
    }
}
