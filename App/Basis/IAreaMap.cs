using System.Collections.Generic;
using RLNET;
using RogueSharp;
using CopperBend.App.Model;

namespace CopperBend.App
{
     public interface IAreaMap : IMap
    {
        string Name { get; set; }
        ITile[,] Tiles { get; set; }
        List<IItem> Items { get; set; }
        List<IActor> Actors { get; set; }
        bool DisplayDirty { get; set; }

        ITile this[ICoord coord] { get; }
        //ITile this[int x, int y] { get; }

        bool SetActorPosition(IActor actor, int x, int y);
        bool SetActorCoord(IActor player, ICoord coord);
        void UpdatePlayerFieldOfView(IActor player);
        void DrawMap(RLConsole mapConsole);
        IActor ActorAtLocation(int newX, int newY);
        IActor ActorAtCoord(ICoord coord);
    }
}
