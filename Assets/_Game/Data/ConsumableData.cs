using UnityEngine;

namespace ZombieGame.Data
{
    [CreateAssetMenu(fileName = "New Consumable", menuName = "Zombie Game/Items/Consumable")]
    public class ConsumableData : ItemData
    {
        [Header("Потребности (Moodles)")]
        [Tooltip("Минус голод = Персонаж наедается")]
        public float hungerChange = -20f;
        [Tooltip("Минус жажда = Персонаж напивается")]
        public float thirstChange = 0f;

        [Header("Макронутриенты (Хардкор-диета)")]
        public float calories = 250f; // От калорий зависит набор/потеря веса тела
        public float proteins = 10f;  // Белки
        public float carbs = 30f;     // Углеводы
        public float lipids = 5f;     // Жиры

        [Header("Медицина и Менталка")]
        public float healthRestore = 0f;    // Медленное лечение
        public float painReduction = 0f;    // Обезболивающее
        public float stressReduction = 0f;  // Сигареты/Антидепрессанты снимают стресс
        public bool curesInfection = false; // Антибиотики

        [Header("Свежесть (Гниение)")]
        public bool canSpoil = true;
        public int daysToSpoil = 5;
        [Tooltip("Если съесть гнилое, будет отравление?")]
        public bool isDangerousWhenSpoiled = true; 
    }
}
