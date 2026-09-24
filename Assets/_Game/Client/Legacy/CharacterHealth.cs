using UnityEngine;

namespace ZombieGame.Health
{
    public class CharacterHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        public float maxHealth = 100f;
        
        private float _currentHealth;
        private bool _isDead = false;

        private void Start()
        {
            _currentHealth = maxHealth;
        }

        public void TakeDamage(float amount, string partName = "Body")
        {
            if (_isDead) return;

            _currentHealth -= amount;
            Debug.Log($"[CharacterHealth] Получено {amount} урона по {partName}. Осталось ХП: {_currentHealth}");

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void HandleVitalPartSevered(string partName)
        {
            if (_isDead) return;
            Debug.Log($"[CharacterHealth] ФАТАЛЬНО! Оторвана критическая часть тела: {partName}");
            Die();
        }

        private void Die()
        {
            _isDead = true;
            Debug.Log("[CharacterHealth] ПЕРСОНАЖ МЕРТВ!");
            // Позже здесь будет переход в Ragdoll или анимация смерти
        }
    }
}
