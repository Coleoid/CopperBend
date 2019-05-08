using CopperBend.Contract;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using SadConsole.Input;
using CopperBend.Model;
using Direction = CopperBend.Contract.Direction;

namespace CopperBend.Engine
{
    public partial class CommandDispatcher : IControlPanel
    {
        private const int lowercase_a = 97;
        private const int lowercase_z = 123;

        private int AlphaIndexOfKeyPress(AsciiKey key)
        {
            //if (!key.Character) return -1;
            var asciiNum = (int)key.Character;
            if (asciiNum < lowercase_a || lowercase_z < asciiNum) return -1;
            return asciiNum - lowercase_a;
        }

        public void ScheduleAgent(IScheduleAgent agent, int tickOff)
        {
            Schedule.AddAgent(agent, tickOff);
        }

        private Point PointInDirection(Point start, CmdDirection direction)
        {
            int newX = start.X;
            int newY = start.Y;

            if ((direction & CmdDirection.North) == CmdDirection.North)
            {
                newY--;
            }

            if ((direction & CmdDirection.South) == CmdDirection.South)
            {
                newY++;
            }

            if ((direction & CmdDirection.West) == CmdDirection.West)
            {
                newX--;
            }

            if ((direction & CmdDirection.East) == CmdDirection.East)
            {
                newX++;
            }

            return new Point(newX, newY);
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

        private Direction DirectionOfKey(AsciiKey keyPress)
        {
            return
                keyPress.Key == Keys.Up ? Direction.Up :
                keyPress.Key == Keys.Down ? Direction.Down :
                keyPress.Key == Keys.Left ? Direction.Left :
                keyPress.Key == Keys.Right ? Direction.Right :

                keyPress.Key == Keys.NumPad1 ? Direction.DownLeft :
                keyPress.Key == Keys.NumPad2 ? Direction.Down :
                keyPress.Key == Keys.NumPad3 ? Direction.DownRight :
                keyPress.Key == Keys.NumPad4 ? Direction.Left :
                keyPress.Key == Keys.NumPad6 ? Direction.Right :
                keyPress.Key == Keys.NumPad7 ? Direction.UpLeft :
                keyPress.Key == Keys.NumPad8 ? Direction.Up :
                keyPress.Key == Keys.NumPad9 ? Direction.UpRight :
                Direction.None;
        }

        //public void SetMapDirty()
        //{
        //    Map.IsDisplayDirty = true;
        //}

        public bool CanActorSeeTarget(IBeing being, Point target)
        {
            //FINISH: one FOV and one Pathfinder per map
            //FieldOfView fov = new FieldOfView(Map);
            //fov.ComputeFov(actor.Point, actor.Awareness, true);
            //return fov.IsInFov(target);
            return false;
        }

        public void HealActor(IBeing being, int amount)
        {
            being.Heal(amount);
        }

        public void FeedActor(IBeing being, int amount)
        {
            //0.0
            //actor.Feed(amount);
        }

        public List<Point> GetPathTo(Point start, Point target)
        {
            //Map.SetWalkable(start, true);
            //Map.SetWalkable(target, true);

            ////IMapView<bool> view = Map.sd

            ////PathFinder pathFinder = new PathFinder(Map, 1.0, Math.Sqrt(2));
            ////GoRogue.Pathing.AStar finder = new AStar(IMapView<bool> null, Distance.EUCLIDEAN);
            //var pathList = pathFinder.ShortestPathList(start, target);

            //Map.SetWalkable(start, false);
            //Map.SetWalkable(target, false);

            //return pathList;
            throw new NotImplementedException();
        }

        public void PutItemOnMap(IItem item)
        {
            ItemMap.Add(item, item.Point);
        }

        public void RemovePlantAt(Point point)
        {
            //Map.GetTileAt(point).RemovePlant();
        }

        public void Till(Space space)
        {
            space.Till();
        }

        public void Learn(Fruit fruit)
        {
            Describer.Learn(fruit);
        }

        public int XP { get; set; } = 0;
        public void AddExperience(PlantType plant, Exp experience)
        {
            //TODO:  An entire experience subsystem.  For now it can be "points".

            XP += 20;
        }

        public void CheckActorAtCoordEvent(IBeing being, ITile tile)
        {
            //if (Map.LocationMessages.ContainsKey(tile.Point))
            //{
            //    var message = Map.LocationMessages[tile.Point];
            //    foreach (var line in message)
            //        Output.WriteLine(line);

            //    Map.LocationMessages.Remove(tile.Point);
            //}

            ////0.2
            //if (Map.LocationEventEntries.ContainsKey(tile.Point))
            //{
            //    var entries = Map.LocationEventEntries[tile.Point];
            //    foreach (var entry in entries)
            //    {
            //        //CommandQueue.Enqueue(entry.Command);
            //    }
            //}

            //0.3 may unify those collections and loops, may restructure flow
        }

        public bool PlayerMoved { get; set; }
        public Action<EngineMode, Func<bool>> PushEngineMode { get; set; }
        public Action ClearPendingInput { get; set; }
    }
}
