using UnityEngine;
using System.Collections.Generic;
using ZombieGame.Data;
using FishNet.Object;

namespace ZombieGame.Equipment
{
    // Enterprise подход: отдельный менеджер, который логически хранит то, что НАДЕТО на персонажа.
    // Считает итоговую защиту от укусов и броню.
    public class EquipmentManager : NetworkBehaviour
    {
        // Словарь: Слот тела -> Надетый предмет
        private Dictionary<EquipSlot, ClothingData> equippedItems = new Dictionary<EquipSlot, ClothingData>();

        [Header("Итоговые характеристики (Только для чтения)")]
        public float totalBiteDefense = 0f;
        public float totalScratchDefense = 0f;
        public float totalInsulation = 0f;
        public float currentSpeedModifier = 1.0f; // Перемножение всех штрафов к бегу

        // Надеваем вещь
        public void Equip(ClothingData clothing)
        {
            if (clothing == null) return;

            // Если слот уже занят (например, на Торсе уже есть рубашка), сначала снимаем её
            if (equippedItems.ContainsKey(clothing.equipSlot))
            {
                Unequip(clothing.equipSlot);
            }

            equippedItems[clothing.equipSlot] = clothing;
            Debug.Log($"[EquipmentManager] Надето в слот {clothing.equipSlot}: {clothing.itemName}");

            RecalculateStats();

            // Передаем команду визуализатору (нашему ClothingManager), чтобы он прикрепил 3D модель к костям
            ClothingManager visualManager = GetComponent<ClothingManager>();
            if (visualManager != null && clothing.prefab != null)
            {
                visualManager.EquipClothing(clothing.prefab);
            }
        }

        // Снимаем вещь
        public void Unequip(EquipSlot slot)
        {
            if (equippedItems.TryGetValue(slot, out ClothingData item))
            {
                Debug.Log($"[EquipmentManager] Снято из слота {slot}: {item.itemName}");
                equippedItems.Remove(slot);
                
                RecalculateStats();

                // Позже добавим команду в ClothingManager на удаление 3D-модели
            }
        }

        // Перерасчет характеристик выживания
        private void RecalculateStats()
        {
            totalBiteDefense = 0f;
            totalScratchDefense = 0f;
            totalInsulation = 0f;
            currentSpeedModifier = 1.0f;

            foreach (var kvp in equippedItems)
            {
                ClothingData item = kvp.Value;
                totalBiteDefense += item.biteDefense;
                totalScratchDefense += item.scratchDefense;
                totalInsulation += item.insulation;
                currentSpeedModifier *= item.runSpeedModifier; // Штрафы перемножаются
            }

            Debug.Log($"Характеристики обновлены! Укус: {totalBiteDefense}%, Царапины: {totalScratchDefense}%, Скорость: x{currentSpeedModifier}");
        }

        // Вернуть надетый предмет в слоте
        public ClothingData GetItemInSlot(EquipSlot slot)
        {
            if (equippedItems.TryGetValue(slot, out ClothingData item))
            {
                return item;
            }
            return null;
        }
    }
}
