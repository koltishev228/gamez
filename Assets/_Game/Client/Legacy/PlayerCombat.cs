using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;
using ZombieGame.Equipment; // Чтобы знать, какое оружие в руках

namespace ZombieGame.Combat
{
    public class PlayerCombat : NetworkBehaviour
    {
        [Header("Настройки боя")]
        [Tooltip("Точка перед персонажем, где регистрируется попадание")]
        public Transform attackPoint;
        [Tooltip("Радиус поражения (нарисуется красным кругом в редакторе)")]
        public float attackRange = 1.0f;
        [Tooltip("Урон кулаками (без оружия)")]
        public float unarmedDamage = 10f;
        
        [Tooltip("По каким слоям проходит урон (например, слой Enemy)")]
        public LayerMask enemyLayer;

        [Header("Тайминги")]
        public float attackCooldown = 1f;
        private float _nextAttackTime = 0f;

        private Animator _animator;
        private WeaponManager _weaponManager;

        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            _weaponManager = GetComponent<WeaponManager>();
        }

        private void Update()
        {
            if (!base.IsOwner) return; // Только мы можем управлять своим персонажем

            // Проверяем КД (перезарядку) удара
            if (Time.time >= _nextAttackTime)
            {
                // ЛКМ (Левая кнопка мыши) через новую систему ввода
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    // ВАЖНО: Если мы кликаем по UI (Инвентарю), то не бьем!
                    if (UnityEngine.EventSystems.EventSystem.current != null && 
                        UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    {
                        return; // Выходим, не делаем удар
                    }

                    Attack();
                    _nextAttackTime = Time.time + attackCooldown;
                }
            }
        }

        private void Attack()
        {
            // 1. Включаем анимацию у нас на экране
            if (_animator != null)
            {
                _animator.SetTrigger("Attack");
            }

            // 2. Отправляем сигнал на сервер, чтобы он проверил урон и показал анимацию другим игрокам
            ServerAttack();
        }

        [ServerRpc]
        private void ServerAttack()
        {
            // Говорим остальным игрокам проиграть вашу анимацию
            ObserversAttack();

            // Проверяем физику: есть ли враги в зоне удара?
            if (attackPoint == null) return;

            Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayer);
            
            // Рассчитываем урон
            float damageToDeal = unarmedDamage;
            if (_weaponManager != null && _weaponManager.CurrentWeapon != null)
            {
                // Если в руках топор/нож, берем случайный урон между минимальным и максимальным из вашей Data-карточки
                damageToDeal = Random.Range(_weaponManager.CurrentWeapon.minDamage, _weaponManager.CurrentWeapon.maxDamage);
            }

            foreach (Collider enemy in hitEnemies)
            {
                Debug.Log($"[Server] Попали по: {enemy.name}, нанесли урон: {damageToDeal:F1}");
                
                // TODO: на Этапе 2 здесь будет вызов получения урона у Зомби:
                // enemy.GetComponent<HealthSystem>().TakeDamage(damageToDeal);
            }
        }

        [ObserversRpc(ExcludeOwner = true)]
        private void ObserversAttack()
        {
            // Эта функция срабатывает у ВСЕХ КРОМЕ ВАС, чтобы они видели, как вы бьете
            if (_animator != null)
            {
                _animator.SetTrigger("Attack");
            }
        }

        // Рисуем красный шар в редакторе Unity, чтобы вам было удобно настраивать зону удара
        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        [ContextMenu("✨ Магия: Найти AttackPoint")]
        public void AutoAssignAttackPoint()
        {
            Transform[] allTransforms = GetComponentsInChildren<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.name.ToLower().Contains("attackpoint"))
                {
                    attackPoint = t;
                    Debug.Log($"[PlayerCombat] AttackPoint автоматически привязан!");
                    return;
                }
            }
            Debug.LogWarning("[PlayerCombat] Объект со словом AttackPoint не найден внутри персонажа.");
        }
    }
}
