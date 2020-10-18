using System;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Logic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace CopperBend.Creation
{
    public class YTC_IBeingStrategy : IYamlTypeConverter
    {
        //public bool Accepts(Type type) => type is IBeingStrategy;
        public bool Accepts(Type type) => typeof(IBeingStrategy).IsAssignableFrom(type);

        public object ReadYaml(IParser parser, Type type)
        {
            IBeingStrategy bs = null;

            parser.Consume<MappingStart>();
            {
                string subtype = parser.GetKVP_string("SubType");
                //bs = subtype switch
                //{
                //    "User Input" => new BeingStrategy_UserInput(null, null, null, null, null, null), //INPROG
                //    "Do Nothing" => new BeingStrategy_DoNothing(null),
                //    _ => throw new Exception($"Not ready to generate '{subtype}' Being Strategy from Yaml."),
                //};

                // more generally, while (..., but right now I only have one optional element
                //if (parser.TryConsume<Scalar>(out Scalar scalar))
                //{
                //    switch (scalar.Value)
                //    {
                //    case "Storage":
                //        parser.Consume<MappingStart>();
                //        while (!parser.TryConsume<MappingEnd>(out _))
                //        {
                //            (string key, string val) = parser.GetKVP();
                //            bs.Storage[key] = val;
                //        }
                //        break;

                //    default:
                //        throw new Exception($"Reading Being Strategy, didn't expect '{scalar.Value}'.");
                //    }
                //}
            }
            parser.Consume<MappingEnd>();

            return bs;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            emitter.Emit(new MappingStart());

            var bs = (IBeingStrategy)value;
            emitter.EmitKVP("SubType", bs.SubType);

            //if (bs.Storage.Any())
            //{
            //    emitter.Emit(new Scalar(null, "Storage"));
            //    emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
            //    {
            //        foreach (var key in bs.Storage.Keys)
            //        {
            //            emitter.EmitKVP(key, bs.Storage[key]);
            //        }
            //    }
            //    emitter.Emit(new MappingEnd());
            //}

            emitter.Emit(new MappingEnd());
        }
    }
}
