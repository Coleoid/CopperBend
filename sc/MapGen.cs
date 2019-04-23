﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace CbRework
{
    public class MapGen
    {
        protected internal List<Rectangle> Rooms;
        protected TileBase[] _tiles;
        protected int _width;

        public MapGen()
        {
        }

        public Map GenerateMap(int mapWidth, int mapHeight, int maxRooms, int minRoomSize, int maxRoomSize)
        {
            Random randNum = new Random();

            Map map = new Map(mapWidth, mapHeight);
            _tiles = map.Tiles;
            _width = mapWidth;
            FillWalls();

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

            map.PlayerStartPoint = Rooms[0].Center;

            return map;
        }

        public void FillWalls()
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                _tiles[i] = new TileWall();
            }
        }

        private void CreateHorizontalTunnel(int x1, int x2, int y)
        {
            int lowX = Math.Min(x1, x2);
            int highX = Math.Max(x1, x2);
            for (int x = lowX; x <= highX; x++)
                SetTile(x, y, new TileFloor());
        }

        private void CreateVerticalTunnel(int y1, int y2, int x)
        {
            int lowY = Math.Min(y1, y2);
            int highY = Math.Max(y1, y2);
            for (int y = lowY; y <= highY; y++)
                SetTile(x, y, new TileFloor());
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
                    SetTile(x, y, new TileFloor());
                }
            }
        }

        private void SetTile(int x, int y, TileBase tile)
        {
            _tiles[x + (y * _width)] = tile;
        }
    }
}
