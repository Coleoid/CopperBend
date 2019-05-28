namespace CopperBend.Contract
{
    public interface IGameState
    {
        ICompoundMap Map { get; }
        IBeing Player { get; }
        //0.1 should grow to be the repo of everything needed to save/load
    }
}
