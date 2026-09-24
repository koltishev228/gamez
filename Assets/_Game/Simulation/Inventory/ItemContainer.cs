using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace Zomboid.Sim.Inventory
{
    /// <summary>
    /// Базовый инвентарь. Можно вешать на игрока, рюкзак, машину, ящик.
    /// Синхронизируется по сети через FishNet.
    /// </summary>
    public class ItemContainer : NetworkBehaviour
    {
        [Tooltip("Максимальный вес, который может хранить контейнер")]
        public float MaxWeight = 20f;

        [Tooltip("Максимальное количество слотов (если 0 - ограничено только весом)")]
        public int MaxSlots = 0;

        // FishNet V4: SyncList автоматически синхронизируется, атрибут больше не нужен
        public readonly SyncList<ItemInstance> NetworkItems = new SyncList<ItemInstance>();

        public IReadOnlyList<ItemInstance> Items => NetworkItems.Collection;

        public float GetCurrentWeight()
        {
            float total = 0f;
            foreach (var item in NetworkItems)
            {
                total += item.GetTotalWeight();
            }
            return total;
        }

        // ==========================================
        // ТОЛЬКО СЕРВЕР: Логика изменения инвентаря
        // ==========================================

        [Server]
        public bool TryAddItem(ItemInstance item)
        {
            if (string.IsNullOrEmpty(item.DefinitionId)) return false;

            float weight = item.GetTotalWeight();
            if (GetCurrentWeight() + weight > MaxWeight)
            {
                Debug.LogWarning($"[ItemContainer] Not enough capacity for {item.DefinitionId}");
                return false;
            }

            if (MaxSlots > 0 && NetworkItems.Count >= MaxSlots)
            {
                Debug.LogWarning("[ItemContainer] Inventory is full (slots limit)");
                return false;
            }

            // TODO: Реализовать логику стакания (сложения в одну кучу), если предмет IsStackable
            
            NetworkItems.Add(item);
            return true;
        }

        [Server]
        public bool RemoveItem(ItemInstance item)
        {
            return NetworkItems.Remove(item);
        }

        [Server]
        public void Clear()
        {
            NetworkItems.Clear();
        }
    }
}
