using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CopperBend.MapUtil
{
    public interface IMap
    {
        int Width { get; }
        int Height { get; }

        void Initialize(int width, int height);

        bool IsTransparent(int x, int y);
        bool IsTransparent(Point point);
        void SetIsTransparent(Point point, bool isTransparent);

        bool IsWalkable(Point point);
        void SetIsWalkable(Point point, bool isWalkable);

        bool IsExplored(Point point);
        void SetIsExplored(Point point, bool isExplored);

        bool IsWithinMap(Point point);

        /// <summary> Sets whole Map to be transparent and walkable </summary>
        void Clear();

        /// <summary> Sets the whole Map to the specified values </summary>
        void Clear(bool isTransparent, bool isWalkable);

        IMap Clone();

        /// <summary>
        /// Copies the Cell properties of a smaller source Map into this destination Map at the specified location
        /// </summary>
        void Copy(IMap sourceMap, Point offset);

        /// <summary>
        /// Performs a field-of-view calculation with the specified parameters.
        /// Field-of-view (FOV) is basically a calculation of what is observable in the Map from a given Cell with a given light radius.
        /// Any existing field-of-view calculations will be overwritten when calling this method.
        /// </summary>
        /// <param name="xOrigin">X location of the Cell to perform the field-of-view calculation with 0 as the farthest left</param>
        /// <param name="yOrigin">Y location of the Cell to perform the field-of-view calculation with 0 as the top</param>
        /// <param name="radius">The number of Cells in which the field-of-view extends from the origin Cell. Think of this as the intensity of the light source.</param>
        /// <param name="lightWalls">True if walls should be included in the field-of-view when they are within the radius of the light source. False excludes walls even when they are within range.</param>
        /// <returns>List of Cells representing what is observable in the Map based on the specified parameters</returns>
        ReadOnlyCollection<Cell> ComputeFov(Point origin, int radius, bool lightWalls);

        /// <summary>
        /// Performs a field-of-view calculation with the specified parameters and appends it any existing field-of-view calculations.
        /// Field-of-view (FOV) is basically a calculation of what is observable in the Map from a given Cell with a given light radius.
        /// </summary>
        /// <example>
        /// When a character is holding a light source in a large area that also has several other sources of light such as torches along the walls
        /// ComputeFov could first be called for the character and then AppendFov could be called for each torch to give us the final combined FOV given all the light sources
        /// </example>
        /// <param name="xOrigin">X location of the Cell to perform the field-of-view calculation with 0 as the farthest left</param>
        /// <param name="yOrigin">Y location of the Cell to perform the field-of-view calculation with 0 as the top</param>
        /// <param name="radius">The number of Cells in which the field-of-view extends from the origin Cell. Think of this as the intensity of the light source.</param>
        /// <param name="lightWalls">True if walls should be included in the field-of-view when they are within the radius of the light source. False excludes walls even when they are within range.</param>
        /// <returns>List of Cells representing what is observable in the Map based on the specified parameters</returns>
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
