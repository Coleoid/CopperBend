using System;
using System.Collections.Generic;
using System.Linq;
using CopperBend.Contract;
using log4net;
using Microsoft.Xna.Framework;
using SadConsole;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CopperBend.Engine
{

    public class MapGen
    {
        protected internal List<Rectangle> Rooms;
        protected SpaceMap _map;
        private ILog log;
        private Cell _wallCell;
        private TerrainType _wall;
        private Cell _floorCell;
        private TerrainType _floor;

        public MapGen()
        {
            log = LogManager.GetLogger("CB", "CB.MapGen");
            _wallCell = new Cell(Color.LightGray, Color.DarkGray, '#');
            _floorCell = new Cell(Color.DarkGray, Color.Black, '.');

            _wall = new TerrainType
            {
                Name = "Wall",
                CanWalkThrough = false,
                CanSeeThrough = false,
                CanPlant = false,
                Looks = _wallCell
            };

            _floor = new TerrainType
            {
                Name = "Floor",
                CanWalkThrough = true,
                CanSeeThrough = true,
                CanPlant = true,
                Looks = _floorCell
            };

        }

        public SpaceMap GenerateMap(int mapWidth, int mapHeight, int maxRooms, int minRoomSize, int maxRoomSize)
        {
            Random randNum = new Random();

            _map = new SpaceMap(mapWidth, mapHeight);
            FillWalls(_map);

            // add up to maxRooms non-overlapping rooms to map
            Rooms = new List<Rectangle>();
            for (int i = 0; i < maxRooms; i++)
            {
                int newRoomWidth = randNum.Next(minRoomSize, maxRoomSize);
                int newRoomHeight = randNum.Next(minRoomSize, maxRoomSize);

                int newRoomX = randNum.Next(0, mapWidth - newRoomWidth - 1);
                int newRoomY = randNum.Next(0, mapHeight - newRoomHeight - 1);

                Rectangle newRoom = new Rectangle(newRoomX, newRoomY, newRoomWidth, newRoomHeight);

                // rooms which would overlap existing rooms are skipped
                var borderedRoom = new Rectangle(newRoom.Location, newRoom.Size);
                borderedRoom.Inflate(4, 4);
                if (Rooms.Any(r => borderedRoom.Intersects(r))) continue;

                Rooms.Add(newRoom);
                CreateRoom(newRoom);

                for (int r = 1; r < Rooms.Count; r++)
                {
                    //for all remaining rooms get the center of the room and the previous room
                    Point previousRoomCenter = Rooms[r - 1].Center;
                    Point currentRoomCenter = Rooms[r].Center;

                    CreateHorizontalTunnel(previousRoomCenter.X, currentRoomCenter.X, previousRoomCenter.Y);
                    CreateVerticalTunnel(previousRoomCenter.Y, currentRoomCenter.Y, currentRoomCenter.X);
                }
            }

            _map.PlayerStartPoint = Rooms[0].Center;

            return _map;
        }

        public void FillWalls(SpaceMap map)
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    map.Add(new Space {Terrain = _wall}, x, y);
                }
            }
        }

        private void CreateHorizontalTunnel(int x1, int x2, int y)
        {
            int lowX = Math.Min(x1, x2);
            int highX = Math.Max(x1, x2);
            for (int x = lowX; x <= highX; x++)
                SetFloorSpace(x, y);
        }

        private void CreateVerticalTunnel(int y1, int y2, int x)
        {
            int lowY = Math.Min(y1, y2);
            int highY = Math.Max(y1, y2);
            for (int y = lowY; y <= highY; y++)
                SetFloorSpace(x, y);
        }

        private void SetFloorSpace(int x, int y)
        {
            var space = _map.GetItem(x, y);
            space.Terrain = _floor;
        }

        public void CreateRoom(Rectangle room)
        {
            CreateRoom(room.X, room.Y, room.X + room.Width, room.Y + room.Height);
        }

        public void CreateRoom(int xStart, int yStart, int xEnd, int yEnd)
        {
            //log.Debug($"Creating room [({xStart}, {yStart})-({xEnd}, {yEnd})]");
            for (int y = yStart; y <= yEnd; y++)
            {
                for (int x = xStart; x <= xEnd; x++)
                {
                    SetFloorSpace(x, y);
                }
            }
        }
    }
}
