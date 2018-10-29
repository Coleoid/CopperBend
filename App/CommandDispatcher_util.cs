using CopperBend.App.Model;
using RLNET;
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

        public Coord PlayerCoords => Player.Coord;

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

        private Coord CoordInDirection(Coord start, Direction direction)
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

        public bool CanActorSeeTarget(IActor actor, Coord target)
        {
            //FINISH: one FOV and one Pathfinder per map
            FieldOfView fov = new FieldOfView(Map);
            fov.ComputeFov(actor.Coord.X, actor.Coord.Y, actor.Awareness, true);
            return fov.IsInFov(target.X, target.Y);
        }

        public void AttackPlayer(IActor actor)
        {
            //0.0
            int damage = 2;
            WriteLine($"The {actor.Name} hit me for {damage}.");
            Player.AdjustHealth(-damage);

            if (damage > 0)
                WriteLine($"Ow.  Down to {Player.Health}.");
        }

        public void HealPlayer(int amount)
        {
            Player.AdjustHealth(amount);
            WriteLine($"So nice.  Up to {Player.Health}.");
        }

        public List<Coord> GetPathTo(Coord start, Coord target)
        {
            Map.SetIsWalkable(start, true);
            Map.SetIsWalkable(target, true);

            PathFinder pathFinder = new PathFinder(Map, 1.0, Math.Sqrt(2));

            var pathList = pathFinder.ShortestPathList(start, target);

            Map.SetIsWalkable(start, false);
            Map.SetIsWalkable(target, false);

            return pathList;
        }

        public bool MoveActorTo(IActor actor, Coord step)
        {
            return Map.MoveActor(actor, step);
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

        public void PutItemOnMap(IItem item)
        {
            Map.Items.Add(item);
        }

        public void RemovePlantAt(Coord coord)
        {
            Map.Tiles[coord.X, coord.Y].RemovePlant();
        }

        public void Till(ITile tile)
        {
            tile.Till();
            tile.SetTileType(Map.TileTypes["TilledDirt"]);
        }

        public void Learn(Fruit fruit)
        {
            watcher.Learn(fruit);
        }
    }
}
