using UnityEngine;
using FishNet.Object;
using ZombieGame.Inventory;
using ZombieGame.Data; // Подключаем базу данных предметов

namespace ZombieGame.Interaction
{
    public class PickupItem : NetworkBehaviour
    {
        [Header("База данных предмета")]
        [Tooltip("Вставьте сюда файл ItemData (например, ваш Box)")]
        public ItemData itemData;

        private void OnMouseDown()
        {
            if (!base.IsClient) return;

            InventoryManager localInventory = null;
            InventoryManager[] allInventories = FindObjectsOfType<InventoryManager>();
            
            foreach (var inv in allInventories)
            {
                if (inv.IsOwner) 
                {
                    localInventory = inv;
                    break;
                }
            }

            if (localInventory != null)
            {
                if (itemData != null)
                {
                    localInventory.AddItem(itemData);
                    ServerDespawnItem();
                }
                else
                {
                    Debug.LogWarning("У этого предмета на земле не назначен файл ItemData!");
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ServerDespawnItem()
        {
            base.ServerManager.Despawn(gameObject);
        }
    }
}
