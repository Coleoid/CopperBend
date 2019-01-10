﻿using System;
using System.Collections.Generic;
using System.Linq;
using RLNET;

namespace CopperBend.App
{
    public class GameWindow
    {
        public RLRootConsole RootConsole { get; set; }

        private RLConsole MapPane;
        private int MapWidth = 60;
        private int MapHeight = 60;

        private RLConsole StatPane;
        private int StatWidth = 20;
        private int StatHeight = 60;

        public RLConsole TextPane { get; set; }
        private int TextWidth = 80;
        private int TextHeight = 20;

        public RLConsole LargePane { get; set; }
        private int LargeWidth = 60;
        private int LargeHeight = 60;
        private bool LargePaneVisible;

        private Queue<RLKeyPress> InputQueue;
        public Queue<string> MessageQueue;
        private EventBus EventBus;

        public bool WaitingAtMorePrompt = false;
        public bool DisplayDirty { get; set; } = false;
        private int ShownMessages = 0;


        public GameWindow(Queue<RLKeyPress> inputQueue, EventBus eventBus)
        {
            InputQueue = inputQueue;
            EventBus = eventBus;
            var consoleSettings = new RLSettings
            {
                Title = "Copper Bend",
                BitmapFile = "assets\\terminal12x12_gs_ro.png",
                Width = 80,
                Height = 80,
                CharWidth = 12,
                CharHeight = 12,
                Scale = 1f,
                WindowBorder = RLWindowBorder.Resizable,
                ResizeType = RLResizeType.ResizeCells,
            };

            RootConsole = new RLRootConsole(consoleSettings);
            MapPane = new RLConsole(MapWidth, MapHeight);
            StatPane = new RLConsole(StatWidth, StatHeight);
            TextPane = new RLConsole(TextWidth, TextHeight);
            LargePane = new RLConsole(LargeWidth, LargeHeight);
            LargePaneVisible = false;


            MessageQueue = new Queue<string>();
            EventBus.SendLargeMessageSubscribers += LargeMessage;

        }

        internal void Render(IAreaMap map)
        {
            //FUTURE:  real-time (background) animation around here

            bool rootDirty = false;

            if (map.DisplayDirty)
            {
                map.DrawMap(MapPane);
                RLConsole.Blit(MapPane, 0, 0, MapWidth, MapHeight, RootConsole, 0, 0);
                map.DisplayDirty = false;
                rootDirty = true;
            }

            //  I haven't even begun to code status reporting
            //  ...I've barely thought about it.
            //  Health reporting should be vague, perhaps just to begin with
            //  Magical energy is called...  something that's not greek.
            //  Perhaps two fatigue/physical energy pools?  Wind and Vitality?
            //  I'm not gathering experience from anywhere yet
            //  No status effects occurring yet (haste, confusion, ...)
            //if (Stats.DisplayDirty)
            //{
            //    Stats.Report(StatConsole);
            //    RLConsole.Blit(MapConsole, 0, 0, StatWidth, StatHeight, RootConsole, MapWidth, 0);
            //    Stats.DisplayDirty = false;
            //    rootDirty = true;
            //}

            //if (GameWindow.DisplayDirty)
            //{
            RLConsole.Blit(TextPane, 0, 0, TextWidth, TextHeight, RootConsole, 0, MapHeight);
            rootDirty = true;
            //GameWindow.DisplayDirty = false;
            //}

            //  Large messages blitted last since they overlay the rest of the panes
            if (LargePaneVisible)
            {
                RLConsole.Blit(LargePane, 0, 0, LargeWidth, LargeHeight, RootConsole, 10, 10);
            }

            if (rootDirty)
            {
                //RootConsole.Clear();  //  por que?
                RootConsole.Draw();
                rootDirty = false;
            }
        }

        internal void Run(UpdateEventHandler onUpdate, UpdateEventHandler onRender)
        {
            RootConsole.Update += onUpdate;
            RootConsole.Render += onRender;
            RootConsole.Run();
        }

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

                    EventBus.MessagePanelFull(this, new EventArgs());
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
            EventBus.AllMessagesSent(this, new EventArgs());
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

            EventBus.ClearLargeMessage(this, new EventArgs());
        }

        internal void LargeMessage(object sender, LargeMessageEventArgs args)
        {
            var lines = args.Lines.ToList();

            int cY = 1;

            foreach (var line in lines)
            {
                int linesWritten = LargePane.Print(1, cY, 0, line, Palette.PrimaryLighter, new RLColor(0, 0, 0), 58, 1);                cY += linesWritten;
            }
        }
    }
}
