using UnityEngine;
using ZombieGame.Simulation; 

namespace Zomboid.Simulation.World
{
    public class Building : MonoBehaviour
    {
        [Header("Parts to hide when player is inside")]
        [Tooltip("Перетащите сюда объекты крыши и верхних этажей")]
        public GameObject[] RoofObjects;
        
        [Tooltip("Перетащите сюда стены, перекрывающие вид (обычно Южные и Восточные)")]
        public GameObject[] BlockingWalls;

        [Header("Auto-Generation (Опционально)")]
        [Tooltip("Перетащите сюда папку с полами для авто-создания зоны этажа")]
        public Transform FloorParent;
        public float CeilingHeight = 3.5f;

        [Tooltip("Перетащите сюда папку с окнами для авто-создания зон заглядывания")]
        public Transform WindowsParent;

        private int _localPlayersInside = 0;
        private int _peekers = 0;

        public void AddPeeker() { _peekers++; UpdateVisibility(); }
        public void RemovePeeker() { _peekers--; if (_peekers < 0) _peekers = 0; UpdateVisibility(); }

        private void UpdateVisibility()
        {
            bool isVisible = (_localPlayersInside == 0 && _peekers == 0);
            ToggleVisibility(isVisible);
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<PlayerControllerNet>();
            if (player != null && player.IsOwner)
            {
                _localPlayersInside++;
                UpdateVisibility();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var player = other.GetComponentInParent<PlayerControllerNet>();
            if (player != null && player.IsOwner)
            {
                _localPlayersInside--;
                if (_localPlayersInside < 0) _localPlayersInside = 0;
                UpdateVisibility();
            }
        }

        private void ToggleVisibility(bool isVisible)
        {
            foreach (var obj in RoofObjects)
            {
                if (obj != null)
                {
                    foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
                        renderer.enabled = isVisible;
                }
            }
            
            foreach (var obj in BlockingWalls)
            {
                if (obj != null)
                {
                    foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
                        renderer.enabled = isVisible;
                }
            }
        }

        [ContextMenu("Auto-Fit Trigger to House (Один большой куб)")]
        private void AutoFitTrigger()
        {
            BoxCollider col = GetComponent<BoxCollider>();
            if (col == null) col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;

            Transform root = transform.parent != null ? transform.parent : transform;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);

            transform.position = root.position;
            transform.rotation = root.rotation;
            col.center = transform.InverseTransformPoint(bounds.center);
            col.size = bounds.size;
            
            Debug.Log("Сгенерирован один большой триггер!");
        }

        [ContextMenu("Auto-Generate Triggers From Floors (Точно по форме)")]
        private void GenerateTriggersFromFloors()
        {
            if (FloorParent == null)
            {
                Debug.LogError("Сначала назначьте FloorParent в инспекторе!");
                return;
            }

            var oldColliders = GetComponents<BoxCollider>();
            foreach (var c in oldColliders) DestroyImmediate(c);

            Renderer[] floorRenderers = FloorParent.GetComponentsInChildren<Renderer>();
            if (floorRenderers.Length == 0) return;

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            foreach (var r in floorRenderers)
            {
                BoxCollider col = gameObject.AddComponent<BoxCollider>();
                col.isTrigger = true;

                Bounds b = r.bounds;
                Vector3 worldCenter = b.center;
                worldCenter.y = b.min.y + (CeilingHeight / 2f);
                
                col.center = transform.InverseTransformPoint(worldCenter);
                
                Vector3 size = b.size;
                size.y = CeilingHeight;
                size.x += 0.1f;
                size.z += 0.1f;
                col.size = size;
            }
            
            Debug.Log($"Успех! Создано {floorRenderers.Length} триггеров пола.");
        }

        [ContextMenu("Auto-Generate Window Peek Triggers")]
        private void GenerateWindowTriggers()
        {
            if (WindowsParent == null)
            {
                Debug.LogError("Назначьте WindowsParent в инспекторе!");
                return;
            }

            foreach (var t in GetComponentsInChildren<WindowPeekTrigger>())
            {
                DestroyImmediate(t.gameObject);
            }

            int count = 0;
            foreach (Transform window in WindowsParent)
            {
                Renderer r = window.GetComponentInChildren<Renderer>();
                if (r == null) continue;

                GameObject triggerObj = new GameObject($"AutoWindowTrigger_{count}");
                triggerObj.transform.SetParent(this.transform);
                
                // Ставим триггер ровно по геометрическому центру окна
                triggerObj.transform.position = r.bounds.center;
                triggerObj.transform.rotation = window.rotation; // берем поворот самого окна

                BoxCollider col = triggerObj.AddComponent<BoxCollider>();
                col.isTrigger = true;
                
                // Делаем куб ОГРОМНЫМ в глубину (Z = 4), чтобы он точно доставал до игрока на улице, 
                // даже если у стен толстые невидимые коллайдеры, не дающие подойти вплотную.
                col.size = new Vector3(2.5f, 2.5f, 4f); 
                col.center = Vector3.zero; 

                var peekScript = triggerObj.AddComponent<WindowPeekTrigger>();
                peekScript.TargetBuilding = this;
                peekScript.PeekAngle = 60f;

                count++;
            }
            
            Debug.Log($"Сгенерировано зон для окон: {count}");
        }
    }
}
