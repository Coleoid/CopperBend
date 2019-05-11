using System;
using System.Collections.Generic;
using GoRogue;
using CopperBend.Fabric;

//  Some methods to communicate and control, in junk drawer mode.
//  I'll keep accumulating until structure emerges.
//  In other places I pass important domain bits as arguments.

//  Functional completeness levels:
//  0.1:  Works in a limited way, with lame code
//  0.2:  Code less lame, but either incomplete or awkward for player
//  0.K:  Not expecting more needed before initial release
//  0.5:  Quality beyond initial release needs


namespace CopperBend.Contract
{
    public interface IControlPanel
    {
        void ScheduleAgent(IScheduleAgent agent, int tickOff);

        bool CanActorSeeTarget(IBeing being, Coord target);
        List<Coord> GetPathTo(Coord start, Coord target);

        //void Learn(Fruit fruit);
        //void SetMapDirty();

        void PutItemOnMap(IItem item);
        void RemovePlantAt(Coord position);
        void Till(Space space);

        void AddExperience(PlantType plant, Exp experience);

        bool CommandBeing(IBeing being, Command command);

        bool PlayerMoved { get; set; }
        Action<EngineMode, Func<bool>> PushEngineMode { get; }
        Action ClearPendingInput { get; }
    }

    //0.1:  Categories of experience
    public enum Exp
    {
        Unknown = 0,
        PlantSeed,
        EatFruit,
    }

    public interface ILogWindow
    {
        void Add(string message);
    }
}
