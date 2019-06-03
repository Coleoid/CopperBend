namespace CopperBend.Contract
{
    public interface IGameState
    {
        ICompoundMap Map { get; }
        IBeing Player { get; }
        //0.2.SAVE  include RNGs
        //0.2.SAVE  include Schedule
        //0.2.SAVE  include game mode and callback (stacks...)
        //0.2.SAVE  include beings and items
        //0.2.SAVE  watch player for new parts to save
        //0.2.SAVE  include player knowledge repos
    }
}
