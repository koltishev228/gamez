using UnityEngine;

namespace ZombieGame.Health
{
    public class BodyPart : MonoBehaviour
    {
        public string partName = "Arm";
        [Tooltip("Ссылка на центральный скрипт здоровья. Если пусто, найдет автоматически у родителя.")]
        public CharacterHealth mainHealth;
        
        [Header("Damage Settings")]
        [Tooltip("Множитель урона. Голова = 2.0, Рука = 0.5")]
        public float damageMultiplier = 1f;
        [Tooltip("Сколько урона может выдержать конечность до отрыва")]
        public float partMaxHealth = 30f;
        [Tooltip("Если включить, отрыв этой конечности мгновенно убивает персонажа (Голова, Торс)")]
        public bool isVital = false; 

        [Header("Dismemberment Visuals")]
        [Tooltip("Меш (SkinnedMeshRenderer/MeshRenderer) этой конечности на целой модели, который нужно скрыть")]
        public GameObject originalMesh; 
        [Tooltip("Префаб оторванного куска с физикой (Rigidbody)")]
        public GameObject severedPrefab; 
        [Tooltip("Точка спавна куска и крови. Если пусто, будет использовать координаты этого объекта")]
        public Transform severSpawnPoint; 
        [Tooltip("Префаб партиклов крови (опционально)")]
        public ParticleSystem bloodSplatterPrefab;

        private float _currentPartHealth;
        private bool _isSevered = false;

        private void Start()
        {
            _currentPartHealth = partMaxHealth;
            if (mainHealth == null) mainHealth = GetComponentInParent<CharacterHealth>();
            if (severSpawnPoint == null) severSpawnPoint = transform;
        }

        public void Hit(float rawDamage)
        {
            if (_isSevered) return; // Уже оторвано

            // Умножаем на резисты/уязвимости конкретной части тела
            float finalDamage = rawDamage * damageMultiplier;
            _currentPartHealth -= finalDamage;

            // Передаем итоговый урон центральной системе (чтобы отнималась полоска жизней)
            if (mainHealth != null)
            {
                mainHealth.TakeDamage(finalDamage, partName);
            }

            // Если конечность получила критический урон - отрываем её
            if (_currentPartHealth <= 0)
            {
                SeverPart();
            }
        }

        [ContextMenu("Тест: Оторвать конечность")]
        public void SeverPart()
        {
            if (_isSevered) return;
            _isSevered = true;
            Debug.Log($"[BodyPart] Конечность {partName} оторвана!");

            // 1. Скрываем оригинальный меш
            if (originalMesh != null)
            {
                originalMesh.SetActive(false);
            }

            // 2. Спавним физический кусок мяса
            if (severedPrefab != null)
            {
                GameObject severedObj = Instantiate(severedPrefab, severSpawnPoint.position, severSpawnPoint.rotation);
                
                // Добавляем импульс, чтобы кусок красиво отлетел
                Rigidbody rb = severedObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Взрывной вектор вверх и в случайную сторону
                    Vector3 randomForce = (Random.insideUnitSphere + Vector3.up * 1.5f).normalized * 5f;
                    rb.AddForce(randomForce, ForceMode.Impulse);
                    rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
                }
            }

            // 3. Брызги крови
            if (bloodSplatterPrefab != null)
            {
                Instantiate(bloodSplatterPrefab, severSpawnPoint.position, severSpawnPoint.rotation);
            }

            // 4. Проверка на фатальность (если это голова)
            if (isVital && mainHealth != null)
            {
                mainHealth.HandleVitalPartSevered(partName);
            }
        }
    }
}
