namespace CopperBend.Contract
{
    public enum EngineMode
    {
        Unknown = 0,
        NoGameRunning,
        MenuOpen,
        GameWorldActing,
        PlayerActing,
        MessagesPendingUserInput,
        LargeMessagePending,
    }
}
