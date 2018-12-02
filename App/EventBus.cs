using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CopperBend.App
{
    public class EventBus
    {
        public EventBus(string magicWord)
        {
            if (magicWord != "please")
                throw new Exception("EventBus.OurBus is probably what you want.");
        }

        private static EventBus _ourBusInstance;
        public static EventBus OurBus 
        {
            get
            {
                if (_ourBusInstance == null)
                    _ourBusInstance = new EventBus("please");
                return _ourBusInstance;
            }
            set
            {
                _ourBusInstance = value;
                LogManager.GetLogger("CopperBend")
                    .Warn("Changing the EventBus may abandon subscribers.");
            }
        }

        public event EventHandler MessagePanelFullSubscribers;
        public event EventHandler AllMessagesSentSubscribers;
        public event EventHandler<LargeMessageEventArgs> SendLargeMessageSubscribers;
        public event EventHandler ClearLargeMessageSubscribers;

        public void MessagePanelFull(object sender, EventArgs args)
        {
            MessagePanelFullSubscribers?.Invoke(sender, args);
        }

        public void AllMessagesSent(object sender, EventArgs args)
        {
            AllMessagesSentSubscribers?.Invoke(sender, args);
        }

        internal void SendLargeMessage(object sender, IEnumerable<string> lines)
        {
            if (lines.Any())
                SendLargeMessage(sender, new LargeMessageEventArgs(lines));
        }

        internal void SendLargeMessage(object sender, LargeMessageEventArgs args)
        {
            SendLargeMessageSubscribers?.Invoke(sender, args);
        }

        internal void ClearLargeMessage(object sender, EventArgs args)
        {
            ClearLargeMessageSubscribers?.Invoke(sender, args);
        }
    }

    public class LargeMessageEventArgs : EventArgs
    {
        public IEnumerable<string> Lines;
        public LargeMessageEventArgs(IEnumerable<string> lines)
        {
            Lines = lines;
        }
    }
}
