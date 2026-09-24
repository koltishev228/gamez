using System.Collections.Generic;
using UnityEngine;

namespace Zomboid.Data.Items
{
    /// <summary>
    /// Универсальное определение предмета. 
    /// Вместо жесткого наследования (WeaponData, FoodData) мы используем массив компонентов.
    /// </summary>
    public enum ItemCategory
    {
        None = 0,
        Weapon = 1,
        Medicine = 2,
        Food = 3,
        Craft = 4
    }

    [CreateAssetMenu(fileName = "New Item Def", menuName = "Zomboid/Items/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Base Info (ID & Localization)")]
        public string Id;
        [Tooltip("Ключ локализации для названия (например: item_name_axe)")]
        public string NameKey;
        [Tooltip("Ключ локализации для описания")]
        public string DescriptionKey;
        
        public ItemCategory Category;

        [Header("Editor Only (Для удобства в инспекторе)")]
        public string EditorName;
        [TextArea(2, 4)] public string EditorDescription;

        
        [Header("Visuals & Physics")]
        public Sprite Icon;
        public GameObject WorldPrefab;
        public float Weight = 1.0f;
        public int MaxStack = 1;

        [Header("Components")]
        // SerializeReference позволяет добавлять любые классы, наследующие ItemComponent,
        // прямо в этот список в инспекторе без создания отдельных файлов-ассетов.
        [SerializeReference]
        public List<ItemComponent> Components = new List<ItemComponent>();

        /// <summary>
        /// Возвращает первый найденный компонент нужного типа.
        /// </summary>
        public T GetComponent<T>() where T : ItemComponent
        {
            foreach (var comp in Components)
            {
                if (comp is T match)
                    return match;
            }
            return null;
        }

        public bool HasComponent<T>() where T : ItemComponent
        {
            return GetComponent<T>() != null;
        }
    }
}
