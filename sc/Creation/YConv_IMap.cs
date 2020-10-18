//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Text;
//using YamlDotNet.Serialization;
//using YamlDotNet.Core;
//using CopperBend.Contract;
//using YamlDotNet.Core.Events;
//using CopperBend.Fabric;
//using GoRogue;

//namespace CopperBend.Persist
//{
//    public class YConv_IMap : IYamlTypeConverter
//    {
//        //0.1: pragma will come off after code fills out.
//#pragma warning disable CA1801 // Remove unused parameter
//#pragma warning disable IDE0060 // Remove unused parameter

//        private Cartographer Cartographer { get; set; }

//        #region IYamlTypeConverter
//        public bool Accepts(Type type)
//        {
//            return typeof(ICompoundMap).IsAssignableFrom(type)
//                || typeof(IRotMap).IsAssignableFrom(type)
//                || typeof(ISpaceMap).IsAssignableFrom(type)
//                || typeof(MultiSpatialMap<IBeing>).IsAssignableFrom(type)
//                || typeof(IItemMap).IsAssignableFrom(type)
//                ;
//        }


//        public void WriteYaml(IEmitter emitter, object value, Type type)
//        {
//            emitter.Emit(new MappingStart());

//            switch (value)
//            {
//            case CompoundMap compoundMap:
//                Cartographer.CompoundMap_ToYaml(emitter, compoundMap);
//                break;

//            case SpaceMap spaceMap:
//                Cartographer.SpaceMap_ToYaml(emitter, spaceMap);
//                break;

//            case RotMap rotMap:
//                Cartographer.RotMap_ToYaml(emitter, rotMap);
//                break;

//            case MultiSpatialMap<IBeing> beingMap:
//                Cartographer.BeingMap_ToYaml(emitter, beingMap);
//                break;

//            case ItemMap itemMap:
//                Cartographer.ItemMap_ToYaml(emitter, itemMap);
//                break;

//            default:
//                throw new NotImplementedException($"Not ready to Write map type [{value.GetType().Name}].");
//            }

//            emitter.Emit(new MappingEnd());
//        }


//        public object ReadYaml(IParser parser, Type type)
//        {
//            parser.Consume<MappingStart>();
//            var mapType = parser.Consume<Scalar>();
//            var parsed = DispatchParse(parser, mapType.Value);
//            parser.Consume<MappingEnd>();

//            return parsed;
//        }

//        private object DispatchParse(IParser parser, string type)
//        {
//            var map = type switch
//            {
//                "CompoundMap" => Cartographer.CompoundMap_FromYaml(parser),
//                "SpaceMap" => Cartographer.SpaceMap_FromYaml(parser),
//                "RotMap" => Cartographer.RotMap_FromYaml(parser),
//                "BeingMap" => Cartographer.BeingMap_FromYaml(parser),
//                "ItemMap" => Cartographer.ItemMap_FromYaml(parser),
//                _ => throw new NotImplementedException($"NI: Dispatch parse of map type [{type}]."),
//            };
//            return map;
//        }


//        public object ReadYaml_x(IParser parser, Type type)
//        {
//            //if (!Debugger.IsAttached) Debugger.Launch();
//            ISpace space;

//            parser.Consume<MappingStart>();
//            space = ParseSpace(parser);
//            parser.Consume<MappingEnd>();

//            return space;
//        }
//        #endregion

//        private void EmitSpace(IEmitter emitter, ISpace ispace)
//        {
//            var space = (Space)ispace;
//            emitter.EmitKVP("ID", space.ID.ToString(CultureInfo.InvariantCulture));
//            emitter.EmitKVP("Terrain", space.Terrain.Name);

//            var kst = FlagString(space.IsKnown, space.IsSown, space.IsTilled);
//            emitter.EmitKVP("Flags", kst);
//        }

//        private string FlagString(params bool[] flags)
//        {
//            var flagBuilder = new StringBuilder();
//            foreach (var flag in flags)
//            {
//                flagBuilder.Append(flag ? "T" : "F");
//            }

//            return flagBuilder.ToString();
//        }

//        private List<bool> FlagsFromString(string flagString)
//        {
//            var flags = new List<bool>();
//            foreach (var flag in flagString.ToCharArray())
//            {
//                flags.Add(flag == 'T');
//            }

//            return flags;
//        }

//        private ISpace ParseSpace(IParser parser)
//        {
//            uint id = parser.GetKVP_uint("ID");

//            var space = new Space(id)
//            {
//                Terrain = GetTerrain(parser.GetKVP_string("Terrain")),
//            };

//            var flagText = parser.GetKVP_string("Flags");
//            var flags = FlagsFromString(flagText);
//            space.IsKnown = flags[0];
//            space.IsSown = flags[1];
//            space.IsTilled = flags[2];

//            return space;
//        }

//        private Terrain GetTerrain(string name)
//        {
//            return null;
//        }
//    }
//#pragma warning restore CA1801 // Remove unused parameter
//#pragma warning restore IDE0060 // Remove unused parameter
//}
