using System;
using System.Collections.Generic;
using CopperBend.Contract;
using CopperBend.Model;
using GoRogue;
using Microsoft.Xna.Framework;
using SadConsole.Components;

namespace CopperBend.Fabric
{
    // All significant beings, their current state and relationships
    public class SocialRegister : IBook
    {
        public BeingCreator BeingCreator { get; }
        public Dictionary<string, IBeing> WellKnownBeings { get; }

        public SocialRegister(BeingCreator creator)
        {
            BeingCreator = creator;
            WellKnownBeings = new Dictionary<string, IBeing>();
        }


        public Being CreatePlayer()
        {
            var player = BeingCreator.CreateBeing(Color.ForestGreen, Color.Black, '@');
            player.Name = "Suvail";
            player.AddComponent(new EntityViewSyncComponent());
            player.IsPlayer = true;

            //0.2: remove these pre-equipped items
            player.AddToInventory(Equipper.BuildItem("hoe"));
            player.AddToInventory(Equipper.BuildItem("seed:Healer", 2));

            return player;
        }


        public void LoadRegister(IBeing ourHero)
        {
            AddTownsfolk("Jeromi Kandering");
            AddTownsfolk("Carol Embreya");
            AddTownsfolk("Bock Merrital");
            AddTownsfolk("Caben Merrital");
            AddTownsfolk("Runcel Longwalk");
            AddTownsfolk("Kellet Benison");
            AddTownsfolk("Gervandik");
            AddTownsfolk("Merotta Carrier");
            AddTownsfolk("Belo Freitan");
            AddTownsfolk("Kuvven Turtler");
            AddTownsfolk("Helmai Preston");
            AddTownsfolk("Matchel Dariley");
            AddTownsfolk("Pal Ketterit");
            AddTownsfolk("Kuez Bell");

            //0.1: Needs to load NPCs from file

            WellKnownBeings[ourHero.Name] = ourHero;
        }

        public void AddTownsfolk(string name)
        {
            AddBeingFromdetails(name, Color.AliceBlue, Color.Black, name[0], "Townsfolk");
        }

        public void AddBeingFromdetails(string name, Color fg, Color bg, int glyph, string beingType)
        {
            if (WellKnownBeings.ContainsKey(name))
                throw new Exception("We've already got one.");

            var b = BeingCreator.CreateBeing(fg, bg, name[0]);
            b.Name = name;
            b.BeingType = beingType;
            WellKnownBeings[name] = b;

        }

        internal IBeing FindBeing(string entityWanted)
        {
            if (WellKnownBeings.ContainsKey(entityWanted))
            {
                return WellKnownBeings[entityWanted];
            }

            return null;
        }
    }
}
