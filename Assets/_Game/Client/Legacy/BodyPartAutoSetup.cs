using UnityEngine;

namespace ZombieGame.Health
{
    public class BodyPartAutoSetup : MonoBehaviour
    {
        [ContextMenu("АВТО-НАСТРОЙКА ВСЕХ ЧАСТЕЙ")]
        public void SetupAllParts()
        {
            CharacterHealth health = GetComponent<CharacterHealth>();
            if (health == null)
            {
                Debug.LogError("Сначала повесьте CharacterHealth на этот объект!");
                return;
            }

            // Ищем любые рендереры мешей (учитывает и Skinned, и обычные)
            Renderer[] allParts = GetComponentsInChildren<Renderer>(true);
            int count = 0;

            foreach (Renderer partMesh in allParts)
            {
                GameObject partObj = partMesh.gameObject;

                // Добавляем BodyPart, если его еще нет
                BodyPart bp = partObj.GetComponent<BodyPart>();
                if (bp == null) bp = partObj.AddComponent<BodyPart>();

                bp.partName = partObj.name;
                bp.mainHealth = health;
                bp.originalMesh = partObj;

                // Пытаемся добавить коллайдер, если его нет
                Collider col = partObj.GetComponent<Collider>();
                if (col == null)
                {
                    MeshCollider mc = partObj.AddComponent<MeshCollider>();
                    mc.convex = true;
                }

                count++;
            }

            Debug.Log($"[AutoSetup] Успешно настроено частей тела: {count}! Можно тестировать.");
        }
    }
}
