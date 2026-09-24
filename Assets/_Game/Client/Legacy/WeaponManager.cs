using UnityEngine;
using ZombieGame.Data;
using FishNet.Object;

namespace ZombieGame.Equipment
{
    public class WeaponManager : NetworkBehaviour
    {
        [Header("Настройки костей")]
        [Tooltip("Перетащите сюда кость правой руки персонажа (например, mixamorig:RightHand)")]
        public Transform rightHandSocket;

        [Header("Настройка хвата (Офсеты)")]
        [Tooltip("Двигайте эти ползунки в игре, чтобы топор ровно лег в ладонь")]
        public Vector3 positionOffset = Vector3.zero;
        public Vector3 rotationOffset = Vector3.zero;

        private GameObject _currentWeaponObj;
        public WeaponData CurrentWeapon { get; private set; }

        public void EquipWeapon(WeaponData weapon)
        {
            if (weapon == null || weapon.prefab == null) return;
            
            if (rightHandSocket == null)
            {
                Debug.LogError("Сначала назначьте кость RightHand в скрипте WeaponManager на персонаже!");
                return;
            }

            // Удаляем старое оружие из рук, если оно было
            if (_currentWeaponObj != null)
            {
                Destroy(_currentWeaponObj);
            }

            CurrentWeapon = weapon;

            // Создаем 3D-модель и сразу делаем её дочерней к руке
            _currentWeaponObj = Instantiate(weapon.prefab, rightHandSocket);
            
            // КРИТИЧЕСКИ ВАЖНО: Отключаем физику и скрипт подбора у предмета в руке.
            // Иначе топор будет сталкиваться с ногами и мешать ходить, а игра будет думать, что он лежит на земле.
            var colliders = _currentWeaponObj.GetComponentsInChildren<Collider>();
            foreach (var col in colliders) col.enabled = false;
            
            var pickup = _currentWeaponObj.GetComponent<Interaction.PickupItem>();
            if (pickup != null) pickup.enabled = false;

            // Центрируем модель прямо в ладони с учетом нашего смещения
            _currentWeaponObj.transform.localPosition = positionOffset;
            _currentWeaponObj.transform.localEulerAngles = rotationOffset;

            Debug.Log($"[WeaponManager] Взято в руки: {weapon.itemName}");
        }

        // Этот метод будет полезен для настройки топора прямо во время игры
        private void Update()
        {
            if (_currentWeaponObj != null)
            {
                // Если мы меняем оффсеты в Инспекторе, оружие двигается в реальном времени
                _currentWeaponObj.transform.localPosition = positionOffset;
                _currentWeaponObj.transform.localEulerAngles = rotationOffset;
            }
        }

        [ContextMenu("✨ Магия: Найти правую руку")]
        public void AutoAssignRightHand()
        {
            Transform[] allBones = GetComponentsInChildren<Transform>();
            foreach (Transform t in allBones)
            {
                string n = t.name.ToLower();
                // Ищем кисть правой руки (именно ладонь, а не пальцы)
                if (n.Contains("right") && n.Contains("hand") && !n.Contains("thumb") && !n.Contains("index") && !n.Contains("middle") && !n.Contains("ring") && !n.Contains("pinky"))
                {
                    rightHandSocket = t;
                    Debug.Log($"[WeaponManager] Правая рука найдена: {t.name}");
                    return;
                }
            }
            Debug.LogWarning("[WeaponManager] Кость правой руки не найдена!");
        }
    }
}
