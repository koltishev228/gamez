using UnityEngine;

namespace ZombieGame.Data
{
    // Теперь это базовый класс для ВСЕХ предметов. 
    // Он содержит только общие свойства.
    [CreateAssetMenu(fileName = "New Generic Item", menuName = "Zombie Game/Items/Generic Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Базовая информация")]
        public string itemID;          // Уникальный ID (например: brick_01)
        public string itemName;        // Имя в игре (например: Кирпич)
        
        [TextArea(3, 5)]
        public string description;     // Описание для инвентаря
        
        [Header("Физика и Инвентарь")]
        public float weight = 0.5f;    // Физический вес (кг)
        public bool isStackable = true;
        public int maxStackSize = 1;

        [Header("Графика")]
        public Sprite icon;            // 2D картинка для UI
        public GameObject prefab;      // 3D модель для выброса на землю

        [Header("Хардкор: Состояние (Condition)")]
        [Tooltip("Может ли предмет испачкаться грязью?")]
        public bool canBeDirty = true;
        [Tooltip("Может ли предмет быть в крови?")]
        public bool canBeBloody = true;
        [Tooltip("Намокает ли под дождем?")]
        public bool canBeWet = true;
    }
}
