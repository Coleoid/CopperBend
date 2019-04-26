using CopperBend.Engine;
using CopperBend.Fabric;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CopperBend.Contract
{
    public interface IMap
    {
        int Width { get; }
        int Height { get; }

        void Initialize(int width, int height);

        bool IsTransparent(int x, int y);
        bool IsTransparent(Point point);
        void SetTransparent(Point point, bool isTransparent);

        bool IsWalkable(Point point);
        void SetWalkable(Point point, bool isWalkable);

        bool IsExplored(Point point);
        void SetExplored(Point point, bool isExplored);

        bool IsWithinMap(Point point);

        /// <summary> Sets whole Map to be transparent and walkable </summary>
        void Clear();

        /// <summary> Sets the whole Map to the specified values </summary>
        void Clear(bool isTransparent, bool isWalkable);

        //IMap Clone();

        /// <summary>
        /// Overwrites the cells of this map with the source Map, at the specified offset
        /// </summary>
        void Copy(IMap sourceMap, Point offset);

        ReadOnlyCollection<Cell> ComputeFov(Point origin, int radius, bool lightWalls);

        ReadOnlyCollection<Cell> AppendFov(Point origin, int radius, bool lightWalls);

        IEnumerable<Cell> GetAllCells();
        IEnumerable<Point> GetAllPoints();

        IEnumerable<Cell> GetCellsAlongLine(Point origin, Point destination);

        IEnumerable<Point> GetPointsInSquare(Point center, int distance);

        IEnumerable<Cell> GetBorderCellsInDiamond(Point center, int distance);

        IEnumerable<Cell> GetBorderCellsInSquare(Point center, int distance);

        string ToString(bool useFov);

        Cell GetCell(int index);
        Cell GetCell(int x, int y);
        Cell GetCell(Point point);

        int GetIndex(int x, int y);
        int GetIndex(Point cell);

        Point GetPoint(int index);
    }

    public class MapState
    {
        /// <summary>
        /// Flags Enumeration of the possible properties for any Cell in the Map
        /// </summary>
        [Flags]
        public enum CellProperties
        {
            /// <summary>
            /// Not set
            /// </summary>
            None = 0,
            /// <summary>
            /// A character could normally walk across the Cell without difficulty
            /// </summary>
            Walkable = 1,
            /// <summary>
            /// There is a clear line-of-sight through this Cell
            /// </summary>
            Transparent = 2,
            /// <summary>
            /// The Cell is in the currently observable field-of-view
            /// </summary>
            Visible = 4,
            /// <summary>
            /// The Cell has been in the field-of-view in the player at some point during the game
            /// </summary>
            Explored = 8
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public CellProperties[] Cells
        {
            get; set;
        }
    }
}
