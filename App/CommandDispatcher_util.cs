using RLNET;
using RogueSharp;
using System;
using CopperBend.App.Basis;

namespace CopperBend.App
{
    public partial class CommandDispatcher : IControlPanel
    {
        public void WriteLine(string text)
        {
            Console.Out.WriteLine(text);
        }

        public void Prompt(string text)
        {
            Console.Out.Write(text);
            Console.Out.Flush();
        }

        private int AlphaIndexOfKeyPress(RLKeyPress key)
        {
            if (!key.Char.HasValue) return -1;
            var asciiNum = (int)key.Char.Value;
            if (asciiNum < 97 || asciiNum > 123) return -1;
            return asciiNum - 97;
        }

        private bool IsPlayerScheduled = false;
        public void PlayerBusyFor(int ticks)
        {
            Scheduler.Add(new ScheduleEntry(ticks, PlayerReadyForInput));
            GameState.Mode = GameMode.Schedule;
            IsPlayerScheduled = true;
        }

        private ScheduleEntry PlayerReadyForInput(ScheduleEntry entry, IControlPanel controls)
        {
            controls.SwitchGameToMode(GameMode.PlayerReady);
            IsPlayerScheduled = false;
            return null;
        }

        private ICoord newCoord(ICoord start, Direction direction)
        {
            int newX = start.X;
            int newY = start.Y;

            if (direction == Direction.Up
                || direction == Direction.UpLeft
                || direction == Direction.UpRight)
            {
                newY--;
            }

            if (direction == Direction.Down
                || direction == Direction.DownLeft
                || direction == Direction.DownRight)
            {
                newY++;
            }

            if (direction == Direction.Left
                || direction == Direction.UpLeft
                || direction == Direction.DownLeft)
            {
                newX--;
            }

            if (direction == Direction.Right
                || direction == Direction.UpRight
                || direction == Direction.DownRight)
            {
                newX++;
            }

            return new Coord(newX, newY);
        }

        private Direction DirectionOfKey(RLKeyPress keyPress)
        {
            return
                keyPress.Key == RLKey.Up ? Direction.Up :
                keyPress.Key == RLKey.Down ? Direction.Down :
                keyPress.Key == RLKey.Left ? Direction.Left :
                keyPress.Key == RLKey.Right ? Direction.Right :

                keyPress.Key == RLKey.Keypad1 ? Direction.DownLeft :
                keyPress.Key == RLKey.Keypad2 ? Direction.Down :
                keyPress.Key == RLKey.Keypad3 ? Direction.DownRight :
                keyPress.Key == RLKey.Keypad4 ? Direction.Left :
                keyPress.Key == RLKey.Keypad6 ? Direction.Right :
                keyPress.Key == RLKey.Keypad7 ? Direction.UpLeft :
                keyPress.Key == RLKey.Keypad8 ? Direction.Up :
                keyPress.Key == RLKey.Keypad9 ? Direction.UpRight :
                Direction.None;
        }

        public void AddToSchedule(ScheduleEntry entry)
        {
            Scheduler.Add(entry);
        }

        public void SetMapDirty()
        {
            Map.DisplayDirty = true;
        }

        public void SwitchGameToMode(GameMode mode)
        {
            GameState.Mode = mode;
        }
    }
}
