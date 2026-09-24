using FishNet.Object;
using UnityEngine;
using Zomboid.Sim.Systems.Interaction;
using Zomboid.Data.Items;
using Zomboid.Core.Localization;

namespace Zomboid.Sim.Inventory
{
    /// <summary>
    /// Физический предмет, валяющийся в мире.
    /// Когда игрок нажимает E, предмет добавляется в инвентарь игрока и уничтожается в мире.
    /// </summary>
    public class WorldItem : NetworkBehaviour, IInteractable
    {
        [Tooltip("ID предмета (какой это предмет, например: item_name_axe)")]
        public string DefinitionId;

        [Tooltip("Количество в стаке (если это патроны или доски)")]
        public int Amount = 1;

        // Внутренний экземпляр, который будет передан в инвентарь
        private ItemInstance _instance;

        public override void OnStartServer()
        {
            base.OnStartServer();
            // Инициализируем данные предмета при спавне на сервере
            _instance = new ItemInstance(DefinitionId, Amount);
        }

        public void ServerInteract(PlayerInteraction interactor)
        {
            // Пытаемся положить предмет в инвентарь игрока
            if (interactor.Container.TryAddItem(_instance))
            {
                Debug.Log($"[WorldItem] {interactor.gameObject.name} picked up {DefinitionId}");
                
                // Уничтожаем объект в мире (и рассылаем клиентам команду на удаление)
                Despawn();
            }
            else
            {
                Debug.LogWarning($"[WorldItem] {interactor.gameObject.name} inventory is full!");
            }
        }

        public string GetInteractText()
        {
            // Получаем чертеж предмета для UI
            var def = DefRegistry.GetItem(DefinitionId);
            if (def != null)
            {
                string localizedName = LocalizationManager.Get(def.NameKey);
                return Amount > 1 ? $"Поднять {localizedName} (x{Amount})" : $"Поднять {localizedName}";
            }
            return "Поднять предмет";
        }
    }
}
