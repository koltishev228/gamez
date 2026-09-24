using UnityEngine;
using FishNet.Object;

namespace ZombieGame.Health
{
    // Типы ран (Как в Zomboid)
    public enum WoundType { None, Scratch, Laceration, Bite, Burn, Fracture }

    // Конкретные части тела
    public enum BodyPartType 
    { 
        Head, Neck, Torso, Groin, 
        LeftArm, RightArm, LeftHand, RightHand, 
        LeftLeg, RightLeg, LeftFoot, RightFoot 
    }

    [System.Serializable]
    // ПЕРЕИМЕНОВАТЬ: Было BodyPart, стало BodyPartStats, чтобы не конфликтовать со старым скриптом расчлененки!
    public class BodyPartStats 
    {
        public BodyPartType partType;
        public float health = 100f;
        
        [Header("Состояния ранения")]
        public WoundType wound = WoundType.None;
        public bool isBleeding = false;
        public bool isBandaged = false;
        public bool isDisinfected = false;
        
        [Header("Инфекции")]
        public bool hasRegularInfection = false; // Попала грязь в рану
        public bool hasZombieInfection = false;  // Z-вирус (Смерть)
    }

    public class HealthSystem : NetworkBehaviour
    {
        [Header("Общее состояние (Для UI)")]
        public float overallHealth = 100f; // Высчитывается автоматически
        public bool isDead = false;
        
        [Header("Анатомия персонажа")]
        public BodyPartStats[] bodyParts;

        private void Awake()
        {
            // Инициализация всех жизненно важных частей тела
            bodyParts = new BodyPartStats[]
            {
                new BodyPartStats { partType = BodyPartType.Head },
                new BodyPartStats { partType = BodyPartType.Neck },
                new BodyPartStats { partType = BodyPartType.Torso },
                new BodyPartStats { partType = BodyPartType.Groin },
                new BodyPartStats { partType = BodyPartType.LeftArm },
                new BodyPartStats { partType = BodyPartType.RightArm },
                new BodyPartStats { partType = BodyPartType.LeftLeg },
                new BodyPartStats { partType = BodyPartType.RightLeg }
            };
        }

        // Пример метода получения хардкорного урона
        [Server]
        public void ReceiveDamage(BodyPartType part, float damage, WoundType woundType)
        {
            if (isDead) return;

            foreach (var bp in bodyParts)
            {
                if (bp.partType == part)
                {
                    bp.health -= damage;
                    bp.wound = woundType;
                    
                    // Царапины, рваные раны и укусы вызывают кровотечение
                    if (woundType == WoundType.Scratch || woundType == WoundType.Laceration || woundType == WoundType.Bite)
                    {
                        bp.isBleeding = true;
                    }

                    // Шанс заражения зомби-вирусом (Классика Zomboid)
                    if (woundType == WoundType.Bite) bp.hasZombieInfection = true; // 100%
                    if (woundType == WoundType.Laceration && Random.value < 0.25f) bp.hasZombieInfection = true; // 25%
                    if (woundType == WoundType.Scratch && Random.value < 0.07f) bp.hasZombieInfection = true; // 7%

                    break;
                }
            }
            RecalculateOverallHealth();
        }

        [Server]
        private void RecalculateOverallHealth()
        {
            float total = 0;
            foreach (var bp in bodyParts)
            {
                total += bp.health;
            }
            overallHealth = total / bodyParts.Length;

            // Смерть от падения здоровья до нуля
            if (overallHealth <= 0)
            {
                Die();
            }
        }

        [Server]
        private void Die()
        {
            isDead = true;
            Debug.Log("Игрок мертв!");
            // Позже добавим превращение в зомби, если hasZombieInfection == true
        }
    }
}
