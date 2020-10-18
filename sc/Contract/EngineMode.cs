namespace CopperBend.Contract
{
    public enum EngineMode
    {
        Unset = 0,
        NoGameRunning,
        PlayerTurn,
        WorldTurns,
        MenuOpen,
        MessagesPendingUserInput,
        LargeMessagePending,
    }
}
