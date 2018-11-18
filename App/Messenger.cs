using System;
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

        public Messenger(Queue<RLKeyPress> inputQueue, RLConsole textConsole)
        {
            InputQueue = inputQueue;
            TextConsole = textConsole;
            MessageQueue = new Queue<string>();
        }

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
                    
                    EventBus.OurBus.RaiseMessagePanelFull(this, new EventArgs());
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
            EventBus.OurBus.RaiseAllMessagesSent(this, new EventArgs());
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
            int linesWritten = TextConsole.Print(CursorX, CursorY, maxLinesWritable, text, Palette.PrimaryLighter, new RLColor(0, 0, 0), 78, 1);

            var newMessage = new MessageLine { Text = text, Lines = linesWritten };
            MessageLines.Enqueue(newMessage);
            CursorY += linesWritten;
            CursorX = 1;

            //  Time to scroll?
            bool scrollTime = CursorY == textConsoleHeight;
            while (scrollTime)
            {
                TextConsole.Clear();
                MessageLines.Dequeue();
                CursorY = 1;
                foreach (var msg in MessageLines)
                {
                    linesWritten = TextConsole.Print(CursorX, CursorY, maxLinesWritable, msg.Text, Palette.PrimaryLighter, new RLColor(0, 0, 0), 78, 1);
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
            TextConsole.Print(1, CursorY, 5, promptText, Palette.PrimaryLighter, new RLColor(0, 0, 0), 78, 1);
            CursorX += promptText.Length;
        }

        internal void LargeMessage(List<string> message)
        {
            //TODO:  signal this? LargeMessageVisible = (message.Count > 0);
            foreach (var line in message)
            {
                LargeMessageLine(line);
            }

        }

        private Queue<MessageLine> LargeMessageLines = new Queue<MessageLine>();
        //PASTA  0.1  HACK  FIXME
        internal void LargeMessageLine(string line)
        {
            int maxLinesWritable = 60 - CursorY - 1;
            int linesWritten = TextConsole.Print(CursorX, CursorY, maxLinesWritable, line, Palette.PrimaryLighter, new RLColor(0, 0, 0), 78, 1);

            var newMessage = new MessageLine { Text = line, Lines = linesWritten };
            LargeMessageLines.Enqueue(newMessage);
            CursorY += linesWritten;
            CursorX = 1;

            //  Time to scroll?
            bool scrollTime = CursorY == textConsoleHeight;
            while (scrollTime)
            {
                TextConsole.Clear();
                LargeMessageLines.Dequeue();
                CursorY = 1;
                foreach (var msg in LargeMessageLines)
                {
                    linesWritten = TextConsole.Print(CursorX, CursorY, maxLinesWritable, msg.Text, Palette.PrimaryLighter, new RLColor(0, 0, 0), 78, 1);
                    CursorY += linesWritten;
                }

                //  Were we cutting off some of the latest message?  Then scroll more...
                scrollTime = linesWritten != newMessage.Lines;
                //  ...unless that single message is now filling the text console.
                scrollTime = scrollTime && LargeMessageLines.Count() > 1;
            }
            //  This works fine, though does significantly more graphic work
            //  than necessary--if this shows slowness, we can get wins here.

        }
    }
}
