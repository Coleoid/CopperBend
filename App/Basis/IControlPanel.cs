﻿using CopperBend.App.Model;
using CopperBend.MapUtil;
using RLNET;
using System.Collections.Generic;

//  Here I have some mechanisms to communicate and control,
//  with nothing coherent shaken out yet.  In other places
//  I'm passing important domain bits as arguments.
//  Also see the EventBus for another channel.

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

        // keep accumulating miscellaneous methods until structure becomes clear
        void AddToSchedule(ScheduleEntry entry);
        void Learn(Fruit fruit);
        void SetMapDirty();

        void WriteLine(string text);
        void Prompt(string text);

        void PutItemOnMap(IItem item);
        void RemovePlantAt(Point point);
        void Till(ITile tile);

        void Experience(PlantType plant, Exp experience);
    }

    //  State of the game, not _quite_ globals...
    public interface IGameState
    {
        IAreaMap Map { get; }
        IActor Player { get; }
        GameMode Mode { get; set; }
    }

    public class GameState : IGameState
    {
        public IAreaMap Map { get; set; }
        public IActor Player { get; set; }
        public GameMode Mode { get; set; }
    }

    public enum GameMode
    {
        Unknown = 0,
        MenuOpen,
        LargeMessagePending,
        MessagesPending,
        PlayerReady,
        Schedule,
    }

    public enum GameCommand
    {
        Unset = 0,
        Quit,
        GoToFarmhouse,  //0.1
        NotReadyToLeave,
    }

    //  Categories of experience
    public enum Exp
    {
        Unknown = 0,
        PlantSeed,
        EatFruit,
    }
}
