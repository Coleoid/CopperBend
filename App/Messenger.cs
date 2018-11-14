using RLNET;
using System.Collections.Generic;
using System.Linq;

namespace CopperBend.App
{
    public class Messenger
    {
        public Queue<string> MessageQueue;
        public bool WaitingAtMorePrompt = false;
        //private readonly IControlPanel Controls;
        private readonly RLConsole TextConsole;
        private Queue<RLKeyPress> InputQueue;

        public Messenger(Queue<RLKeyPress> InputQueue, RLConsole textConsole)
        {
            TextConsole = textConsole;
            MessageQueue = new Queue<string>();
        }

        public bool DisplayDirty { get; set; } = false;

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
                    WriteLine("-- more --");
                    DisplayDirty = true;
                    WaitingAtMorePrompt = true;
                    Controls.MessagePanelFull();
                    return;
                }

                var nextMessage = MessageQueue.Dequeue();
                WriteLine(nextMessage);
                DisplayDirty = true;
                ShownMessages++;
            }
        }

        public void ResetWait()
        {
            //0.1
            ShownMessages = 0;
            WaitingAtMorePrompt = false;
        }

        public RLKeyPress GetNextKeyPress()
        {
            return InputQueue.Any() ? InputQueue.Dequeue() : null;
        }

        public void EmptyInputQueue()
        {
            while (InputQueue.Any())
                InputQueue.Dequeue();
        }

        public void HandlePendingMessages()
        {
            if (!WaitingAtMorePrompt) return;

            while (WaitingAtMorePrompt)
            {
                //  Advance to next space keypress, if any
                RLKeyPress key = GetNextKeyPress();
                while (key != null && key.Key != RLKey.Space)
                {
                    key = GetNextKeyPress();
                }

                //  If we run out of keypresses before we find a space,
                // the rest of the messages remain pending
                if (key?.Key != RLKey.Space) return;

                //  Otherwise, show more messages
                ResetWait();
                ShowMessages();
            }

            //  If we reach this point, we sent all messages
            Controls.AllMessagesSent();
        }

        private const int textConsoleHeight = 12;
        private Stack<string> Messages = new Stack<string>();
        private Stack<int> MessageLines = new Stack<int>();  // probably begging for desync bugs

        int CursorX = 1;
        int CursorY = 1;  //  I haven't looked at this closely yet
        public void WriteLine(string text)
        {
            int linesWritten = TextConsole.Print(1, 1, 5, text, Palette.PrimaryLighter, new RLColor(0, 0, 0), 40, 1);
        }

        public void Prompt(string text)
        {
            TextConsole.Print(1, 1, 5, text, Palette.PrimaryLighter, new RLColor(0, 0, 0), 40, 1);
        }
    }
}
