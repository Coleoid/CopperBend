using SadConsole;
using Microsoft.Xna.Framework;
using CopperBend.Contract;
using System;

namespace CopperBend.Model
{
    public class Plant : IScheduleAgent
    {
        public ScheduleEntry GetNextEntry()
        {
            throw new NotImplementedException();
        }

        public ScheduleEntry GetNextEntry(int offset)
        {
            throw new NotImplementedException();
        }
    }

    public class GrowingPlant : IScheduleAgent
    {
        public PlantType PlantType { get; protected set; }
        public GrowingPlant(Seed seed, ISchedule schedule)
        {
            PlantType = seed.PlantType;
            schedule.AddAgent(this);
        }

        public ScheduleEntry GetNextEntry()
        {
            return GetNextEntry(88); // this looks less and less relevant
        }

        public ScheduleEntry GetNextEntry(int offset)
        {
            return new ScheduleEntry
            {
                Action = (cp) => throw new Exception($"Make code to grow plant {PlantType}!"),
                Offset = offset
            };
        }
    }

    //public class Terrain_spec
    //{
    //    public Point Location;
    //    public TileBase Tile { get; set; }

    //    public bool CanSeeThrough => Tile.AllowsLOS;
    //    public bool CanMoveThrough => Tile.AllowsMove;

    //    public bool CanTill;
    //    public bool IsTilled;

    //    public bool CanPlant => IsTilled;
    //    public void Plant(Seed seed, ISchedule schedule)
    //    {
    //        bool dominionAllows = true; //0.2.  Later, land ownership constrains magic plants

    //        if (dominionAllows && !IsPlanted)
    //        {
    //            GrowingPlant = new GrowingPlant(seed, schedule);
    //        }
    //    }
    //    public bool IsPlanted => GrowingPlant != null || GrownPlant != null;
    //    public IScheduleAgent GrowingPlant;  //nameh
    //    public Plant GrownPlant;
    //}

    //public abstract class TileBase : Cell
    //{
    //    public string Name;
    //    public bool AllowsLOS;
    //    public bool AllowsMove;

    //    ///<summary>Adds Name, AllowsLOS, and AllowsMove to SadConsole.Cell.</summary>
    //    public TileBase(
    //        Color foreground, Color background, int glyph, 
    //        string name = "", bool allowsLOS = true, bool allowsMove = true)
    //        : base(foreground, background, glyph)
    //    {
    //        AllowsLOS = allowsLOS;
    //        AllowsMove = allowsMove;
    //        Name = name;
    //    }

    //    public TileBase() : base()
    //    {
    //    }
    //}

    //public class TileWall : TileBase
    //{
    //    public TileWall(string name = "wall", bool allowsLOS = false, bool allowsMove = false, char glyph = '#')
    //        : base(Color.LightGray, Color.Transparent, glyph, name, allowsLOS, allowsMove)
    //    {
    //    }
    //}

    //public class TileFloor : TileBase
    //{
    //    public TileFloor(string name = "floor", bool allowsLOS = true, bool allowsMove = true, char glyph = '.')
    //        : base(Color.DarkGray, Color.Transparent, glyph, name, allowsLOS, allowsMove)
    //    {
    //    }
    //}
}
