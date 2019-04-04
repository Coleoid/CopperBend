using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace CbRework
{
    public class MapGen
    {

        public MapGen()
        {
        }

        Map _map; // Temporarily store the map currently worked on

        public Map GenerateMap(int mapWidth, int mapHeight, int maxRooms, int minRoomSize, int maxRoomSize)
        {
            Random randNum = new Random();

            _map = new Map(mapWidth, mapHeight);
            FillWalls();

            // add up to maxRooms non-overlapping rooms to map
            var Rooms = new List<Rectangle>();
            for (int i = 0; i < maxRooms; i++)
            {
                int newRoomWidth = randNum.Next(minRoomSize, maxRoomSize);
                int newRoomHeight = randNum.Next(minRoomSize, maxRoomSize);

                int newRoomX = randNum.Next(0, mapWidth - newRoomWidth - 1);
                int newRoomY = randNum.Next(0, mapHeight - newRoomHeight - 1);

                Rectangle newRoom = new Rectangle(newRoomX, newRoomY, newRoomWidth, newRoomHeight);

                // skip overlapping rooms
                if (Rooms.Any(r => newRoom.Intersects(r))) continue;

                Rooms.Add(newRoom);
                CreateRoom(newRoom);
            }

            return _map;
        }

        public void FillWalls()
        {
            for (int i = 0; i < _map.Tiles.Length; i++)
            {
                _map.Tiles[i] = new TileWall();
            }
        }

        public void CreateRoom(Rectangle room)
        {
            CreateRoom(room.X, room.Y, room.X + room.Width, room.Y + room.Height);
        }

        public void CreateRoom(int xStart, int yStart, int xEnd, int yEnd)
        {
            for (int x = xStart; x <= xEnd; x++)
            {
                for (int y = yStart; y <= yEnd; y++)
                {
                    _map.Tiles[y * _map.Width + x] = new TileFloor();
                }
            }
        }

    }
}
