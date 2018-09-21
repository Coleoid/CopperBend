using System.Collections.Generic;
using RogueSharp;

namespace CopperBend.App
{
    public interface IActor : IDrawable, ICoord
    {
        string Name { get; set; }
        int Awareness { get; set; }

        List<IItem> Inventory { get; }
        int Health { get; }

        void Damage(int v);
    }
}
