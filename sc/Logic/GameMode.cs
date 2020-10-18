using System;
using System.Collections.Generic;
using CopperBend.Contract;
using log4net;

namespace CopperBend.Logic
{
    /// <summary> Holds and synchronizes current mode and the function
    /// callback executed when it's time for that mode to work </summary>
    public class GameMode : IGameMode
    {
        [InjectProperty] private ILog Log { get; set; }

        //  To allow modes of the game to stack deeply.
        //  Say the schedule reaches the player, who enters a command
        //  to inspect their quest/job list, the stack is:
        //    Large (callback quest list) | Player Turn
        //  Each mode on the stack has its own callback, so if we then
        //  look into details of a quest, the states could be:
        //    Large (callback quest list) | Player Turn
        //    Large (callback quest detail) | Large (callback quest list) | Player Turn
        //  ...with the details screen 
        //  ...and we can later leave the quest details without confusion
        //  about what we're doing.

        private readonly Stack<EngineMode> modeStack;
        private readonly Stack<Action> callbackStack;

        public EngineMode CurrentMode => modeStack.Peek();
        public Action CurrentCallback => callbackStack.Peek();

        public GameMode()
        {
            modeStack = new Stack<EngineMode>(new[] { EngineMode.Unset });
            callbackStack = new Stack<Action>(new Action[] { () => { } });
        }

        public void PushEngineMode(EngineMode newMode, Action newCallback)
        {
            if (newMode == EngineMode.Unset)
                throw new ArgumentOutOfRangeException(nameof(newMode), "Should never shift engine to an unset mode.");

            var oldMode = CurrentMode;
            modeStack.Push(newMode);
            callbackStack.Push(newCallback);

            // Don't log mode shifts from world's turn to player's turn.
            if (oldMode == EngineMode.WorldTurns && newMode == EngineMode.PlayerTurn) return;

            Log.Debug($"Enter mode {newMode}, push down mode {oldMode}.");
        }

        public void PopEngineMode()
        {
            var oldMode = CurrentMode;
            var newMode = modeStack.Pop();
            callbackStack.Pop();

            // Don't log mode shifts from player's turn to world's turn.
            if (oldMode == EngineMode.PlayerTurn && newMode == EngineMode.WorldTurns) return;

            Log.Debug($"Pop mode {oldMode} off, enter mode {newMode}.");
        }
    }
}
