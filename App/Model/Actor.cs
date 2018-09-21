using System.Collections.Generic;
using RLNET;
using RogueSharp;

namespace CopperBend.App.Model
{
    public class Actor : IActor
    {

        public Actor(int x, int y)
        {
            X = x;
            Y = y;
            Inventory = new List<IItem>();
            Health = 6;
        }

        public int Health { get; protected set; }

        //  IActor
        public string Name { get; set; }
        public int Awareness { get; set; }
        public List<IItem> Inventory { get; private set; }

        public void Damage(int amount)
        {
            Health -= amount;
        }

        //  IDrawable
        public RLColor Color { get; set; }
        public char Symbol { get; set; }
        public void MoveTo(int x, int y)
        {
            X = x;
            Y = y;
        }

        //  ICoord
        public int X { get; protected set; }
        public int Y { get; protected set; }
    }
}
