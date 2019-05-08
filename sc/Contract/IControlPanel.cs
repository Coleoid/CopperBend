using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

//  Some methods to communicate and control, in junk drawer mode.
//  I'll keep accumulating until structure emerges.
//  In other places I pass important domain bits as arguments.

namespace CopperBend.Contract
{
    public interface IControlPanel
    {
        void ScheduleAgent(IScheduleAgent agent, int tickOff);

        bool CanActorSeeTarget(IBeing being, Point target);
        List<Point> GetPathTo(Point start, Point target);

        //void Learn(Fruit fruit);
        //void SetMapDirty();

        void PutItemOnMap(IItem item);
        void RemovePlantAt(Point point);
        void Till(Space space);

        void AddExperience(PlantType plant, Exp experience);

        bool CommandBeing(IBeing being, Command command);

        bool PlayerMoved { get; set; }
        Action<EngineMode, Func<bool>> PushEngineMode { get; }
        Action ClearPendingInput { get; }
    }

    public enum EngineMode
    {
        Unknown = 0,
        StartUp,
        MenuOpen,
        LargeMessagePending,
        MessagesPending,
        InputBound,
        Schedule,
        Pause,
    }

    //  Categories of experience
    public enum Exp
    {
        Unknown = 0,
        PlantSeed,
        EatFruit,
    }
}
