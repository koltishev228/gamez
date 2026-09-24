using System;
using UnityEngine;

namespace Zomboid.Data.Items.Components
{
    [Serializable]
    public class FoodComponent : ItemComponent
    {
        [Tooltip("Сколько голода утоляет (в единицах или процентах)")]
        public float HungerRestore = 15f;
        
        [Tooltip("Восстановление жажды")]
        public float ThirstRestore = 0f;
        
        [Tooltip("Можно ли есть в сыром виде?")]
        public bool IsCookable = false;

        [Tooltip("Шанс отравиться, если съесть")]
        [Range(0f, 100f)]
        public float PoisonChance = 0f;
    }
}
