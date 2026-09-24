using UnityEngine;
using Zomboid.Sim.Inventory;
using FishNet.Object;
using System.Collections.Generic;
using FishNet.Object.Synchronizing;

namespace Zomboid.Client.UI.Inventory
{
    /// <summary>
    /// Главное окно инвентаря игрока.
    /// Слушает события изменения коллекции (SyncList) и перерисовывает слоты.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Tooltip("Контейнер для слотов (например, Content в ScrollView)")]
        public Transform SlotsContainer;
        
        [Tooltip("Префаб одного слота инвентаря")]
        public InventorySlotUI SlotPrefab;

        private ItemContainer _targetContainer;
        private readonly List<InventorySlotUI> _spawnedSlots = new List<InventorySlotUI>();

        private void OnEnable()
        {
            // Подписываемся на спавн локального игрока
            ZombieGame.Simulation.PlayerControllerNet.OnLocalPlayerSpawned += OnPlayerSpawned;
            
            // На случай, если игрок УЖЕ заспавнился до включения UI
            BindToLocalPlayer();
        }

        private void OnDisable()
        {
            ZombieGame.Simulation.PlayerControllerNet.OnLocalPlayerSpawned -= OnPlayerSpawned;
            Unbind();
        }

        private void OnPlayerSpawned(Transform playerTransform)
        {
            var container = playerTransform.GetComponent<ItemContainer>();
            if (container != null)
            {
                Bind(container);
            }
        }

        private void BindToLocalPlayer()
        {
            if (FishNet.InstanceFinder.ClientManager == null || FishNet.InstanceFinder.ClientManager.Connection == null) return;
            var localPlayer = FishNet.InstanceFinder.ClientManager.Connection.FirstObject;
            if (localPlayer != null)
            {
                var container = localPlayer.GetComponent<ItemContainer>();
                if (container != null)
                {
                    Bind(container);
                }
            }
        }

        public void Bind(ItemContainer container)
        {
            if (_targetContainer == container) return;

            Unbind();

            _targetContainer = container;
            if (_targetContainer != null)
            {
                // Подписываемся на изменения инвентаря (добавили вещь, убрали вещь)
                _targetContainer.NetworkItems.OnChange += OnInventoryChanged;
                
                // Рисуем то, что уже есть
                RefreshAll();
            }
        }

        public void Unbind()
        {
            if (_targetContainer != null)
            {
                _targetContainer.NetworkItems.OnChange -= OnInventoryChanged;
                _targetContainer = null;
            }
            ClearSlots();
        }

        private Zomboid.Data.Items.ItemCategory _currentFilter = Zomboid.Data.Items.ItemCategory.None;

        public void SetFilter(Zomboid.Data.Items.ItemCategory category)
        {
            _currentFilter = category;
            RefreshAll();
        }

        public void SetFilterInt(int categoryInt)
        {
            SetFilter((Zomboid.Data.Items.ItemCategory)categoryInt);
        }

        // Вызывается каждый раз, когда сервер меняет SyncList
        private void OnInventoryChanged(SyncListOperation op, int index, ItemInstance oldItem, ItemInstance newItem, bool asServer)
        {
            if (asServer) return; // UI реагирует только на клиентские события

            RefreshAll();
        }

        private void RefreshAll()
        {
            ClearSlots();

            if (_targetContainer == null) return;

            foreach (var item in _targetContainer.Items)
            {
                var def = item.GetDefinition();
                if (def == null) continue;

                // Фильтрация
                if (_currentFilter != Zomboid.Data.Items.ItemCategory.None && def.Category != _currentFilter)
                    continue;

                var slot = Instantiate(SlotPrefab, SlotsContainer);
                slot.Bind(item);
                _spawnedSlots.Add(slot);
            }
        }

        private void ClearSlots()
        {
            foreach (var slot in _spawnedSlots)
            {
                if (slot != null) Destroy(slot.gameObject);
            }
            _spawnedSlots.Clear();
        }
    }
}
