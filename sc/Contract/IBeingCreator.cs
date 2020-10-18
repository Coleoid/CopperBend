using Microsoft.Xna.Framework;
using YamlDotNet.Core;

namespace CopperBend.Contract
{
    public interface IBeingCreator
    {
        IBeing Being_FromYaml(IParser parser);
        void Being_ToYaml(IEmitter emitter, IBeing iBeing);

        IBeing CreateBeing(string beingName);
        IBeing CreateBeing(Color foreground, Color background, int glyph, uint id = uint.MaxValue);
    }
}
