using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zomboid.Simulation.World
{
    public class WorldGrid : MonoBehaviour
    {
        public static WorldGrid Instance { get; private set; }

        [Header("Serialized Data (Do not edit manually)")]
        [SerializeField] private List<Chunk> _serializedChunks = new List<Chunk>();
        
        private Dictionary<Vector3Int, Chunk> _chunks = new Dictionary<Vector3Int, Chunk>();

        public event Action<Vector3Int> OnEdgeChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                BuildDictionary();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void BuildDictionary()
        {
            _chunks.Clear();
            foreach (var chunk in _serializedChunks)
            {
                _chunks[chunk.Position] = chunk;
            }
        }

        public void SetChunks(List<Chunk> newChunks)
        {
            _serializedChunks = newChunks;
            BuildDictionary();
        }

        public Chunk GetChunk(Vector3Int chunkPos)
        {
            if (_chunks.Count == 0 && _serializedChunks != null && _serializedChunks.Count > 0)
            {
                BuildDictionary();
            }
            _chunks.TryGetValue(chunkPos, out Chunk chunk);
            return chunk;
        }

        public TileData GetTile(int worldX, int worldZ, int floor = 0)
        {
            Vector3Int chunkPos = new Vector3Int(
                Mathf.FloorToInt((float)worldX / Chunk.CHUNK_SIZE),
                floor,
                Mathf.FloorToInt((float)worldZ / Chunk.CHUNK_SIZE)
            );

            Chunk chunk = GetChunk(chunkPos);
            if (chunk == null)
                return default;

            int localX = worldX - chunkPos.x * Chunk.CHUNK_SIZE;
            int localZ = worldZ - chunkPos.z * Chunk.CHUNK_SIZE;

            return chunk.GetTile(localX, localZ);
        }

        public void SetTile(int worldX, int worldZ, int floor, TileData data)
        {
            Vector3Int chunkPos = new Vector3Int(
                Mathf.FloorToInt((float)worldX / Chunk.CHUNK_SIZE),
                floor,
                Mathf.FloorToInt((float)worldZ / Chunk.CHUNK_SIZE)
            );

            Chunk chunk = GetChunk(chunkPos);
            if (chunk == null)
            {
                chunk = new Chunk(chunkPos);
                _chunks[chunkPos] = chunk;
                _serializedChunks.Add(chunk);
            }

            int localX = worldX - chunkPos.x * Chunk.CHUNK_SIZE;
            int localZ = worldZ - chunkPos.z * Chunk.CHUNK_SIZE;

            chunk.SetTile(localX, localZ, data);
        }

        public void SetEdgeState(int worldX, int worldZ, int floor, Dir dir, EdgeState state)
        {
            TileData t = GetTile(worldX, worldZ, floor);
            switch (dir)
            {
                case Dir.North: t.EdgeNorth = state; break;
                case Dir.East:  t.EdgeEast = state; break;
                case Dir.South: t.EdgeSouth = state; break;
                case Dir.West:  t.EdgeWest = state; break;
            }
            SetTile(worldX, worldZ, floor, t);

            Vector3Int chunkPos = new Vector3Int(
                Mathf.FloorToInt((float)worldX / Chunk.CHUNK_SIZE),
                floor,
                Mathf.FloorToInt((float)worldZ / Chunk.CHUNK_SIZE)
            );
            OnEdgeChanged?.Invoke(chunkPos);
        }

        public bool IsWalkable(int worldX, int worldZ, int floor = 0)
        {
            return GetTile(worldX, worldZ, floor).Has(TileFlags.Walkable);
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_serializedChunks == null) return;
            
            Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
            foreach (var chunk in _serializedChunks)
            {
                float floorHeight = chunk.Position.y * 3f; // Высота этажа 3м
                Vector3 center = new Vector3(chunk.Position.x * Chunk.CHUNK_SIZE + Chunk.CHUNK_SIZE / 2f, floorHeight + 0.05f, chunk.Position.z * Chunk.CHUNK_SIZE + Chunk.CHUNK_SIZE / 2f);
                Gizmos.DrawWireCube(center, new Vector3(Chunk.CHUNK_SIZE, 0, Chunk.CHUNK_SIZE));
                
                for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
                {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                    {
                        var tile = chunk.GetTile(x, z);
                        float worldX = chunk.Position.x * Chunk.CHUNK_SIZE + x + 0.5f;
                        float worldZ = chunk.Position.z * Chunk.CHUNK_SIZE + z + 0.5f;
                        Vector3 tileCenter = new Vector3(worldX, floorHeight + 0.1f, worldZ);
                        
                        DrawEdgeGizmo(tileCenter, tile, Dir.North, tile.EdgeNorth);
                        DrawEdgeGizmo(tileCenter, tile, Dir.East, tile.EdgeEast);
                        DrawEdgeGizmo(tileCenter, tile, Dir.South, tile.EdgeSouth);
                        DrawEdgeGizmo(tileCenter, tile, Dir.West, tile.EdgeWest);
                        
                        if (tile.Has(TileFlags.Walkable))
                        {
                            Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
                            Gizmos.DrawCube(tileCenter, new Vector3(0.9f, 0.01f, 0.9f));
                        }
                    }
                }
            }
        }

        private void DrawEdgeGizmo(Vector3 center, TileData tile, Dir dir, EdgeState state)
        {
            bool blocks = Sight.BlocksSight(in tile, dir);
            
            bool hasWall = tile.Has(dir switch { Dir.North => TileFlags.WallNorth, Dir.East => TileFlags.WallEast, Dir.South => TileFlags.WallSouth, _ => TileFlags.WallWest });
            bool hasDoor = tile.Has(dir switch { Dir.North => TileFlags.DoorNorth, Dir.East => TileFlags.DoorEast, Dir.South => TileFlags.DoorSouth, _ => TileFlags.DoorWest });
            bool hasWindow = tile.Has(dir switch { Dir.North => TileFlags.WindowNorth, Dir.East => TileFlags.WindowEast, Dir.South => TileFlags.WindowSouth, _ => TileFlags.WindowWest });

            if (!hasWall && !hasDoor && !hasWindow) return;

            Gizmos.color = blocks ? Color.red : Color.cyan;

            Vector3 p1 = center, p2 = center;
            switch (dir)
            {
                case Dir.North: p1 += new Vector3(-0.5f, 0, 0.5f); p2 += new Vector3(0.5f, 0, 0.5f); break;
                case Dir.East:  p1 += new Vector3(0.5f, 0, 0.5f); p2 += new Vector3(0.5f, 0, -0.5f); break;
                case Dir.South: p1 += new Vector3(0.5f, 0, -0.5f); p2 += new Vector3(-0.5f, 0, -0.5f); break;
                case Dir.West:  p1 += new Vector3(-0.5f, 0, -0.5f); p2 += new Vector3(-0.5f, 0, 0.5f); break;
            }
            
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p1 + Vector3.up * 0.05f, p2 + Vector3.up * 0.05f);
        }
#endif
    }
}
