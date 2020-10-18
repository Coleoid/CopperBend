using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace CopperBend.Creation
{
    public class YamlEmitter_QuoteBuggedStrings : ChainedEventEmitter
    {
        public YamlEmitter_QuoteBuggedStrings(IEventEmitter nextEmitter)
            : base(nextEmitter)
        { }

        public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
        {
            if (eventInfo.Source.Type == typeof(string))
            {
                if (!(eventInfo.Source.Value is string str))
                {
                }
                else if (str.Contains("\n") || str.Contains("\t"))
                {
                    eventInfo.Style = ScalarStyle.DoubleQuoted;
                }
                else if (str.Contains("~"))
                {
                    eventInfo.Style = ScalarStyle.SingleQuoted;
                }
            }
            base.Emit(eventInfo, emitter);
        }
    }
}
