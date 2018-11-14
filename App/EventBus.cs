using System;
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

        public event EventHandler MessagePanelFull;
        public event EventHandler AllMessagesSent;

        public void RaiseMessagePanelFull(object sender, EventArgs args)
        {
            MessagePanelFull?.Invoke(sender, args);
        }

        public void RaiseAllMessagesSent(object sender, EventArgs args)
        {
            AllMessagesSent?.Invoke(sender, args);
        }
    }
}
