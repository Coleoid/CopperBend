using CopperBend.MapUtil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CopperBend.MapUtil
{
    public interface IMap
    {
        int Width
        {
            get;
        }

        int Height
        {
            get;
        }

        void Initialize(int width, int height);

        bool IsTransparent(int x, int y);

        bool IsWalkable(int x, int y);

        bool IsExplored(int x, int y);

        void SetIsWalkable(Point point, bool isWalkable);
        void SetIsExplored(Point point, bool isExplored);
        void SetIsTransparent(Point point, bool isTransparent);
        bool IsWithinMap(Point point);

        /// <summary> Sets whole Map to be transparent and walkable </summary>
        void Clear();

        /// <summary> Sets the whole Map to the specified values </summary>
        void Clear(bool isTransparent, bool isWalkable);

        IMap Clone();

        /// <summary>
        /// Copies the Cell properties of a smaller source Map into this destination Map at the specified location
        /// </summary>
        void Copy(IMap sourceMap, int left, int top);

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
        ReadOnlyCollection<Cell> ComputeFov(int xOrigin, int yOrigin, int radius, bool lightWalls);

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
        ReadOnlyCollection<Cell> AppendFov(int xOrigin, int yOrigin, int radius, bool lightWalls);

        IEnumerable<Cell> GetAllCells();

        IEnumerable<Cell> GetCellsAlongLine(int xOrigin, int yOrigin, int xDestination, int yDestination);

        IEnumerable<Cell> GetCellsInCircle(int xCenter, int yCenter, int radius);

        IEnumerable<Cell> GetCellsInDiamond(int xCenter, int yCenter, int distance);

        IEnumerable<Cell> GetCellsInSquare(int xCenter, int yCenter, int distance);
        IEnumerable<Point> GetCoordsInSquare(int xCenter, int yCenter, int distance);

        IEnumerable<Cell> GetBorderCellsInCircle(int xCenter, int yCenter, int radius);

        IEnumerable<Cell> GetBorderCellsInDiamond(int xCenter, int yCenter, int distance);

        IEnumerable<Cell> GetBorderCellsInSquare(int xCenter, int yCenter, int distance);

        IEnumerable<Cell> GetCellsInRows(params int[] rowNumbers);

        IEnumerable<Cell> GetCellsInColumns(params int[] columnNumbers);

        Cell GetCell(int x, int y);
        Cell GetCell(Point point);

        string ToString(bool useFov);

        MapState Save();

        void Restore(MapState state);

        Cell CellFor(int index);

        int IndexFor(int x, int y);

        int IndexFor(Point cell);
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

        public int Width
        {
            get; set;
        }

        public int Height
        {
            get; set;
        }

        public CellProperties[] Cells
        {
            get; set;
        }
    }

}
