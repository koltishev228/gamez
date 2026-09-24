using UnityEngine;
using UnityEngine.InputSystem;
using ZombieGame.Inventory;
using FishNet.Object;

namespace ZombieGame.Interaction
{
    public class PlayerInteractor : NetworkBehaviour
    {
        [Header("Настройки подбора")]
        public float pickupRadius = 2.0f; // Как близко нужно подойти
        public LayerMask itemLayer; // Слой, на котором лежат предметы (опционально)

        private InventoryManager _inventory;

        private void Start()
        {
            _inventory = GetComponent<InventoryManager>();
        }

        private void Update()
        {
            if (!base.IsOwner) return;

            // Нажимаем кнопку E на клавиатуре
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryPickupNearbyItem();
            }
        }

        private void TryPickupNearbyItem()
        {
            // Ищем все коллайдеры вокруг игрока
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupRadius);

            foreach (var col in hitColliders)
            {
                PickupItem item = col.GetComponent<PickupItem>();
                if (item != null)
                {
                    // Если нашли предмет - кладем в рюкзак и удаляем с земли
                    if (_inventory != null && item.itemData != null)
                    {
                        _inventory.AddItem(item.itemData);
                        item.ServerDespawnItem();
                        Debug.Log($"[Interactor] Подобрали предмет: {item.itemData.itemName}");
                        return; // Берем только один предмет за раз
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}
