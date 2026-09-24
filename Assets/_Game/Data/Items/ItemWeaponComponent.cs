using System;
using UnityEngine;

namespace Zomboid.Data.Items.Components
{
    public enum ItemWeaponType
    {
        Melee,
        Ranged
    }

    [Serializable]
    public class ItemWeaponComponent : ItemComponent
    {
        public ItemWeaponType Type = ItemWeaponType.Melee;
        
        [Tooltip("Базовый урон")]
        public float Damage = 10f;
        
        [Tooltip("Дистанция атаки (для мили - дальность удара, для стрелкового - дальность полета пули)")]
        public float Range = 1.5f;

        [Tooltip("Задержка между атаками в секундах")]
        public float AttackCooldown = 1f;

        [Header("Durability")]
        [Tooltip("Уменьшается ли прочность при использовании?")]
        public bool HasDurability = true;
        public float MaxDurability = 100f;
    }
}
