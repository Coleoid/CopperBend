using System;
using System.Collections.Generic;
using SadConsole.Input;

namespace CopperBend.Contract
{
    public interface IMessager
    {
        IMessageLogWindow MessageWindow { get; set; }
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
