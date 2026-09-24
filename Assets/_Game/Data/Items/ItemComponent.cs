using System;

namespace Zomboid.Data.Items
{
    /// <summary>
    /// Базовый класс для всех компонентов предмета (например: WeaponComponent, FoodComponent).
    /// Должен быть Serializable, чтобы Unity могла сохранять его через SerializeReference.
    /// </summary>
    [Serializable]
    public abstract class ItemComponent
    {
        // Базовый класс можно расширить общими методами в будущем,
        // например: OnEquip, OnUse и т.д., если компоненты должны иметь логику.
    }
}
