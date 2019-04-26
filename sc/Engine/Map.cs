using Microsoft.Xna.Framework;
using System.Linq;
using GoRogue;
using System.Collections.Generic;
using CopperBend.Model;

namespace CopperBend.Engine
{
    public class Map
    {
        public TileBase[] Tiles { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Point PlayerStartPoint { get; set; }

        public MultiSpatialMap<CbEntity> Entities { get; set; }

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new TileBase[width * height];
            Entities = new MultiSpatialMap<CbEntity>();
        }

        public bool IsTileWalkable(Point location)
        {
            // off the map is not walkable
            if (location.X < 0 || location.X >= Width
             || location.Y < 0 || location.Y >= Height)
                return false;

            return Tiles[location.Y * Width + location.X].AllowsMove;
        }

        // SpatialMap allows only one at a location.  MultiSpatial allows a collection.
        // Efficiently get the entities of this type at this point
        public IEnumerable<T> GetEntitiesAt<T>(Point location) where T : CbEntity
        {
            return Entities.GetItems(location).OfType<T>();
        }

        /// <summary> Remove this entity from this Map </summary>
        /// <param name="entity"></param>
        public void Remove(CbEntity entity)
        {
            // remove from the GoRogue MultiSpatialMap
            Entities.Remove(entity);

            // No longer notify this map when this entity moves
            entity.Moved -= OnEntityMoved;
        }

        // Adds an Entity to the MultiSpatialMap
        public void Add(CbEntity entity)
        {
            // add entity to the SpatialMap
            Entities.Add(entity, entity.Position);

            // Notify the map when the entity moves, so the map can synch the MultiSpatialMap
            entity.Moved += OnEntityMoved;
        }

        private void OnEntityMoved(object sender, CbEntity.EntityMovedEventArgs args)
        {
            Entities.Move(args.Entity as CbEntity, args.Entity.Position);
        }
    }
}
