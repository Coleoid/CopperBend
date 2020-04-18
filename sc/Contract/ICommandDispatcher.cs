using System;
using System.Collections.Generic;
using GoRogue;
using SadConsole.Input;

namespace CopperBend.Contract
{
    public interface ICommandDispatcher
    {
        IAttackSystem AttackSystem { get; set; }
        Action ClearPendingInput { get; set; }
        IGameState GameState { get; set; }
        Func<AsciiKey> GetNextInput { get; set; }
        Func<bool> IsInputReady { get; set; }
        Action More { get; set; }
        bool PlayerMoved { get; set; }
        Action PopEngineMode { get; set; }
        Action<string> Prompt { get; set; }
        Action<EngineMode, Action> PushEngineMode { get; set; }
        Action<string> WriteLine { get; set; }
        Action<IBeing, string> WriteLineIfPlayer { get; set; }
        Dictionary<XPType, int> XP { get; }

        void AddExperience(uint plantID, XPType experience);
        bool CanActorSeeTarget(IBeing being, Coord target);
        void CheckActorAtCoordEvent(IBeing being, Coord position);
        bool CommandBeing(IBeing being, Command command);
        Coord CoordInDirection(Coord start, CmdDirection direction);
        void Dispatch(ScheduleEntry nextAction);
        bool Do_Consume(IBeing being, Command command);
        void FeedBeing(IBeing being, int amount);
        List<Coord> GetPathTo(Coord start, Coord target);
        void HealBeing(IBeing being, int amount);
        void PayCost(IBeing being, IItem item, UseCost cost);
        void PutItemOnMap(IItem item, Coord coord);
        bool RemoveFromAppropriateMap(IDelible mote);
        void RemoveFromSchedule(IScheduleAgent agent);
        void RemovePlantAt(Coord position);
        void ScheduleAgent(IScheduleAgent agent, int tickOff);
        void Till(ISpace space);
    }
}
