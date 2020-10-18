using System;
using SadConsole;

namespace CopperBend.Logic
{
    public class TopConsole : ContainerConsole
    {
        public Action<TimeSpan> EngineUpdate { get; set; }

        public override void Update(TimeSpan timeElapsed)
        {
            EngineUpdate(timeElapsed);
            base.Update(timeElapsed);
        }
    }
}
