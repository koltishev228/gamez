using UnityEngine;
using UnityEditor;
using Zomboid.Simulation.World;
using System.Collections.Generic;

namespace Zomboid.Editor
{
    public class WorldGridBaker : EditorWindow
    {
        private Bounds bakeBounds = new Bounds(Vector3.zero, new Vector3(100, 10, 100));
        private int wallLayerIndex = 0;
        private int floorLayerIndex = 0;
        private int doorLayerIndex = 0;
        private int windowLayerIndex = 0;

        private LayerMask wallLayerMask;
        private LayerMask floorLayerMask;
        private LayerMask doorLayerMask;
        private LayerMask windowLayerMask;

        [MenuItem("Tools/Bake World Grid")]
        public static void ShowWindow()
        {
            GetWindow<WorldGridBaker>("World Grid Baker");
        }

        private void OnGUI()
        {
            GUILayout.Label("World Grid Settings", EditorStyles.boldLabel);
            bakeBounds = EditorGUILayout.BoundsField("Bake Bounds", bakeBounds);
            wallLayerIndex = EditorGUILayout.LayerField("Wall Layer", wallLayerIndex);
            floorLayerIndex = EditorGUILayout.LayerField("Floor Layer", floorLayerIndex);
            doorLayerIndex = EditorGUILayout.LayerField("Door Layer", doorLayerIndex);
            windowLayerIndex = EditorGUILayout.LayerField("Window Layer", windowLayerIndex);

            wallLayerMask = 1 << wallLayerIndex;
            floorLayerMask = 1 << floorLayerIndex;
            doorLayerMask = 1 << doorLayerIndex;
            windowLayerMask = 1 << windowLayerIndex;

            if (GUILayout.Button("Bake Grid"))
            {
                Bake();
            }
        }

        private void Bake()
        {
            WorldGrid grid = FindObjectOfType<WorldGrid>();
            if (grid == null)
            {
                GameObject go = new GameObject("WorldGrid");
                grid = go.AddComponent<WorldGrid>();
                Undo.RegisterCreatedObjectUndo(go, "Create WorldGrid");
            }

            int minX = Mathf.FloorToInt(bakeBounds.min.x);
            int maxX = Mathf.CeilToInt(bakeBounds.max.x);
            int minZ = Mathf.FloorToInt(bakeBounds.min.z);
            int maxZ = Mathf.CeilToInt(bakeBounds.max.z);
            
            int maxFloors = Mathf.Max(1, Mathf.CeilToInt(bakeBounds.size.y / 3f));

            List<Chunk> newChunks = new List<Chunk>();
            Dictionary<Vector3Int, Chunk> chunkDict = new Dictionary<Vector3Int, Chunk>();

            // Сканируем вообще всё, что имеет коллайдеры (Default слой и т.д.)
            LayerMask everythingMask = ~0; 

            for (int floor = 0; floor < maxFloors; floor++)
            {
                // Раньше высота "этажа 0" считалась от bakeBounds.min.y — это значило, что весь расчёт
                // этажей плавает при любом изменении Center/Extent в окне бейкера. Теперь этаж 0 всегда
                // около мировой высоты Y=0..3 (как и предполагает VisibilitySolver, который пока жёстко
                // читает floor=0), независимо от размера Bake Bounds.
                float yPos = floor * 3f + 1f;

                for (int x = minX; x < maxX; x++)
                {
                    for (int z = minZ; z < maxZ; z++)
                    {
                        TileData tile = new TileData();
                        Vector3 center = new Vector3(x + 0.5f, yPos, z + 0.5f);
                        
                        // Ищем пол. QueryTriggerInteraction.Ignore — иначе BuildingTrigger/BuildingTrigger_Attic
                        // (прямоугольные триггеры на весь дом, даже если сам дом Г-образный) сами считаются "стеной".
                        if (Physics.Raycast(center + Vector3.up * 5f, Vector3.down, out _, 10f, everythingMask, QueryTriggerInteraction.Ignore))
                        {
                            tile.SetFlag(TileFlags.Floor, true);
                            tile.SetFlag(TileFlags.Walkable, true);
                        }

                        CheckEdgeSmart(center, Vector3.forward, ref tile, TileFlags.WallNorth, TileFlags.DoorNorth, TileFlags.WindowNorth, everythingMask);
                        CheckEdgeSmart(center, Vector3.back, ref tile, TileFlags.WallSouth, TileFlags.DoorSouth, TileFlags.WindowSouth, everythingMask);
                        CheckEdgeSmart(center, Vector3.right, ref tile, TileFlags.WallEast, TileFlags.DoorEast, TileFlags.WindowEast, everythingMask);
                        CheckEdgeSmart(center, Vector3.left, ref tile, TileFlags.WallWest, TileFlags.DoorWest, TileFlags.WindowWest, everythingMask);

                        if (Physics.CheckSphere(center, 0.3f, everythingMask, QueryTriggerInteraction.Ignore))
                        {
                            tile.SetFlag(TileFlags.Walkable, false);
                        }

                        Vector3Int chunkPos = new Vector3Int(
                            Mathf.FloorToInt((float)x / Chunk.CHUNK_SIZE),
                            floor,
                            Mathf.FloorToInt((float)z / Chunk.CHUNK_SIZE)
                        );

                        if (!chunkDict.TryGetValue(chunkPos, out Chunk chunk))
                        {
                            chunk = new Chunk(chunkPos);
                            chunkDict[chunkPos] = chunk;
                            newChunks.Add(chunk);
                        }

                        int localX = x - chunkPos.x * Chunk.CHUNK_SIZE;
                        int localZ = z - chunkPos.z * Chunk.CHUNK_SIZE;
                        chunk.SetTile(localX, localZ, tile);
                    }
                }
            }

            Undo.RecordObject(grid, "Bake World Grid");
            grid.SetChunks(newChunks);
            EditorUtility.SetDirty(grid);

            Debug.Log($"Baked {newChunks.Count} chunks successfully.");
        }

        // Раньше был один луч с одной высоты (1м над полом) — стены со смещённым коллайдером,
        // дверные рамы или тонкие простенки часто промахивались, и грань считалась открытой (сквозь неё было видно).
        // Теперь бьём несколько лучей на разных высотах и чуть смещаем начало по бокам, чтобы не проскакивало.
        private static readonly float[] SampleHeights = { -0.6f, 0f, 0.6f };
        private static readonly float[] SampleOffsets = { -0.3f, 0f, 0.3f };

        private void CheckEdgeSmart(Vector3 center, Vector3 dir, ref TileData tile, TileFlags wallF, TileFlags doorF, TileFlags windowF, LayerMask mask)
        {
            Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
            bool isWindow = false, isDoor = false, isWall = false;

            foreach (float h in SampleHeights)
            {
                foreach (float o in SampleOffsets)
                {
                    Vector3 origin = center + Vector3.up * h + side * o;
                    if (!Physics.Raycast(origin, dir, out RaycastHit hit, 0.6f, mask, QueryTriggerInteraction.Ignore)) continue;

                    // Пропускаем сами триггеры-маркеры (BuildingTrigger и т.п.) — это не геометрия, а логические зоны.
                    if (hit.collider.isTrigger) continue;

                    // Пропускаем пол/потолок/скат крыши (Interior_FloorToRoof и т.п.) — у них нормаль
                    // почти вертикальная, у настоящей стены — почти горизонтальная. Без этой проверки
                    // наклонный чердачный пол пересекает горизонтальные лучи почти везде и весь чердак
                    // считается сплошной стеной.
                    if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) > 0.5f) continue;

                    string objName = hit.collider.gameObject.name.ToLower();
                    string parentName = hit.collider.transform.parent != null ? hit.collider.transform.parent.name.ToLower() : "";

                    if (objName.Contains("window") || parentName.Contains("window") || objName.Contains("glass"))
                        isWindow = true;
                    else if (objName.Contains("door") || parentName.Contains("door"))
                        isDoor = true;
                    else
                        isWall = true;
                }
            }

            // Приоритет: сплошная стена перекрывает дверь/окно, найденные тем же лучом на другой высоте
            // (например, дверная коробка тоже задевается лучом) — если хоть где-то сплошная стена, считаем стеной.
            if (isWall) tile.SetFlag(wallF, true);
            else if (isDoor) tile.SetFlag(doorF, true);
            else if (isWindow) tile.SetFlag(windowF, true);
        }
    }
}
