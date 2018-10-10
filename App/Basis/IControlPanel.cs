﻿using RLNET;
using RogueSharp;
using System.Collections.Generic;

//  Here I have some mechanisms to communicate and control,
//  with nothing coherent shaken out yet.  In other places
//  I'm passing important domain bits as arguments.

namespace CopperBend.App
{
    public interface IControlPanel
    {
        void PlayerBusyFor(int ticks);
        void RemoveFromInventory(IItem item);
        ICoord PlayerCoords { get; }

        void AttackPlayer();
        bool MoveActorTo(IActor actor, ICoord step);
        bool CanActorSeeTarget(IActor actor, ICoord target);
        List<ICoord> GetPathTo(ICoord start, ICoord target);

        // keep accumulating miscellaneous methods until structure becomes clear
        void AddToSchedule(ScheduleEntry entry);
        void SetMapDirty();

        RLKeyPress GetNextKeyPress();
        void WriteLine(string text);
        void Prompt(string text);

        void PutItemOnMap(IItem item);
        void RemovePlantAt(ICoord coord);
        void HealPlayer(int amount);

        //  This form may turn out well--specialist informs about its state
        void MessagePanelFull();
        void AllMessagesSent();
    }

    //  State of the game, not _quite_ globals...
    public interface IGameState
    {
        IAreaMap Map { get; }
        IActor Player { get; }
        GameMode Mode { get; set; }
    }

    public enum GameMode
    {
        Unknown = 0,
        MenuOpen,
        MessagesPending,
        PlayerReady,
        Schedule,
    }
}
