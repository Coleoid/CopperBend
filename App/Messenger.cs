using System;
using RLNET;
using System.Collections.Generic;
using System.Linq;

namespace CopperBend.App
{
    public class Messenger
    {
        private Queue<RLKeyPress> InputQueue;
        private readonly RLConsole TextPane;
        private readonly RLConsole LargePane;

        public Messenger(Queue<RLKeyPress> inputQueue, RLConsole textPane, RLConsole largePane)
        {
            InputQueue = inputQueue;
            TextPane = textPane;
            LargePane = largePane;
            MessageQueue = new Queue<string>();
            EventBus.OurBus.SendLargeMessageSubscribers += LargeMessage;
        }

        public Queue<string> MessageQueue;
        public bool WaitingAtMorePrompt = false;
        public bool DisplayDirty { get; set; } = false;
        private int ShownMessages = 0;

        public void AddMessage(string newMessage)
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
                    
                    EventBus.OurBus.MessagePanelFull(this, new EventArgs());
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
            InputQueue.Clear();
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
            EventBus.OurBus.AllMessagesSent(this, new EventArgs());
        }

        private const int textConsoleHeight = 12;

        int CursorX = 1;
        int CursorY = 1;  //  I haven't looked at this closely yet

        struct MessageLine
        {
            public string Text { get; set; }
            public int Lines { get; set; }
        }
        private Queue<MessageLine> MessageLines = new Queue<MessageLine>();

        public void WriteLine(string text)
        {
            if (promptText != string.Empty)
            {
                text = promptText + text;
                promptText = string.Empty;
            }

            int maxLinesWritable = textConsoleHeight - CursorY - 1;
            int linesWritten = TextPane.Print(CursorX, CursorY, maxLinesWritable, text, Palette.PrimaryLighter, new RLColor(0, 0, 0), 78, 1);

            var newMessage = new MessageLine { Text = text, Lines = linesWritten };
            MessageLines.Enqueue(newMessage);
            CursorY += linesWritten;
            CursorX = 1;

            //  Time to scroll?
            bool scrollTime = CursorY == textConsoleHeight;
            while (scrollTime)
            {
                TextPane.Clear();
                MessageLines.Dequeue();
                CursorY = 1;
                foreach (var msg in MessageLines)
                {
                    linesWritten = TextPane.Print(CursorX, CursorY, maxLinesWritable, msg.Text, Palette.PrimaryLighter, new RLColor(0, 0, 0), 78, 1);
                    CursorY += linesWritten;
                }

                //  Were we cutting off some of the latest message?  Then scroll more...
                scrollTime = linesWritten != newMessage.Lines;
                //  ...unless that single message is now filling the text console.
                scrollTime = scrollTime && MessageLines.Count() > 1;
            }
            //  This works fine, though does significantly more graphic work
            //  than necessary--if this shows slowness, we can get wins here.
        }

        private string promptText = string.Empty;
        public void Prompt(string text)
        {
            promptText += text;
            TextPane.Print(1, CursorY, 5, promptText, Palette.PrimaryLighter, new RLColor(0, 0, 0), 78, 1);
            CursorX += promptText.Length;
        }

        //  The engine calls here when we're in GameMode.LargeMessagePending
        public void HandleLargeMessage()
        {
            RLKeyPress press = GetNextKeyPress();
            while (press != null && press.Key != RLKey.Escape)
            {
                press = GetNextKeyPress();
            }

            if (press == null) return;

            EventBus.OurBus.ClearLargeMessage(this, new EventArgs());
        }

        internal void LargeMessage(object sender, LargeMessageEventArgs args)
        {
            var lines = args.Lines.ToList();

            int cY = 1;
            
            foreach (var line in lines)
            {
                int linesWritten = LargePane.Print(1, cY, 0, line, Palette.PrimaryLighter, new RLColor(0, 0, 0), 58, 1);
                cY += linesWritten;
            }
        }
    }
}
