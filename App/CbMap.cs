﻿using System.Collections.Generic;
using RLNET;
using RogueSharp;

namespace CopperBend.App
{
    public interface IcbMap : IMap
    {
        string Name { get; set; }
        Tile[,] Tiles { get; set; }
        List<Item> Items { get; set; }
        List<Actor> Actors { get; set; }


        bool SetActorPosition(Actor actor, int x, int y);
        void UpdatePlayerFieldOfView(Actor player);
        void DrawMap(RLConsole mapConsole);
    }

    public class CbMap : Map, IcbMap
    {
        public CbMap(int xWidth, int yHeight)
            : base(xWidth, yHeight)
        {
            Tiles = new Tile[xWidth, yHeight];
            Actors = new List<Actor>();
            Items = new List<Item>();
        }

        public string Name { get; set; }
        public Tile[,] Tiles { get; set; }

        public List<Item> Items { get; set; }

        public List<Actor> Actors { get; set; }

        public Tile this[int x, int y]
        {
            get => Tiles[x, y];
        }

        public bool IsTillable(int x, int y)
        {
            return Tiles[x, y].TerrainType == TerrainType.Dirt;
        }
        public bool IsTillable(Cell cell) => IsTillable(cell.X, cell.Y);

        public void DrawMap(RLConsole mapConsole)
        {
            mapConsole.Clear();
            foreach (ICell cell in GetAllCells())
            {
                DrawCell(mapConsole, cell);
            }

            foreach (var item in Items)
            {
                Draw(mapConsole, item);
            }

            foreach (var actor in Actors)
            {
                Draw(mapConsole, actor);
            }
        }

        private void DrawCell(RLConsole console, ICoord coord)
        {
            if (!IsExplored(coord.X, coord.Y)) return;  // unknown is undrawn

            var rep = Tiles[coord.X, coord.Y].repr;
            var isInFOV = IsInFov(coord.X, coord.Y);
            var fg = rep.Foreground(isInFOV);
            var bg = rep.Background(isInFOV);

            console.Set(coord.X, coord.Y, fg, bg, rep.Symbol);
        }

        private void Draw(RLConsole console, IDrawable thing)
        {
            if (!IsExplored(thing.X, thing.Y)) return;  // unknown is undrawn

            IDrawable show = IsInFov(thing.X, thing.Y)
                ? thing 
                : (IDrawable)Tiles[thing.X, thing.Y];
            console.Set(show.X, show.Y, show.Color, Colors.FloorBackgroundSeen, show.Symbol);
        }

        // Returns true when target cell is walkable and move succeeds
        public bool SetActorPosition(Actor actor, int x, int y)
        {
            // Only allow actor movement if the cell is walkable
            if (!GetCell(x, y).IsWalkable) return false;

            // The cell the actor was previously on is now walkable
            SetIsWalkable(actor, true);
            // Update the actor's position
            actor.X = x;
            actor.Y = y;
            // The new cell the actor is on is now not walkable
            SetIsWalkable(actor, false);
           
            // Don't forget to update the field of view if we just repositioned the player
            if (actor is Player)
            {
                UpdatePlayerFieldOfView(actor);
            }

            return true;
        }

        //  Player field of view changes whenever player moves
        //  ...more cases (shifting terrain, et c.) in the future
        public void UpdatePlayerFieldOfView(Actor player)
        {
            var fovCells = ComputeFov(player.X, player.Y, player.Awareness, true);

            foreach (Cell cell in fovCells)
            {
                SetIsExplored(cell, true);
            }
        }
    }
}
