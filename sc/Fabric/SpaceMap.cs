﻿using System.Collections.Generic;
using GoRogue;
using CopperBend.Contract;
using CopperBend.Model;
using System;
using YamlDotNet.Serialization;

namespace CopperBend.Fabric
{
    public class SpaceMap : SpatialMap<Space>
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Coord PlayerStartPoint { get; set; }
        public static Dictionary<string, TerrainType> TerrainTypes { get; internal set; }
        public static TerrainType TilledSoil => TerrainTypes["tilled dirt"];
        public static TerrainType PlantedSoil => TerrainTypes["planted dirt"];

        public SpaceMap(int width, int height)
            : base(height * width)
        {
            Width = width;
            Height = height;
        }

        public bool CanWalkThrough(Coord position)
        {
            // off the map is not walkable
            if (position.X < 0 || position.X >= Width
             || position.Y < 0 || position.Y >= Height)
                return false;

            return GetItem(position).CanWalkThrough;
        }

        public bool CanSeeThrough(Coord position)
        {
            // off the map is not visible
            if (position.X < 0 || position.X >= Width
             || position.Y < 0 || position.Y >= Height)
                return false;

            return GetItem(position).CanSeeThrough;
        }

        public bool CanPlant(Coord position)
        {
            // off the map is not plantable
            if (position.X < 0 || position.X >= Width
             || position.Y < 0 || position.Y >= Height)
                return false;

            return GetItem(position).CanPlant;
        }


        internal void Sow(Space space, Seed seedToSow)
        {
            if (space.CanPlant)
            {
                space.Terrain = PlantedSoil;
                space.IsSown = true;
            }
        }

        internal void Till(Space space)
        {
            if (space.CanTill)
            {
                space.Terrain = TilledSoil;
                space.IsTilled = true;
            }
        }

        internal void SeeCoords(IEnumerable<Coord> newlySeen)
        {
            foreach (var seen in newlySeen)
            {
                GetItem(seen).IsKnown = true;
            }
        }

        internal bool OpenDoor(Space tile)
        {
            if (tile.Terrain == TerrainTypes["closed door"])
            {
                tile.Terrain = TerrainTypes["open door"];
                return true;
            }

            return false;
        }
    }

    public class Space : IHasID
    {
        public Space(uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
        }

        #region standard IHasID
        public static IDGenerator IDGenerator;
        public uint ID { get; private set; }
        #endregion

        //public int Elevation;  //for later movement/attack mod
        public TerrainType Terrain;

        [YamlIgnore]
        //0.2.MAP  check for modifiers (smoke, dust, giant creature, ...)
        public bool CanSeeThrough => Terrain.CanSeeThrough;
        [YamlIgnore]
        public bool CanWalkThrough => Terrain.CanWalkThrough;

        [YamlIgnore]
        //0.2.MAP  check for modifiers (permission, hostile aura, blight, ...)
        public bool CanPlant => Terrain.CanPlant && IsTilled && !IsSown;
        [YamlIgnore]
        public bool CanTill => Terrain.CanPlant && !IsTilled;

        public bool IsTilled { get; internal set; }
        public bool IsSown { get; internal set; }
        public bool IsKnown { get; internal set; }
    }

    public class AreaBlight : IHasID, IDestroyable
    {
        public AreaBlight(uint id = uint.MaxValue)
        {
            ID = (id == uint.MaxValue ? IDGenerator.UseID() : id);
        }

        #region standard IHasID
        public static IDGenerator IDGenerator;
        public uint ID { get; private set; }
        #endregion

        public int Extent { get; set; }

        #region IDestroyable
        public int MaxHealth => 80;

        public int Health => Extent;

        public void Heal(int amount)
        {
            Guard.Against(amount < 0, $"Cannot heal negative amount {amount}.");
            Extent = Math.Min(MaxHealth, Extent + amount);
        }

        public void Hurt(int amount)
        {
            Guard.Against(amount < 0, $"Cannot hurt negative amount {amount}.");
            Extent = Math.Max(0, Extent - amount);
        }
        #endregion
    }

}
