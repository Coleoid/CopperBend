using CopperBend.App.Basis;
using RLNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopperBend.App
{
    public class Messenger
    {
        public Queue<string> MessageQueue;
        public bool WaitingAtMorePrompt = false;
        private readonly IControlPanel Controls;
        public Messenger(IControlPanel controls)
        {
            MessageQueue = new Queue<string>();
            Controls = controls;
        }

        private int ShownMessages = 0;

        public void Message(string newMessage)
        {
            MessageQueue.Enqueue(newMessage);
            ShowMessages();
        }

        public void ShowMessages()
        {
            while (!WaitingAtMorePrompt && MessageQueue.Any())
            {
                if (ShownMessages >= 3)
                {
                    Controls.WriteLine("-- more --");
                    WaitingAtMorePrompt = true;
                    Controls.MessagePanelFull();
                    return;
                }

                var nextMessage = MessageQueue.Dequeue();
                Controls.WriteLine(nextMessage);
                ShownMessages++;
            }
        }

        public void ClearMessagePanel()
        {
            //0.1
            ShownMessages = 0;
            WaitingAtMorePrompt = false;
        }

        public void HandlePendingMessages()
        {
            if (!WaitingAtMorePrompt) return;

            while (WaitingAtMorePrompt)
            {
                //  Advance to next space keypress, if any
                RLKeyPress key = Controls.GetNextKeyPress();
                while (key != null && key.Key != RLKey.Space)
                {
                    key = Controls.GetNextKeyPress();
                }

                //  If we run out of keypresses before we find a space,
                // the rest of the messages remain pending
                if (key?.Key != RLKey.Space) return;

                //  Otherwise, show more messages
                ClearMessagePanel();
                ShowMessages();
            }

            //  If we reach this point, we sent all messages
            Controls.AllMessagesSent();
        }

    }
}
