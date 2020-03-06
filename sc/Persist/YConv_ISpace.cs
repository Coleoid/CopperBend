using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using CopperBend.Contract;
using YamlDotNet.Core.Events;
using CopperBend.Fabric;

namespace CopperBend.Persist
{
    public class YConv_ISpace : Persistence_util, IYamlTypeConverter
    {
        //0.1: pragma will come off after code fills out.
#pragma warning disable CA1801 // Remove unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        #region IYamlTypeConverter
        public bool Accepts(Type type)
        {
            return typeof(ISpace).IsAssignableFrom(type);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            ISpace space = (ISpace)value;

            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
            EmitSpace(emitter, space);
            emitter.Emit(new MappingEnd());
        }

        public object ReadYaml(IParser parser, Type type)
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            ISpace space;

            parser.Consume<MappingStart>();
            space = ParseSpace(parser);
            parser.Consume<MappingEnd>();

            return space;
        }
        #endregion

        private void EmitSpace(IEmitter emitter, ISpace ispace)
        {
            var space = (Space)ispace;
            EmitKVP(emitter, "ID", space.ID.ToString(CultureInfo.InvariantCulture));
            EmitKVP(emitter, "Terrain", space.Terrain.Name);

            var kst = FlagString(space.IsKnown, space.IsSown, space.IsTilled);
            EmitKVP(emitter, "Flags", kst);
        }

        private string FlagString(params bool[] flags)
        {
            var flagBuilder = new StringBuilder();
            foreach (var flag in flags)
            {
                flagBuilder.Append(flag ? "T" : "F");
            }

            return flagBuilder.ToString();
        }

        private List<bool> FlagsFromString(string flagString)
        {
            var flags = new List<bool>();
            foreach (var flag in flagString.ToCharArray())
            {
                flags.Add(flag == 'T');
            }

            return flags;
        }

        private ISpace ParseSpace(IParser parser)
        {
            uint id = uint.Parse(GetValueNext(parser, "ID"), CultureInfo.InvariantCulture);

            var space = new Space(id)
            {
                Terrain = GetTerrainType(GetValueNext(parser, "Terrain")),
            };

            var flagText = GetValueNext(parser, "Flags");
            var flags = FlagsFromString(flagText);
            space.IsKnown = flags[0];
            space.IsSown = flags[1];
            space.IsTilled = flags[2];

            return space;
        }

        private TerrainType GetTerrainType(string name)
        {
            return null;
        }
    }
#pragma warning restore CA1801 // Remove unused parameter
#pragma warning restore IDE0060 // Remove unused parameter
}
