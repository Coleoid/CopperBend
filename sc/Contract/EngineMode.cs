namespace CopperBend.Contract
{
    public enum EngineMode
    {
        Unknown = 0,
        NoGameRunning,
        MenuOpen,
        LargeMessagePending,
        MessagesPendingUserInput,
        InputBound,
        Schedule,
        Pause,
    }
}
