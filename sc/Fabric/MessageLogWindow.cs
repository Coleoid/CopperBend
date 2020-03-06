using System;
using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using SadConsole;
using GoRogue;
using CopperBend.Contract;

namespace CopperBend.Fabric
{
    public class MessageLogWindow : Window, IMessageLogWindow
    {
        //max number of lines to store in message log
        private const int MaxLines = 100;
        private const int WindowBorderThickness = 2;

        private readonly List<string> lines;

        private readonly ScrollingConsole messageConsole;
        private readonly SadConsole.Controls.ScrollBar messageScrollBar;
        private int scrollBarCurrentPosition;

        // account for the thickness of the window border to prevent UI element spillover

        public MessageLogWindow(int width, int height, string title)
            : base(width, height)
        {
            DefaultBackground = Color.DarkOliveGreen;
            // Ensure that the window background is the correct colour
            //Theme.WindowTheme.FillStyle.Background = DefaultBackground;
            //Theme.WindowTheme.BorderStyle.Background = Color.DarkOliveGreen;//the goggles
            lines = new List<string>();
            CanDrag = true;
            Title = title.Align(HorizontalAlignment.Center, Width);

            // add the message console, reposition, enable the viewport, and add it to the window
            messageConsole = new ScrollingConsole(width - WindowBorderThickness, MaxLines); //0.1: change msgs to rect. font
            messageConsole.Position = new Coord(1, 1);
            messageConsole.ViewPort = new Rectangle(0, 0, width - 1, height - WindowBorderThickness);
            messageConsole.DefaultBackground = Color.Black;
            messageConsole.Font = Global.FontDefault.Master.GetFont(Font.FontSizes.One);

            // create a scrollbar and attach it to an event handler, then add it to the Window
            messageScrollBar = new SadConsole.Controls.ScrollBar(Orientation.Vertical, height - WindowBorderThickness)
            {
                Position = new Coord(messageConsole.Width + 1, messageConsole.Position.X),
                IsEnabled = false,
            };
            messageScrollBar.ValueChanged += MessageScrollBar_ValueChanged;
            Add(messageScrollBar);

            // enable mouse input
            UseMouse = true;

            // Add the child consoles to the window
            Children.Add(messageConsole);
        }

        private void MessageScrollBar_ValueChanged(object sender, EventArgs e)
        {
            messageConsole.ViewPort = new Rectangle(0, messageScrollBar.Value + WindowBorderThickness, messageConsole.Width, messageConsole.ViewPort.Height);
        }

        public override void Draw(TimeSpan drawTime)
        {
            base.Draw(drawTime);
        }

        public override void Update(TimeSpan time)
        {
            base.Update(time);

            // Ensure that the scrollbar tracks the current position of the _messageConsole.
            if (messageConsole.TimesShiftedUp != 0 | messageConsole.Cursor.Position.Y >= messageConsole.ViewPort.Height + scrollBarCurrentPosition)
            {
                //enable the scrollbar once the messagelog has filled up with enough text to warrant scrolling
                messageScrollBar.IsEnabled = true;

                // Make sure we've never scrolled the entire size of the buffer
                if (scrollBarCurrentPosition < messageConsole.Height - messageConsole.ViewPort.Height)
                    // Record how much we've scrolled to enable how far back the bar can see
                    scrollBarCurrentPosition += messageConsole.TimesShiftedUp != 0 ? messageConsole.TimesShiftedUp : 1;

                // Determines the scrollbar's max vertical position
                // Thanks @Kaev for simplifying this math!
                messageScrollBar.Maximum = scrollBarCurrentPosition - WindowBorderThickness;

                // This will follow the cursor since we move the render area in the event.
                messageScrollBar.Value = scrollBarCurrentPosition;

                // Reset the shift amount.
                messageConsole.TimesShiftedUp = 0;
            }
        }

        private bool isNewLine = true;
        private int cursor_x = 1;
        /// <summary> add a complete line to the messages </summary>
        public void WriteLine(string message)
        {
            Add_message_to_list(message);
            Coord cursor = (cursor_x, lines.Count);
            messageConsole.Cursor.Position = cursor;
            messageConsole.Cursor.Print(message + "\n");
            cursor_x = 1;
            isNewLine = true;
        }

        /// <summary> add an unfinished line to the messages </summary>
        public void Prompt(string message)
        {
            Add_message_to_list(message);
            Coord cursor = (cursor_x, lines.Count);
            messageConsole.Cursor.Position = cursor;
            messageConsole.Cursor.Print(message);
            cursor_x += message.Length;
            isNewLine = false;
        }

        private void Add_message_to_list(string message)
        {
            if (isNewLine)
            {
                lines.Add(message);
            }
            else
            {
                lines[lines.Count - 1] = lines[lines.Count - 1] + message;  // macro-yecch-tacular.
            }

            if (lines.Count > MaxLines) { lines.RemoveRange(0, lines.Count - MaxLines); }
        }
    }


    public class NarrativeWindow : Window //, IMessageLogWindow
    {
        private const int WindowBorderThickness = 2;

        ////max number of lines to store in message log
        private static int maxLines;
        private readonly List<string> lines;


        private readonly ScrollingConsole messageConsole;
        private readonly SadConsole.Controls.ScrollBar messageScrollBar;
        private int scrollBarCurrentPosition;

        // account for the thickness of the window border to prevent UI element spillover

        public NarrativeWindow(int width, int height, string title)
            : base(width, height)
        {
            maxLines = height;
            DefaultBackground = Color.DarkOliveGreen;
            // Ensure that the window background is the correct colour
            //Theme.WindowTheme.FillStyle.Background = DefaultBackground;
            //Theme.WindowTheme.BorderStyle.Background = Color.DarkOliveGreen;//the goggles
            lines = new List<string>();
            CanDrag = true;
            Title = title.Align(HorizontalAlignment.Center, Width);

            // add the message console, reposition, enable the viewport, and add it to the window
            messageConsole = new ScrollingConsole(width - WindowBorderThickness, maxLines)
            {
                Position = new Coord(1, 1),
                ViewPort = new Rectangle(0, 0, width - 1, height - WindowBorderThickness),
                DefaultBackground = Color.Black,
                Font = Global.FontDefault.Master.GetFont(Font.FontSizes.One),
                //Font = ?? //0.1: change msgs to rect. font
            };

            // create a scrollbar and attach it to an event handler, then add it to the Window
            messageScrollBar = new SadConsole.Controls.ScrollBar(Orientation.Vertical, height - WindowBorderThickness)
            {
                Position = new Coord(messageConsole.Width + 1, messageConsole.Position.X),
                IsEnabled = false,
            };
            messageScrollBar.ValueChanged += MessageScrollBar_ValueChanged;
            Add(messageScrollBar);

            // enable mouse input
            UseMouse = true;

            // Add the child consoles to the window
            Children.Add(messageConsole);
        }

        private void MessageScrollBar_ValueChanged(object sender, EventArgs e)
        {
            messageConsole.ViewPort = new Rectangle(0, messageScrollBar.Value + WindowBorderThickness, messageConsole.Width, messageConsole.ViewPort.Height);
        }

        public override void Draw(TimeSpan drawTime)
        {
            base.Draw(drawTime);
        }

        public override void Update(TimeSpan time)
        {
            base.Update(time);

            // Ensure that the scrollbar tracks the current position of the _messageConsole.
            if (messageConsole.TimesShiftedUp != 0 | messageConsole.Cursor.Position.Y >= messageConsole.ViewPort.Height + scrollBarCurrentPosition)
            {
                //enable the scrollbar once the messagelog has filled up with enough text to warrant scrolling
                messageScrollBar.IsEnabled = true;

                // Make sure we've never scrolled the entire size of the buffer
                if (scrollBarCurrentPosition < messageConsole.Height - messageConsole.ViewPort.Height)
                    // Record how much we've scrolled to enable how far back the bar can see
                    scrollBarCurrentPosition += messageConsole.TimesShiftedUp != 0 ? messageConsole.TimesShiftedUp : 1;

                // Determines the scrollbar's max vertical position
                // Thanks @Kaev for simplifying this math!
                messageScrollBar.Maximum = scrollBarCurrentPosition - WindowBorderThickness;

                // This will follow the cursor since we move the render area in the event.
                messageScrollBar.Value = scrollBarCurrentPosition;

                // Reset the shift amount.
                messageConsole.TimesShiftedUp = 0;
            }
        }

        private bool isNewLine = true;
        private int cursor_x = 1;
        /// <summary> add a complete line to the messages </summary>
        public void WriteLine(string message)
        {
            Add_message_to_list(message);
            Coord cursor = (cursor_x, lines.Count);
            messageConsole.Cursor.Position = cursor;
            messageConsole.Cursor.Print(message + "\n");
            cursor_x = 1;
            isNewLine = true;
        }

        /// <summary> add an unfinished line to the messages </summary>
        public void Prompt(string message)
        {
            Add_message_to_list(message);
            Coord cursor = (cursor_x, lines.Count);
            messageConsole.Cursor.Position = cursor;
            messageConsole.Cursor.Print(message);
            cursor_x += message.Length;
            isNewLine = false;
        }

        private void Add_message_to_list(string message)
        {
            if (isNewLine)
            {
                lines.Add(message);
            }
            else
            {
                lines[lines.Count - 1] = lines[lines.Count - 1] + message;  // macro-yecch-tacular.
            }

            if (lines.Count > maxLines) { lines.RemoveRange(0, lines.Count - maxLines); }
        }
    }
}
