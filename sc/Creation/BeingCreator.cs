using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using YamlDotNet.Core;
using CopperBend.Contract;
using CopperBend.Model;

namespace CopperBend.Creation
{
    public class BeingCreator : IBeingCreator
    {
        private LateDependency<ISadConEntityFactory> lateSCEF;

        public ISadConEntityFactory SadConEntityFactory
        {
            get => lateSCEF.Value;
            set => lateSCEF.Value = value;
        }

        public IBeing Being_FromYaml(IParser parser)
        {
            var type = parser.GetKVP_string("BeingType");
            uint id = parser.GetKVP_uint("ID");

            Color fg = parser.GetKVP_Color("Foreground");
            Color bg = parser.GetKVP_Color("Background");
            int glyph = parser.GetKVP_string("Glyph")[0];

            var being = CreateBeing(fg, bg, glyph, id: id);

            being.Name = parser.GetKVP_string("Name");
            being.Awareness = parser.GetKVP_int("Awareness");
            being.Health = parser.GetKVP_int("Health");
            being.BeingType = type;

            return being;
        }

        public void Being_ToYaml(IEmitter emitter, IBeing iBeing)
        {
            var being = (Being)iBeing;
            emitter.EmitKVP("BeingType", being.BeingType);
            emitter.EmitKVP("ID", being.ID.ToString(CultureInfo.InvariantCulture));

            emitter.EmitKVP("Foreground", being.Foreground.ToString());
            emitter.EmitKVP("Background", being.Background.ToString());
            emitter.EmitKVP("Glyph", ((char)being.Glyph).ToString(CultureInfo.InvariantCulture));

            emitter.EmitKVP("Name", being.Name ?? string.Empty);
            emitter.EmitKVP("Awareness", being.Awareness.ToString(CultureInfo.InvariantCulture));
            emitter.EmitKVP("Health", being.Health.ToString(CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// 0.1: will grow to retrieve NPCs and instance beings from templates
        /// </summary>
        public IBeing CreateBeing(string beingName)
        {
            var being = beingName switch
            {
                "Suvail" => new Being(Guid.NewGuid(), Color.LawnGreen, Color.Black, '@')
                {
                    IsPlayer = true,
                    StrategyStyle = StrategyStyle.UserInput,
                },
                "flame rat" => new Being(Guid.NewGuid(), Color.Red, Color.Black, 'r'),
                "Phredde" => new Being(Guid.NewGuid(), Color.Gray, Color.Black, 'p'),
                _ => throw new Exception($"Don't know how to CreateBeing(\"{beingName}\")."),
            };
            being.Name = beingName;
            SadConEntityFactory?.SetIEntityOnPort(being);

            return being;
        }

        public IBeing CreateBeing(Color foreground, Color background, int glyph, uint id = uint.MaxValue)
        {
            var being = new Being(Guid.NewGuid(), foreground, background, glyph, id);

            SadConEntityFactory?.SetIEntityOnPort(being);

            return being;
        }
    }
}
