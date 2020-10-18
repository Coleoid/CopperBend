using System;

namespace CopperBend.Logic.Tests
{
    public partial class Tests_Base
    {
        [Flags]
        public enum MockableServices
        {
            None = 0,
            Log = 1,
            Schedule = 2,
            Describer = 4,
            Messager = 8,
            EntityFactory = 16,
            MessageLogWindow = 32,
            ControlPanel = 64,

            // et c...

            All = int.MaxValue,
        }
    }
}
