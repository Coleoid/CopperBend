﻿//  Should grow to be complete, for save/load.

namespace CopperBend.App
{
    public interface IGameState
    {
        IAreaMap Map { get; }
        IActor Player { get; }
        GameMode Mode { get; set; }
    }

    public class GameState : IGameState
    {
        public IAreaMap Map { get; set; }
        public IActor Player { get; set; }
        public GameMode Mode { get; set; }
    }
}