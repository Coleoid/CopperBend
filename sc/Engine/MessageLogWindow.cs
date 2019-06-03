﻿using System;
using System.Collections.Generic;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;
using SadConsole;
using GoRogue;

namespace CopperBend.Engine
{
    public class MessageLogWindow : Window
    {
        //max number of lines to store in message log
        private static readonly int _maxLines = 100;
        private readonly List<string> _lines;


        private ScrollingConsole _messageConsole;
        private SadConsole.Controls.ScrollBar _messageScrollBar;
        private int _scrollBarCurrentPosition;

        // account for the thickness of the window border to prevent UI element spillover
        private int _windowBorderThickness = 2;

        public MessageLogWindow(int width, int height, string title) : base(width, height)
        {
            DefaultBackground = Color.DarkGreen;
            // Ensure that the window background is the correct colour
            Theme.WindowTheme.FillStyle.Background = DefaultBackground;
            _lines = new List<string>();
            CanDrag = true;
            Title = title.Align(HorizontalAlignment.Center, Width);

            // add the message console, reposition, enable the viewport, and add it to the window
            _messageConsole = new ScrollingConsole(width - _windowBorderThickness, _maxLines);
            _messageConsole.Position = new Coord(1, 1);
            _messageConsole.ViewPort = new Rectangle(0, 0, width - 1, height - _windowBorderThickness);

            // create a scrollbar and attach it to an event handler, then add it to the Window
            _messageScrollBar = new SadConsole.Controls.ScrollBar(Orientation.Vertical, height - _windowBorderThickness)
            {
                Position = new Coord(_messageConsole.Width + 1, _messageConsole.Position.X),
                IsEnabled = false
            };
            _messageScrollBar.ValueChanged += MessageScrollBar_ValueChanged;
            Add(_messageScrollBar);

            // enable mouse input
            UseMouse = true;

            // Add the child consoles to the window
            Children.Add(_messageConsole);
        }

        void MessageScrollBar_ValueChanged(object sender, EventArgs e)
        {
            _messageConsole.ViewPort = new Rectangle(0, _messageScrollBar.Value + _windowBorderThickness, _messageConsole.Width, _messageConsole.ViewPort.Height);
        }

        public override void Draw(TimeSpan drawTime)
        {
            base.Draw(drawTime);
        }

        public override void Update(TimeSpan time)
        {
            base.Update(time);

            // Ensure that the scrollbar tracks the current position of the _messageConsole.
            if (_messageConsole.TimesShiftedUp != 0 | _messageConsole.Cursor.Position.Y >= _messageConsole.ViewPort.Height + _scrollBarCurrentPosition)
            {
                //enable the scrollbar once the messagelog has filled up with enough text to warrant scrolling
                _messageScrollBar.IsEnabled = true;

                // Make sure we've never scrolled the entire size of the buffer
                if (_scrollBarCurrentPosition < _messageConsole.Height - _messageConsole.ViewPort.Height)
                    // Record how much we've scrolled to enable how far back the bar can see
                    _scrollBarCurrentPosition += _messageConsole.TimesShiftedUp != 0 ? _messageConsole.TimesShiftedUp : 1;

                // Determines the scrollbar's max vertical position
                // Thanks @Kaev for simplifying this math!
                _messageScrollBar.Maximum = _scrollBarCurrentPosition - _windowBorderThickness;

                // This will follow the cursor since we move the render area in the event.
                _messageScrollBar.Value = _scrollBarCurrentPosition;

                // Reset the shift amount.
                _messageConsole.TimesShiftedUp = 0;
            }
        }

        private bool isNewLine = true;
        private int cursor_x = 1;
        /// <summary> add a complete line to the messages </summary>
        public void WriteLine(string message)
        {
            add_message_to_list(message);
            Coord cursor = (cursor_x, _lines.Count);
            _messageConsole.Cursor.Position = cursor;
            _messageConsole.Cursor.Print(message + "\n");
            cursor_x = 1;
            isNewLine = true;
        }

        /// <summary> add an unfinished line to the messages </summary>
        public void Prompt(string message)
        {
            add_message_to_list(message);
            Coord cursor = (cursor_x, _lines.Count);
            _messageConsole.Cursor.Position = cursor;
            _messageConsole.Cursor.Print(message);
            cursor_x += message.Length;
            isNewLine = false;
        }

        private void add_message_to_list(string message)
        {
            if (isNewLine)
                _lines.Add(message);
            else
                _lines[_lines.Count - 1] = _lines[_lines.Count - 1] + message;  // macro-yecch-tacular.

            if (_lines.Count > _maxLines) { _lines.RemoveRange(0, _lines.Count - _maxLines); }
        }
    }
}
