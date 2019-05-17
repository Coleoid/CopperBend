using System;
using System.Collections.Generic;
using SadConsole.Input;
using Coord = GoRogue.Coord;
using CopperBend.Contract;
using CopperBend.Model;
using CopperBend.Fabric;

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

        private Coord CoordInDirection(Coord start, CmdDirection direction)
        {
            int newX = start.X;
            int newY = start.Y;

            if (direction.HasFlag(CmdDirection.North)) newY--;
            if (direction.HasFlag(CmdDirection.South)) newY++;
            if (direction.HasFlag(CmdDirection.West))  newX--;
            if (direction.HasFlag(CmdDirection.East))  newX++;

            return (newX, newY);
        }

        public bool CanActorSeeTarget(IBeing being, Coord target)
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

        public List<Coord> GetPathTo(Coord start, Coord target)
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
            ItemMap.Add(item, item.Location);
        }

        public void RemovePlantAt(Coord position)
        {
            //Map.GetTileAt(point).RemovePlant();
        }

        public void Till(Space space)
        {
            SpaceMap.Till(space);
        }

        public void Learn(Fruit fruit)
        {
            Describer.Learn(fruit);
        }

        public int XP { get; set; } = 0;
        public void AddExperience(uint plantID, Exp experience)
        {
            //TODO:  An entire experience subsystem.  For now it can be "points".

            XP += 20;
        }

        public void CheckActorAtCoordEvent(IBeing being, Coord position)
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

        public Func<bool> IsInputReady { get; set; }
        public Func<AsciiKey> GetNextInput { get; set; }
        public Action ClearPendingInput { get; set; }
        public Action<string> AddMessage { get; set; }
    }
}
