using System;
using RLNET;

namespace CopperBend.App
{
    public class EventArgs_KeyPress : EventArgs
    {
        public RLKeyPress KeyPress { get; private set; }
        public bool Cancel { get; set; }

        public EventArgs_KeyPress(RLKeyPress keyPress)
        {
            KeyPress = keyPress;
            Cancel = false;
        }
    }
}
