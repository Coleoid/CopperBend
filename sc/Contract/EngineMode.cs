namespace CopperBend.Contract
{
    public enum EngineMode
    {
        Unknown = 0,
        StartUp,
        MenuOpen,
        LargeMessagePending,
        MessagesPending,
        InputBound,
        Schedule,
        Pause,
    }
}
