using System;
using GoRogue;
using CopperBend.Fabric;
using SadConsole.Input;

//  Functional completeness levels:
//  0.1  ---  The code works in a limited way, and is lame
//  0.2  ---  Code is less lame, yet not ready for initial release
//  0.K  ---  Ready for initial release
//  0.+  ---  Quality beyond initial release needs, guess I was inspired

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

        Coord CoordInDirection(Coord start, CmdDirection direction);
        void PutItemOnMap(Item item);
        void RemovePlantAt(Coord position);
        void Till(Space space);

        void AddExperience(uint plantID, Exp experience);
        bool PlayerMoved { get; set; }

        //bool CanActorSeeTarget(IBeing being, Coord target);
        //List<Coord> GetPathTo(Coord start, Coord target);
        //void Learn(Fruit fruit);
        //void SetMapDirty();

        //  This approach works well.
        //  Now nobody touches the engine, where these details originate.
        //  Events/subscriptions also worked, but the defining advantage
        // of events is providing multiple subscribers, which we didn't
        // need, so the (significant) coding overhead was waste.
        //0.2  group in interface, have ControlPanel delegate to another implementing obj?
        Func<bool> IsInputReady { get; }
        Func<AsciiKey> GetNextInput { get; }
        Action ClearPendingInput { get; }

        //0.2  group in interface, have ControlPanel delegate to another implementing obj?
        Action<string> WriteLine { get; }
        Action<IBeing, string> WriteLineIfPlayer { get; }
        Action<string> Prompt { get; }
    }

    //0.1.XP  extend categories
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
