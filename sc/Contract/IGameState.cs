using GoRogue;

namespace CopperBend.Contract
{
    public interface IGameState
    {
        ICompoundMap Map { get; }
        IBeing Player { get; }

        void DirtyCoord(Coord newPosition);
        //0.2.SAVE  include beings and items on levels
        //0.2.SAVE  Player basics:  health & energy, items, experience
        //0.2.SAVE  include player knowledge repos
        //0.2.SAVE  include game mode and callback (stacks...)
        //0.2.SAVE  include Schedule
        //0.2.SAVE  include RNGs
        //0.+.SAVE  watch player for new parts to save
        //0.+.SAVE  NPC relationships
    }
}
