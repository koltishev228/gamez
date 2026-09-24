using UnityEngine;

namespace ZombieGame.Health
{
    public class DamageTester : MonoBehaviour
    {
        [Tooltip("Какую часть тела будем тестировать?")]
        public BodyPart targetPart;
        
        [Tooltip("Сколько базового урона нанести?")]
        public float testDamage = 15f;

        [ContextMenu("Тест: Нанести урон (Hit)")]
        public void ApplyDamage()
        {
            if (targetPart != null)
            {
                Debug.Log($"[DamageTester] Наносим {testDamage} базового урона по {targetPart.partName}");
                targetPart.Hit(testDamage);
            }
            else
            {
                Debug.LogWarning("Target Part не назначен! Перетащите скрипт BodyPart в это поле.");
            }
        }

        [ContextMenu("Тест: МГНОВЕННЫЙ ОТРЫВ")]
        public void InstantSever()
        {
            if (targetPart != null)
            {
                targetPart.SeverPart();
            }
        }
    }
}
