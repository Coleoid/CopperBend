using CopperBend.App.Model;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//  Here I have some mechanisms to communicate and control,
//  with nothing coherent shaken out yet.  In other places
//  I'm passing important domain bits as arguments.

//  One thing currently lacking:  IItem.ApplyTo needs to
//  put events in the schedule.  I think we can go toward
//  such a method being in the ControlPanel, rather than
//  passing the Scheduler.

namespace CopperBend.App.Basis
{
    public interface IControlPanel
    {
        void WriteLine(string text);
        void Prompt(string text);
        void PlayerBusyFor(int ticks);

        // keep accumulating miscellaneous methods until structure becomes clear
        void AddToSchedule(ScheduleEntry entry);
        bool IsPlayerInFOV(IActor actor);
        void SetMapDirty();
        void SwitchGameToMode(GameMode mode);
        List<ICoord> GetPathTo(ICoord start, ICoord target);
        ICoord PlayerCoords { get; }

        void AttackPlayer();
        bool MoveActorTo(IActor actor, ICoord step);
        void RemoveFromInventory(IItem item);
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
