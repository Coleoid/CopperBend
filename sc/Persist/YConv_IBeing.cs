using System;
using Microsoft.Xna.Framework;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using CopperBend.Contract;
using YamlDotNet.Core.Events;
using CopperBend.Model;

namespace CopperBend.Persist
{
    public class YConv_IBeing : Persistence_util, IYamlTypeConverter
    {
        #region IYamlTypeConverter
        public bool Accepts(Type type)
        {
            return typeof(IBeing).IsAssignableFrom(type);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            IBeing being = (IBeing)value;

            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            //EmitKVP(emitter, "BeingType", being.BeingType);

            switch (being.BeingType)
            {
            case "Being":
                EmitBeing(emitter, being);
                break;

            case "Player":
                EmitPlayer(emitter, being);
                //emitter.Emit(new Scalar(null, "Compendium"));
                //EmitCompendium(emitter, book);
                break;

            case "Monster":
                //EmitTome(emitter, book);
                break;

            default:
                throw new NotImplementedException($"Not ready to Write being type [{being.BeingType}].");
            }

            emitter.Emit(new MappingEnd());
        }

        public object ReadYaml(IParser parser, Type type)
        {
            //if (!Debugger.IsAttached) Debugger.Launch();
            IBeing being = null;

            parser.Consume<MappingStart>();
            var beingType = GetValueNext(parser, "BeingType");

            switch (beingType)
            {
            case "Being":
                being = ParseBeing(parser);
                break;

            default:
                throw new NotImplementedException($"Not ready to Read being type [{beingType}].");
            }

            parser.Consume<MappingEnd>();
            return being;
        }
        #endregion

        private void EmitBeing(IEmitter emitter, IBeing iBeing)
        {
            var being = (Being)iBeing;
            EmitKVP(emitter, "BeingType", being.BeingType);
            EmitKVP(emitter, "ID", being.ID.ToString());

            EmitKVP(emitter, "Foreground", being.Foreground.ToString());
            EmitKVP(emitter, "Background", being.Background.ToString());
            EmitKVP(emitter, "Glyph", ((char)being.Glyph).ToString());

            EmitKVP(emitter, "Name", being.Name ?? string.Empty);
            EmitKVP(emitter, "Awareness", being.Awareness.ToString());
            EmitKVP(emitter, "Health", being.Health.ToString());
            EmitKVP(emitter, "Position", being.Position.ToString());
        }

        private IBeing ParseBeing(IParser parser)
        {
            uint id = uint.Parse(GetValueNext(parser, "ID"));

            string fgText = GetValueNext(parser, "Foreground");
            string bgText = GetValueNext(parser, "Background");
            int glyph = GetValueNext(parser, "Glyph")[0];

            Color fg = Color_FromString(fgText);
            Color bg = Color_FromString(bgText);
            var being = new Being(fg, bg, glyph, id);

            being.Name = GetValueNext(parser, "Name");
            being.Awareness = int.Parse(GetValueNext(parser, "Awareness"));
            being.Health = int.Parse(GetValueNext(parser, "Health"));
            being.Position = Point_FromString(GetValueNext(parser, "Position"));
            return being;
        }

        private void EmitPlayer(IEmitter emitter, IBeing iBeing)
        {
            EmitBeing(emitter, iBeing);
            //var player = (Player)iBeing;
        }
    }
}
