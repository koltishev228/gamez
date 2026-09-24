using System.Collections.Generic;
using UnityEngine;

namespace Zomboid.Data.Items
{
    /// <summary>
    /// Глобальный реестр предметов.
    /// Автоматически подтягивает все ItemDefinition при инициализации.
    /// </summary>
    public static class DefRegistry
    {
        private static Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>();
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;

            // Загружаем все ScriptableObject типа ItemDefinition из любых папок Resources
            ItemDefinition[] loadedItems = Resources.LoadAll<ItemDefinition>("");
            
            _items.Clear();
            foreach (var item in loadedItems)
            {
                if (string.IsNullOrEmpty(item.Id))
                {
                    Debug.LogWarning($"[DefRegistry] ItemDefinition '{item.name}' has empty ID! Skipping.");
                    continue;
                }

                if (_items.ContainsKey(item.Id))
                {
                    Debug.LogError($"[DefRegistry] Duplicate Item ID found: '{item.Id}'! Overwriting.");
                }

                _items[item.Id] = item;
            }

            Debug.Log($"[DefRegistry] Successfully loaded {_items.Count} items.");
            _initialized = true;
        }

        public static ItemDefinition GetItem(string id)
        {
            if (!_initialized) Initialize();

            if (_items.TryGetValue(id, out var def))
            {
                return def;
            }
            
            Debug.LogError($"[DefRegistry] Item with ID '{id}' not found!");
            return null;
        }

        public static IEnumerable<ItemDefinition> GetAllItems()
        {
            if (!_initialized) Initialize();
            return _items.Values;
        }
    }
}
