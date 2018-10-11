﻿using RLNET;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public ICoord PlayerCoords => Player;

        public void PlayerBusyFor(int ticks)
        {
            Scheduler.Add(new ScheduleEntry(ticks, PlayerReadyForInput));
            GameState.Mode = GameMode.Schedule;
            IsPlayerScheduled = true;
        }

        private void PlayerReadyForInput(IControlPanel controls, ScheduleEntry entry)
        {
            GameState.Mode = GameMode.PlayerReady;
            IsPlayerScheduled = false;
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

        public bool CanActorSeeTarget(IActor actor, ICoord target)
        {
            //FINISH: one FOV and one Pathfinder per map
            FieldOfView fov = new FieldOfView(Map);
            fov.ComputeFov(actor.X, actor.Y, actor.Awareness, true);
            return fov.IsInFov(target.X, target.Y);
        }

        public void AttackPlayer()
        {
            //0.0
            Player.AdjustHealth(-2);
            WriteLine("the thingy hit you for 2 points!");
            if (Player.Health < 1)
            {
                WriteLine("You die...");
                //TODO: die
            }
        }

        public List<ICoord> GetPathTo(ICoord start, ICoord target)
        {
            Map.SetIsWalkable(start, true);
            Map.SetIsWalkable(target, true);

            PathFinder pathFinder = new PathFinder(Map, 1.0, Math.Sqrt(2));

            var pathList = pathFinder.ShortestPathList(start, target);

            Map.SetIsWalkable(start, false);
            Map.SetIsWalkable(target, false);

            return pathList;
        }

        public bool MoveActorTo(IActor actor, ICoord step)
        {
            return Map.SetActorPosition(actor, step.X, step.Y);
        }

        public void RemoveFromInventory(IItem item)
        {
            Player.RemoveFromInventory(item);
            if (_usingItem == item)
                _usingItem = null;
        }

        public void GiveToPlayer(IItem item)
        {
            Player.AddToInventory(item);
        }

        public RLKeyPress GetNextKeyPress()
        {
            return InputQueue.Any() ? InputQueue.Dequeue() : null;
        }

        public void MessagePanelFull()
        {
            GameState.Mode = GameMode.MessagesPending;
        }

        public void AllMessagesSent()
        {
            GameState.Mode = IsPlayerScheduled ?
                GameMode.Schedule : GameMode.PlayerReady;
        }

        public void HealPlayer(int amount)
        {
            Player.AdjustHealth(amount);
        }

        public void PutItemOnMap(IItem item)
        {
            Map.Items.Add(item);
        }

        public void RemovePlantAt(ICoord coord)
        {
            Map.Tiles[coord.X, coord.Y].RemovePlant();
        }
    }
}
