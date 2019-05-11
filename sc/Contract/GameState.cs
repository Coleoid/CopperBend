namespace CopperBend.Contract
{
    public interface IGameState
    {
        ICompoundMap Map { get; }
        IBeing Player { get; }
        //0.1:  Later, collect everything which is saved/loaded, and stateful.
    }
}
