using System;
using System.Collections.Generic;
using SadConsole.Input;
using Coord = GoRogue.Coord;
using CopperBend.Contract;

namespace CopperBend.Logic
{
    /// <summary> Holds utility behaviors to reduce clutter in logic file. </summary>
    public partial class CommandDispatcher : IControlPanel
    {
        public void ScheduleAgent(IScheduleAgent agent, int tickOff)
        {
            Schedule.AddAgent(agent, tickOff);
        }

        public Coord CoordInDirection(Coord start, CmdDirection direction)
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

        public void HealBeing(IBeing being, int amount)
        {
            being.Heal(amount);
        }

        public void FeedBeing(IBeing being, int amount)
        {
            //being.Feed(amount); //0.0
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

        public void PutItemOnMap(IItem item, Coord coord)
        {
            ItemMap.Add(item, coord);
        }

        public void RemovePlantAt(Coord position)
        {
            //Map.GetTileAt(point).RemovePlant();
        }

        public bool RemoveFromAppropriateMap(IDelible mote)
        {
            // remove from whichever map
            if (mote is IAreaRot rot)
            {
                RotMap.Remove(rot);
                return true;
            }

            if (mote is IBeing being)
            {
                return BeingMap.Remove(being);
            }

            //1.+:  Make IItems IDestroyable
            if (mote is IItem item)
            {
                return ItemMap.Remove(item);
            }

            throw new Exception($"Unsure how to destroy a {mote.GetType().Name}.");
        }

        public void RemoveFromSchedule(IScheduleAgent agent)
        {
            Schedule.RemoveAgent(agent);
        }


        public void Till(ISpace space)
        {
            SpaceMap.Till(space);
        }

        public Dictionary<XPType, int> XP { get; } = new Dictionary<XPType, int>();
        public void AddExperience(uint plantID, XPType experience)
        {
            if (!XP.ContainsKey(experience))
                XP[experience] = 0;

            //0.1.XP  vary XP gain based on action, fix args
            XP[experience] += 20;
        }

        public void CheckActorAtCoordEvent(IBeing being, Coord position)
        {
            ////0.1.MAP  fix load of map-based events

            //if (Map.LocationMessages.ContainsKey(tile.Point))
            //{
            //    var message = Map.LocationMessages[tile.Point];
            //    foreach (var line in message)
            //        Output.WriteLine(line);

            //    Map.LocationMessages.Remove(tile.Point);
            //}

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
    }
}
