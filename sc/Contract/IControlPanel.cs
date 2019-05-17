using System;
using GoRogue;
using CopperBend.Fabric;
using SadConsole.Input;

//  Functional completeness levels:
//  0.1:  Works in a limited way, with lame code
//  0.2:  Code less lame, but either incomplete or awkward for player
//  0.K:  Not expecting more needed before initial release
//  0.+:  Quality beyond initial release needs

namespace CopperBend.Contract
{
    //  Some methods to communicate and control, in junk drawer mode.
    //  I'll keep accumulating until structure emerges.
    //  In other places I pass important domain bits as arguments.
    public interface IControlPanel
    {
        /// <summary> The main purpose of the CommandDispatcher. </summary>
        bool CommandBeing(IBeing being, Command command);

        /// <summary> When an agent will decide what to do when its turn comes. </summary>
        void ScheduleAgent(IScheduleAgent agent, int tickOff);

        /// <summary> When an action interrupts the entire game, in some way. </summary>
        Action<EngineMode, Func<bool>> PushEngineMode { get; }

        void PutItemOnMap(IItem item);
        void RemovePlantAt(Coord position);
        void Till(Space space);

        void AddExperience(uint plantID, Exp experience);
        bool PlayerMoved { get; set; }

        //bool CanActorSeeTarget(IBeing being, Coord target);
        //List<Coord> GetPathTo(Coord start, Coord target);
        //void Learn(Fruit fruit);
        //void SetMapDirty();

        //  This approach works well.
        //  Nobody touches the engine, where these details originate.
        //  Event bus + subscriptions also worked, but the key upside of
        // events is multiple subscribers, which was unneeded, so the 
        // (significant) overhead was waste.
        Func<bool> IsInputReady { get; }
        Func<AsciiKey> GetNextInput { get; }
        Action ClearPendingInput { get; }
        Action<string> AddMessage { get; }
    }

    //0.1:  Categories of experience
    public enum Exp
    {
        Unknown = 0,
        PlantSeed,
        EatFruit,
        Blight
    }

    public interface ILogWindow
    {
        void Add(string message);
    }
}
