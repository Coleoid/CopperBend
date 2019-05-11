//  Some methods to communicate and control, in junk drawer mode.
//  I'll keep accumulating until structure emerges.
//  In other places I pass important domain bits as arguments.

//  Functional completeness levels:
//  0.1:  Works in a limited way, with lame code
//  0.2:  Code less lame, but either incomplete or awkward for player
//  0.K:  Not expecting more needed before initial release
//  0.5:  Quality beyond initial release needs


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
