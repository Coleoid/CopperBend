using System.Collections.Generic;
using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public interface IAreaMap : IMap
    {
        string Name { get; set; }
        ITile[,] Tiles { get; set; }
        List<IItem> Items { get; set; }
        List<IActor> Actors { get; set; }
        IActor ViewpointActor { get; set; }
        bool DisplayDirty { get; set; }

        ITile this[ICoord coord] { get; }

        bool SetActorPosition(IActor actor, int x, int y);
        bool SetActorCoord(IActor player, ICoord coord);
        void UpdatePlayerFieldOfView(IActor player);
        void DrawMap(RLConsole mapConsole);
        IActor GetActorAtPosition(int newX, int newY);
        IActor GetActorAtCoord(ICoord coord);
        List<string> FirstSightMessages { get; set; }
        Dictionary<(int, int), List<string>> LocationMessages { get; }
    }
}
