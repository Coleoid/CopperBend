using GoRogue;

//  Functional completeness levels:
//  0.0  ---  Code is needed, and doesn't exist or work at all
//  0.1  ---  Placeholder code that doesn't completely fail
//  0.2  ---  Code is less lame, yet not ready for initial release
//  0.K  ---  Ready for initial release
//  1.+  ---  Speculative thought for after initial release
//  +.+  ---  Quality beyond initial release needs, guess I was inspired

namespace CopperBend.Contract
{
    //  Some methods to communicate and control, in junk drawer mode.
    //  I'll keep accumulating until structure emerges.
    //  In other places I pass important domain bits as arguments.
    public interface IControlPanel
    {
        /// <summary> The main purpose of the CommandDispatcher. </summary>
        bool CommandBeing(IBeing being, Command command);

        /// <summary> When an agent will next get a turn. </summary>
        void ScheduleAgent(IScheduleAgent agent, int tickOff);

        IGameState GameState { get; set; }

        Coord CoordInDirection(Coord start, CmdDirection direction);
        void PutItemOnMap(IItem item, Coord coord);
        void RemovePlantAt(Coord position);
        bool RemoveFromAppropriateMap(IDelible mote);
        void RemoveFromSchedule(IScheduleAgent agent);
        void Till(ISpace space);

        void AddExperience(uint plantID, XPType experience);
        bool PlayerMoved { get; set; }
        IAttackSystem AttackSystem { get; }

        void Dispatch(ScheduleEntry nextAction);

        //bool CanActorSeeTarget(IBeing being, Coord target);
        //List<Coord> GetPathTo(Coord start, Coord target);
    }

    //0.1.XP  extend categories
    public enum XPType
    {
        Unknown = 0,
        PlantSeed,
        EatFruit,
        Rot,
    }

    public interface ILogWindow
    {
        void Add(string message);
    }
}
