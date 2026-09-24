using UnityEngine;

namespace ZombieGame.Data
{
    public enum WeaponType { MeleeBlunt, MeleeBlade, Firearm }

    [CreateAssetMenu(fileName = "New Weapon", menuName = "Zombie Game/Items/Weapon")]
    public class WeaponData : ItemData
    {
        [Header("Боевые Характеристики")]
        public WeaponType weaponType;
        
        public float minDamage = 1f;
        public float maxDamage = 2f;
        public float attackSpeed = 1f;
        public float attackRange = 1.5f;
        
        [Header("Прочность (Durability)")]
        public int maxCondition = 10;
        [Range(0f, 100f)] 
        [Tooltip("Шанс в процентах, что оружие сломается при 1 ударе")]
        public float conditionLowerChance = 10f; 

        [Header("Огнестрел (Только для Firearm)")]
        [Tooltip("На каком расстоянии зомби услышат выстрел")]
        public float noiseRadius = 50f;
        public int maxAmmo = 0;
        public string ammoType = ""; // ID патронов
    }
}
