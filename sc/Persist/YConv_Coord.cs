using System;
using System.Globalization;
using System.Text.RegularExpressions;
using GoRogue;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace CopperBend.Persist
{
    public class YConv_Coord : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return typeof(Coord).IsAssignableFrom(type);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var value = ((Scalar)parser.Current).Value;
            parser.MoveNext();

            var match = Regex.Match(value, @"\(X=(-?\d+), Y=(-?\d+)\)");
            int x = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            int y = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);

            return new Coord(x, y);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var coord = (Coord)value;
            string coordText = $"(X={coord.X}, Y={coord.Y})";
            emitter.Emit(new Scalar(coordText));
        }
    }
}
