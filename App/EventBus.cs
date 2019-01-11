using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CopperBend.App
{
    public class EventBus
    {
        private ILog log;
        public event EventHandler MessagePanelFullSubscribers;
        public event EventHandler AllMessagesSentSubscribers;
        public event EventHandler<LargeMessageEventArgs> SendLargeMessageSubscribers;
        public event EventHandler ClearLargeMessageSubscribers;

        public EventBus()
        {
            log = LogManager.GetLogger("CB.Bus");
            log.Debug("Bus created");
        }

        public void MessagePanelFull(object sender, EventArgs args)
        {
            log.Debug("MessagePanelFull");
            MessagePanelFullSubscribers?.Invoke(sender, args);
        }

        public void AllMessagesSent(object sender, EventArgs args)
        {
            log.Debug("AllMessagesSent");
            AllMessagesSentSubscribers?.Invoke(sender, args);
        }

        internal void SendLargeMessage(object sender, IEnumerable<string> lines)
        {
            Guard.AgainstNullArgument(lines);
            log.Debug($"SendLargeMessage with {lines.Count()} lines");
            if (lines.Any())
                SendLargeMessage(sender, new LargeMessageEventArgs(lines));
        }

        internal void SendLargeMessage(object sender, LargeMessageEventArgs args)
        {
            log.Debug("SendLargeMessage");
            SendLargeMessageSubscribers?.Invoke(sender, args);
        }

        internal void ClearLargeMessage(object sender, EventArgs args)
        {
            log.Debug("ClearLargeMessage");
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
