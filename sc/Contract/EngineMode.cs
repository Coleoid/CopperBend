namespace CopperBend.Contract
{
    public enum EngineMode
    {
        Unknown = 0,
        NoGameRunning,
        PlayerTurn,
        WorldTurns,
        MenuOpen,
        MessagesPendingUserInput,
        LargeMessagePending,
    }
}
