using RLNET;

namespace CopperBend.App
{
    public interface IGameWindow
    {
        bool DisplayDirty { get; set; }
        RLConsole LargePane { get; set; }
        RLRootConsole RootConsole { get; set; }
        RLConsole TextPane { get; set; }

        void Run(UpdateEventHandler onUpdate, UpdateEventHandler onRender);
        void Render(IAreaMap map);
        void AddMessage(string newMessage);
        void EmptyInputQueue();
        RLKeyPress GetNextKeyPress();
        void HandleLargeMessage();
        void HandlePendingMessages();
        void Prompt(string text);
        void ResetWait();
        void ShowMessages();
        void WriteLine(string text);
    }
}
