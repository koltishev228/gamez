using UnityEngine;
using FishNet.Object;
using System.Collections.Generic;
using ZombieGame.UI;
using ZombieGame.Data; // Подключаем базу данных
using UnityEngine.InputSystem;

namespace ZombieGame.Inventory
{
    public class InventoryManager : NetworkBehaviour
    {
        [Header("UI Ссылки")]
        [Tooltip("Перетащите сюда ваш префаб ячейки инвентаря")]
        public GameObject slotPrefab; 
        
        [Tooltip("Временный файл ItemData для теста по кнопке I")]
        public ItemData testItem;

        private Transform _itemsGridParent;
        // Теперь инвентарь хранит полноценные предметы, а не просто картинки!
        private List<ItemData> _items = new List<ItemData>(); 

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!base.IsOwner) return;

            Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            foreach(Transform t in allTransforms)
            {
                if (t.name == "ItemsGrid" && t.gameObject.scene.IsValid())
                {
                    _itemsGridParent = t;
                    foreach (Transform child in _itemsGridParent)
                    {
                        Destroy(child.gameObject);
                    }
                    break;
                }
            }
        }

        private void Update()
        {
            if (!base.IsOwner) return;

            if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
            {
                if (testItem != null)
                {
                    AddItem(testItem);
                }
                else
                {
                    Debug.LogWarning("Вы не назначили Test Item в скрипте InventoryManager!");
                }
            }
        }

        [Header("Физика инвентаря")]
        public float maxWeightCapacity = 15.0f; // Максимальный вес в кг
        public float currentWeight = 0f; // Текущий вес
        
        [Tooltip("Перетащите сюда текстовый объект UI, который будет показывать вес")]
        public TMPro.TextMeshProUGUI weightText;

        public void AddItem(ItemData item)
        {
            // Проверка перегруза перед поднятием
            if (currentWeight + item.weight > maxWeightCapacity)
            {
                Debug.LogWarning($"Перегруз! Вы не можете поднять {item.itemName}. Освободите место.");
                return; 
            }

            _items.Add(item);
            RecalculateWeight();
            
            Debug.Log($"Подобран предмет: {item.itemName}. Вес рюкзака: {currentWeight:F1} / {maxWeightCapacity:F1} кг");
            RedrawUI();
        }

        public void RemoveItem(ItemData item)
        {
            if (_items.Contains(item))
            {
                _items.Remove(item);
                RecalculateWeight();
                RedrawUI();
            }
        }

        // Новый метод: Использование предмета при клике в инвентаре
        public void UseItem(ItemData item)
        {
            // Если это одежда
            if (item is ClothingData clothing)
            {
                Equipment.EquipmentManager equipmentManager = GetComponent<Equipment.EquipmentManager>();
                if (equipmentManager != null)
                {
                    equipmentManager.Equip(clothing);
                    RemoveItem(item);
                }
                else
                {
                    Debug.LogWarning("На игроке не найден скрипт EquipmentManager!");
                }
            }
            // Если это ОРУЖИЕ
            else if (item is WeaponData weapon)
            {
                Equipment.WeaponManager weaponManager = GetComponent<Equipment.WeaponManager>();
                if (weaponManager != null)
                {
                    weaponManager.EquipWeapon(weapon);
                    // Временно не удаляем оружие из рюкзака (чтобы его можно было снять/выбросить позже)
                    // Но оно физически появляется в руке!
                }
                else
                {
                    Debug.LogWarning("На игроке не найден скрипт WeaponManager!");
                }
            }
            // Если это еда или лекарство
            else if (item is ConsumableData food)
            {
                Health.PlayerSurvival survival = GetComponent<Health.PlayerSurvival>();
                if (survival != null)
                {
                    survival.Consume(food); // Персонаж съедает предмет (меняется голод/жажда)
                    RemoveItem(item);       // Предмет исчезает из инвентаря
                }
                else
                {
                    Debug.LogWarning("На игроке не найден скрипт PlayerSurvival!");
                }
            }
            else
            {
                Debug.Log($"Изучаем предмет: {item.itemName}");
            }
        }

        private void RecalculateWeight()
        {
            currentWeight = 0f;
            foreach (var item in _items)
            {
                currentWeight += item.weight;
            }
            
            if (weightText != null)
            {
                weightText.text = $"Вес: {currentWeight:F1} / {maxWeightCapacity:F1} кг";
            }
        }

        private void RedrawUI()
        {
            if (_itemsGridParent == null || slotPrefab == null) return;

            foreach (Transform child in _itemsGridParent)
            {
                Destroy(child.gameObject);
            }

            foreach (ItemData item in _items)
            {
                GameObject newSlotObj = Instantiate(slotPrefab, _itemsGridParent);
                UIInventorySlot slot = newSlotObj.GetComponent<UIInventorySlot>();
                if (slot != null)
                {
                    // Передаем сам предмет И ссылку на текущий InventoryManager
                    slot.SetItem(item, this);
                }
            }
        }
    }
}
