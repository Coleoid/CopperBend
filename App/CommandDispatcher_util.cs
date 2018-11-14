using CopperBend.App.Model;
using CopperBend.MapUtil;
using RLNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CopperBend.App
{
    public partial class CommandDispatcher : IControlPanel
    {
        private const int lowercase_a = 97;
        private const int lowercase_z = 123;

        public void WriteLine(string text) => Messenger.WriteLine(text);

        public void Prompt(string text) => Messenger.Prompt(text);

        private int AlphaIndexOfKeyPress(RLKeyPress key)
        {
            if (!key.Char.HasValue) return -1;
            var asciiNum = (int)key.Char.Value;
            if (asciiNum < lowercase_a || lowercase_z < asciiNum) return -1;
            return asciiNum - lowercase_a;
        }

        private bool IsPlayerScheduled = false;

        public Point PlayerPoint => Player.Point;

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

        private Point PointInDirection(Point start, Direction direction)
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

            return new Point(newX, newY);
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

        public bool CanActorSeeTarget(IActor actor, Point target)
        {
            //FINISH: one FOV and one Pathfinder per map
            FieldOfView fov = new FieldOfView(Map);
            fov.ComputeFov(actor.Point, actor.Awareness, true);
            return fov.IsInFov(target);
        }

        public void AttackPlayer(IActor actor)
        {
            //0.1
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

        public List<Point> GetPathTo(Point start, Point target)
        {
            Map.SetIsWalkable(start, true);
            Map.SetIsWalkable(target, true);

            PathFinder pathFinder = new PathFinder(Map, 1.0, Math.Sqrt(2));

            var pathList = pathFinder.ShortestPathList(start, target);

            Map.SetIsWalkable(start, false);
            Map.SetIsWalkable(target, false);

            return pathList;
        }

        public bool MoveActorTo(IActor actor, Point step)
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

        public void RemovePlantAt(Point point)
        {
            Map.Tiles[point.X, point.Y].RemovePlant();
        }

        public void Till(ITile tile)
        {
            tile.Till();
            tile.SetTileType(Map.TileTypes["TilledDirt"]);
        }

        public void Learn(Fruit fruit)
        {
            describer.Learn(fruit);
        }

        //  Pure plumbing -- this manifold should eliminate much other plumbing
        public void QueueCommand(GameCommand command) => GameState.QueueCommand(command);

        public int XP { get; set; } = 0;
        public void Experience(PlantType plant, Exp experience)
        {
            //TODO:  An entire experience subsystem.  For now it can be "points".

            XP += 20;
        }
    }
}
