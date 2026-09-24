using System;
using UnityEngine;
using Zomboid.Data.Items;

namespace Zomboid.Sim.Inventory
{
    /// <summary>
    /// Конкретный физический предмет в мире или инвентаре.
    /// Хранит свое уникальное состояние (ID в базе, текущая прочность и т.д.).
    /// </summary>
    [Serializable]
    public struct ItemInstance
    {
        public string InstanceId;
        public string DefinitionId;
        public float CurrentDurability;
        public int CurrentStackSize;
        public bool IsBloody;
        public string CustomData;

        public ItemInstance(string definitionId, int stackSize = 1)
        {
            InstanceId = Guid.NewGuid().ToString();
            DefinitionId = definitionId;
            CurrentDurability = 100f;
            CurrentStackSize = stackSize;
            IsBloody = false;
            CustomData = "";
        }

        public ItemDefinition GetDefinition()
        {
            return DefRegistry.GetItem(DefinitionId);
        }

        public float GetTotalWeight()
        {
            var def = GetDefinition();
            return def != null ? def.Weight * CurrentStackSize : 0f;
        }
    }
}
