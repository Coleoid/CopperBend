using System;
using System.Collections.Generic;
using System.Linq;
using CopperBend.Contract;
using CopperBend.Fabric;
using log4net;

namespace CopperBend.Engine
{
    public class EventBus
    {
        private ILog log;
        //public event EventHandler MessagePanelFullSubscribers;
        public event EventHandler AllMessagesSentSubscribers;
        public event EventHandler<LargeMessageEventArgs> SendLargeMessageSubscribers;
        public event EventHandler ClearLargeMessageSubscribers;
        public event EventHandler ClearPendingInputSubscribers;
        public event EventHandler<EnterModeEventArgs> EnterEngineModeSubscribers;

        public EventBus()
        {
            log = LogManager.GetLogger("CB", "CB.Bus");
            log.Debug("Bus created");
        }

        public void MessagePanelFull(object sender, EventArgs args)
        {
            log.Debug("MessagePanelFull");
            //MessagePanelFullSubscribers?.Invoke(sender, args);
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

        internal void ClearPendingInput(object sender, EventArgs args)
        {
            log.Debug("ClearPendingInput");
            ClearPendingInputSubscribers?.Invoke(sender, args);
        }

        internal void EnterMode(EngineMode mode, Func<bool> callback)
        {
            log.Debug($"Enter mode: {mode}");
            var args = new EnterModeEventArgs(mode, callback);
            EnterEngineModeSubscribers?.Invoke(this, args);
        }
    }

    public class EnterModeEventArgs : EventArgs
    {
        public EngineMode Mode;
        public Func<bool> Callback;

        public EnterModeEventArgs(EngineMode mode, Func<bool> callback)
        {
            Mode = mode;
            Callback = callback;
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
