﻿using RogueSharp;

namespace CopperBend.App
{
    public interface IItem : IDrawable
    {
        string Name { get; }
        int Quantity { get; set; }
        Coord Coord { get; }
        void MoveTo(Coord coord);

        bool IsUsable { get; }
        void ApplyTo(ITile tile, IControlPanel controls);

        bool IsConsumable { get; }
        string ConsumeVerb { get; }
        void Consumed(IControlPanel controls);

        bool SameThingAs(IItem item);
    }

    public interface ISeed : IItem
    { }

    public enum SeedType
    {
        Boomer,
        Healer,
        Thornfriend,
    }
}
