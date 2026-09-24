using System;
using UnityEngine;

namespace Zomboid.Simulation.World
{
    [Serializable]
    public class Chunk
    {
        public const int CHUNK_SIZE = 16;
        
        public Vector3Int Position; // x = cx, y = floor, z = cz
        public TileData[] Tiles;

        public Chunk(Vector3Int position)
        {
            Position = position;
            Tiles = new TileData[CHUNK_SIZE * CHUNK_SIZE];
        }

        public int GetIndex(int localX, int localZ)
        {
            return localZ * CHUNK_SIZE + localX;
        }

        public TileData GetTile(int localX, int localZ)
        {
            if (localX < 0 || localX >= CHUNK_SIZE || localZ < 0 || localZ >= CHUNK_SIZE)
                return default;
                
            return Tiles[GetIndex(localX, localZ)];
        }

        public void SetTile(int localX, int localZ, TileData data)
        {
            if (localX >= 0 && localX < CHUNK_SIZE && localZ >= 0 && localZ < CHUNK_SIZE)
            {
                Tiles[GetIndex(localX, localZ)] = data;
            }
        }
    }
}
