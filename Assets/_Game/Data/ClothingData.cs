using UnityEngine;

namespace ZombieGame.Data
{
    // Слоты, куда можно надеть вещь
    public enum EquipSlot { Head, Face, Neck, Torso, Back, Arms, Hands, Legs, Feet }

    [CreateAssetMenu(fileName = "New Clothing", menuName = "Zombie Game/Items/Clothing")]
    public class ClothingData : ItemData
    {
        [Header("Параметры Одежды")]
        public EquipSlot equipSlot = EquipSlot.Torso;
        
        [Header("Защита (Protection)")]
        [Range(0f, 100f)] public float biteDefense = 0f;    // Шанс заблокировать укус
        [Range(0f, 100f)] public float scratchDefense = 10f; // Шанс заблокировать царапину
        
        [Header("Выживание (Survival)")]
        [Range(0f, 1f)] public float insulation = 0.5f;       // Насколько вещь греет зимой
        [Range(0f, 1f)] public float windResistance = 0.2f;   // Защита от пронизывающего ветра
        [Range(0f, 1f)] public float waterResistance = 0.1f;  // Водонепроницаемость

        [Header("Штрафы")]
        [Tooltip("Множитель скорости бега (тяжелая броня замедляет)")]
        public float runSpeedModifier = 1.0f; 
    }
}
