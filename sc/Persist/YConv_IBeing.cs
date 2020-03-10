using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using CopperBend.Contract;
using CopperBend.Model;
using CopperBend.Fabric;

namespace CopperBend.Persist
{
    public class YConv_IBeing : Persistence_util, IYamlTypeConverter
    {
        public BeingCreator BeingCreator { get; set; }

        #region IYamlTypeConverter
        public bool Accepts(Type type)
        {
            return typeof(IBeing).IsAssignableFrom(type);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            IBeing being = (IBeing)value;

            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            EmitBeing(emitter, being);

            emitter.Emit(new MappingEnd());
        }

        public object ReadYaml(IParser parser, Type type)
        {
            //if (!Debugger.IsAttached) Debugger.Launch();

            parser.Consume<MappingStart>();
            IBeing being = ParseBeing(parser);
            parser.Consume<MappingEnd>();

            return being;
        }
        #endregion

        public void EmitBeing(IEmitter emitter, IBeing iBeing)
        {
            var being = (Being)iBeing;
            EmitKVP(emitter, "BeingType", being.BeingType);
            EmitKVP(emitter, "ID", being.ID.ToString(CultureInfo.InvariantCulture));

            EmitKVP(emitter, "Foreground", being.Foreground.ToString());
            EmitKVP(emitter, "Background", being.Background.ToString());
            EmitKVP(emitter, "Glyph", ((char)being.Glyph).ToString(CultureInfo.InvariantCulture));

            EmitKVP(emitter, "Name", being.Name ?? string.Empty);
            EmitKVP(emitter, "Awareness", being.Awareness.ToString(CultureInfo.InvariantCulture));
            EmitKVP(emitter, "Health", being.Health.ToString(CultureInfo.InvariantCulture));
            EmitKVP(emitter, "Position", being.Position.ToString());
        }

        public IBeing ParseBeing(IParser parser)
        {
            var type = GetValueNext(parser, "BeingType");
            uint id = uint.Parse(GetValueNext(parser, "ID"), CultureInfo.InvariantCulture);

            string fgText = GetValueNext(parser, "Foreground");
            string bgText = GetValueNext(parser, "Background");
            int glyph = GetValueNext(parser, "Glyph")[0];

            Color fg = Color_FromString(fgText);
            Color bg = Color_FromString(bgText);
            var being = BeingCreator.CreateBeing(fg, bg, glyph, id);

            being.Name = GetValueNext(parser, "Name");
            being.Awareness = int.Parse(GetValueNext(parser, "Awareness"), CultureInfo.InvariantCulture);
            being.Health = int.Parse(GetValueNext(parser, "Health"), CultureInfo.InvariantCulture);
            being.Position = Point_FromString(GetValueNext(parser, "Position"));
            being.BeingType = type;

            return being;
        }
    }
}
