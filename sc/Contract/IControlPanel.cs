using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

//  Some methods to communicate and control, in junk drawer mode.
//  I'll keep accumulating until structure emerges.
//  In other places I pass important domain bits as arguments.
//  See the EventBus for another channel.

namespace CopperBend.Contract
{
    public interface IControlPanel
    {
        void ScheduleActor(IActor actor, int tickOff);

        void AttackPlayer(IActor actor);

        bool CanActorSeeTarget(IActor actor, Point target);
        List<Point> GetPathTo(Point start, Point target);

        void AddToSchedule(ICanAct actor, int offset);
        //void Learn(Fruit fruit);
        void SetMapDirty();

        void PutItemOnMap(IItem item);
        void RemovePlantAt(Point point);
        void Till(ITile tile);

        void Experience(PlantType plant, Exp experience);
        void EnterMode(object sender, EngineMode mode, Func<bool> callback);

        bool CommandActor(IActor actor, Command command);
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
