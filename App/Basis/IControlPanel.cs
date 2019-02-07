using CopperBend.App.Model;
using CopperBend.MapUtil;
using System.Collections.Generic;

//  Some methods to communicate and control, in junk drawer mode.
//  I'll keep accumulating until structure emerges.
//  In other places I pass important domain bits as arguments.
//  See the EventBus for another channel.

namespace CopperBend.App
{
    public interface IControlPanel
    {
        void PlayerBusyFor(int ticks);
        Point PlayerPoint { get; }

        void AttackPlayer(IActor actor);
        void HealPlayer(int amount);

        void RemoveFromInventory(IItem item);
        void GiveToPlayer(IItem item);

        bool MoveActorTo(IActor actor, Point step);
        bool CanActorSeeTarget(IActor actor, Point target);
        List<Point> GetPathTo(Point start, Point target);

        void AddToSchedule(ICanAct actor, int offset);
        void Learn(Fruit fruit);
        void SetMapDirty();

        void WriteLine(string text);
        void Prompt(string text);

        void PutItemOnMap(IItem item);
        void RemovePlantAt(Point point);
        void Till(ITile tile);

        void Experience(PlantType plant, Exp experience);
        void EnterMode(object sender, EngineMode mode);
    }

    public enum EngineMode
    {
        Unknown = 0,
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
