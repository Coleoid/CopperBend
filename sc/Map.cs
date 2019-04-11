//using System;

using Microsoft.Xna.Framework;
using System.Linq;

namespace CbRework
{
    public class Map
    {
        public TileBase[] Tiles { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Point PlayerStartPoint { get; set; }

        public GoRogue.MultiSpatialMap<CbEntity> Entities; // Keeps track of all the Entities on the map

        //Build a new map with a specified width and height
        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new TileBase[width * height];
        }

        public bool IsTileWalkable(Point location)
        {
            // off the map is disallowed
            if (location.X < 0 || location.X >= Width
             || location.Y < 0 || location.Y >= Height)
                return false;

            return Tiles[location.Y * Width + location.X].AllowsMove;
        }

        // Checking whether a certain type of
        // entity is at a specified location the manager's list of entities
        // and if it exists, return that Entity
        public T GetEntityAt<T>(Point location) where T : CbEntity
        {
            return Entities.GetItems(location).OfType<T>().FirstOrDefault();
        }

        // Removes an Entity from the MultiSpatialMap
        public void Remove(CbEntity entity)
        {
            // remove from SpatialMap
            Entities.Remove(entity);

            // Link up the entity's Moved event to a new handler
            entity.Moved -= OnEntityMoved;
        }

        // Adds an Entity to the MultiSpatialMap
        public void Add(CbEntity entity)
        {
            // add entity to the SpatialMap
            Entities.Add(entity, entity.Position);

            // Link up the entity's Moved event to a new handler
            entity.Moved += OnEntityMoved;
        }

        // When the Entity's .Moved value changes, it triggers this event handler
        // which updates the Entity's current position in the SpatialMap
        private void OnEntityMoved(object sender, CbEntity.EntityMovedEventArgs args)
        {
            Entities.Move(args.Entity as CbEntity, args.Entity.Position);
        }
    }
}
