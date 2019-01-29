using CopperBend.App.Model;
using CopperBend.MapUtil;
using RLNET;
using System;
using System.Collections.Generic;

namespace CopperBend.App
{
    public partial class CommandDispatcher : IControlPanel
    {
        private const int lowercase_a = 97;
        private const int lowercase_z = 123;

        public void WriteLine(string text) => Window.WriteLine(text);

        public void Prompt(string text) => Window.Prompt(text);

        private int AlphaIndexOfKeyPress(RLKeyPress key)
        {
            if (!key.Char.HasValue) return -1;
            var asciiNum = (int)key.Char.Value;
            if (asciiNum < lowercase_a || lowercase_z < asciiNum) return -1;
            return asciiNum - lowercase_a;
        }

        public Point PlayerPoint => Player.Point;


        //>>> elim
        public void PlayerBusyFor(int ticks)
        {
            //Schedule.Add(Player, ticks);
            //GameState.Mode = GameMode.Schedule;
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

        public void AddToSchedule(IActor actor, int offset)
        {
            Schedule.Add(actor.NextAction, offset);
        }

        public void SetMapDirty()
        {
            Map.IsDisplayDirty = true;
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
            Player.Hurt(-damage);

            if (damage > 0)
                WriteLine($"Ow.  Down to {Player.Health}.");
        }

        public void HealPlayer(int amount)
        {
            Player.Hurt(amount);
            WriteLine($"So nice.  Up to {Player.Health}.");
        }

        public List<Point> GetPathTo(Point start, Point target)
        {
            Map.SetWalkable(start, true);
            Map.SetWalkable(target, true);

            PathFinder pathFinder = new PathFinder(Map, 1.0, Math.Sqrt(2));

            var pathList = pathFinder.ShortestPathList(start, target);

            Map.SetWalkable(start, false);
            Map.SetWalkable(target, false);

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
            tile.SetTileType(Map.TileTypes["tilled dirt"]);
        }

        public void Learn(Fruit fruit)
        {
            Describer.Learn(fruit);
        }

        public int XP { get; set; } = 0;
        public void Experience(PlantType plant, Exp experience)
        {
            //TODO:  An entire experience subsystem.  For now it can be "points".

            XP += 20;
        }

        public void AddToSchedule(ICanAct actor, int offset)
        {
            throw new NotImplementedException();
        }
    }
}
