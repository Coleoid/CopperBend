using System;
using System.Collections.Generic;
using RLNET;

namespace CopperBend.App
{
    public interface IGameWindow
    {
        bool DisplayDirty { get; set; }
        RLConsole LargePane { get; set; }
        RLConsole TextPane { get; set; }

        void Run(UpdateEventHandler onUpdate, UpdateEventHandler onRender);
        void Render(IAreaMap map);
        void AddMessage(string newMessage);
        void EmptyInputQueue();
        //RLKeyPress GetNextKeyPress();
        void HandleLargeMessage();
        void HandlePendingMessages();
        void Prompt(string text);
        void ClearMessagePause();
        void ShowMessages();
        void WriteLine(string text);
        void ShowInventory(IEnumerable<IItem> inventory, Func<IItem, bool> filter);
        RLKeyPress GetKeyPress();
        void Close();
    }
}
