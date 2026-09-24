using UnityEngine;

namespace ZombieGame.Data
{
    [CreateAssetMenu(fileName = "NeedsConfig", menuName = "Zomboid/Data/NeedsConfig")]
    public class NeedsConfig : ScriptableObject 
    {
        [Tooltip("Скорость роста голода за 1 игровой час (0..1)")]
        public float HungerPerHour = 0.017f; // ~100% за 60 игровых часов
        
        [Tooltip("Скорость роста жажды за 1 игровой час (0..1)")]
        public float ThirstPerHour = 0.042f; // ~100% за 24 ч
        
        [Tooltip("Скорость роста усталости за 1 игровой час (0..1)")]
        public float FatiguePerHour = 0.05f; // ~100% за 20 ч
    }
}
