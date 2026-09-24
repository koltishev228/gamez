using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Zomboid.Editor
{
    // ModularHousePack1 в основном без коллайдеров на стенах/дверях/окнах/углах — WorldGridBaker
    // стреляет рейкастами и физически не во что попадать. Этот тул один раз добавляет MeshCollider
    // на все меши модулей дома, у которых своего коллайдера ещё нет.
    public static class AddMissingColliders
    {
        private static readonly string[] TargetFolders =
        {
            "Assets/ModularHousePack1/Prefabs/Modules/Walls",
            "Assets/ModularHousePack1/Prefabs/Modules/Doors",
            "Assets/ModularHousePack1/Prefabs/Modules/Window",
            "Assets/ModularHousePack1/Prefabs/Modules/Corners",
            "Assets/ModularHousePack1/Prefabs/Modules/Floors",
        };

        [MenuItem("Tools/Zomboid/Add Missing Colliders To House Modules")]
        public static void Run()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", TargetFolders);
            int prefabsTouched = 0;
            int collidersAdded = 0;

            foreach (var guid in guids.Distinct())
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                bool changed = false;
                var meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);

                foreach (var mf in meshFilters)
                {
                    if (mf.sharedMesh == null) continue;
                    if (mf.GetComponent<Collider>() != null) continue;

                    var mc = mf.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    mc.convex = false; // статичная геометрия — нужна только для рейкастов бейкера
                    changed = true;
                    collidersAdded++;
                }

                if (changed)
                {
                    EditorUtility.SetDirty(prefab);
                    prefabsTouched++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[AddMissingColliders] Готово: обработано {guids.Distinct().Count()} префабов, изменено {prefabsTouched}, добавлено {collidersAdded} MeshCollider.");
        }
    }
}
