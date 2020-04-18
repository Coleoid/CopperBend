using System;
using System.Collections.Generic;
using CopperBend.Contract;
using log4net;

namespace CopperBend.Logic
{
    public class ModeNode : IGameMode, IPanelService
    {
        private readonly ILog log;

        public ModeNode(ILog logger)
        {
            log = logger;
            modeStack = new Stack<EngineMode>();
            callbackStack = new Stack<Action>();

        }

        public void RegisterWithPanel(IServicePanel isp)
        {
            // Current idea is the logger lives on through new game
            // boundaries, so may not care about any events.
        }

        //  We can stack modes of the game to any level.
        //  Say the schedule reaches the player, who enters a command
        //  to inspect their quest/job list, the stack is:
        //    Large, Input, Schedule, Start
        //  Each mode on the stack has its own callback, so if we then
        //  look into details of a quest, the states could be:
        //    Large, Large, Input, Schedule, Start
        //  ...and we can later leave the quest details without confusion
        //  about what we're doing.

        private Stack<EngineMode> modeStack;
        private Stack<Action> callbackStack;

        public EngineMode CurrentMode { get; set; } = EngineMode.Unknown;

        public Action CurrentCallback { get; set; } = () => { };

        public void PushEngineMode(EngineMode newMode, Action callback)
        {
            if (newMode == EngineMode.Unknown)
                throw new Exception($"Should never EnterMode({newMode}).");

            var oldMode = CurrentMode;
            modeStack.Push(CurrentMode);
            CurrentMode = newMode;
            callbackStack.Push(CurrentCallback);
            CurrentCallback = callback;

            // fires when restarting game 12 nov 19
            //if (oldMode == CurrentMode)
            //    if (!Debugger.IsAttached) Debugger.Launch();

            // Don't log mode shifts from world's turn to player's turn.
            if (oldMode == EngineMode.WorldTurns && CurrentMode == EngineMode.PlayerTurn) return;

            log.Debug($"Enter mode {CurrentMode}, push down mode {oldMode}.");
        }

        public void PopEngineMode()
        {
            var oldMode = CurrentMode;
            CurrentMode = modeStack.Pop();
            CurrentCallback = callbackStack.Pop();

            // Don't log mode shifts from player's turn to world's turn.
            if (oldMode == EngineMode.PlayerTurn && CurrentMode == EngineMode.WorldTurns) return;

            log.Debug($"Pop mode {oldMode} off, enter mode {CurrentMode}.");
        }
    }
}
