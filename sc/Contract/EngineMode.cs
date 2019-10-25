namespace CopperBend.Contract
{
    public enum EngineMode
    {
        Unknown = 0,
        StartUp,
        MenuOpen,
        LargeMessagePending,
        MessagesPendingUserInput,
        InputBound,
        Schedule,
        Pause,
    }
}
