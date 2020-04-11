using System;
using System.Collections.Generic;
using SadConsole.Input;

namespace CopperBend.Contract
{
    public interface IMessager
    {
        //Action<string> WriteLine { get; set; }
        //Action<string> Prompt { get; set; }
        //Action More { get; set; }

        Func<bool> ShouldClearQueueOnEscape { get; set; }
        void QueueInput(IReadOnlyCollection<AsciiKey> keys);
        AsciiKey GetNextKeyPress();

        bool IsInputReady();
        void ClearPendingInput();


        void WriteLineIfPlayer(IBeing being, string message);
        void WriteLine(string message);
        void Prompt(string message);
        void More();

        void ResetMessagesSentSincePause();

        void HandleLargeMessage();
        void HideLargeMessage();
    }
}
