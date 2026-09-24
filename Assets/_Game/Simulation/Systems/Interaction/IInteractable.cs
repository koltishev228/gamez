using FishNet.Object;
using UnityEngine;

namespace Zomboid.Sim.Systems.Interaction
{
    /// <summary>
    /// Интерфейс для любых объектов, с которыми игрок может взаимодействовать (кнопка Е).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Вызывается НА СЕРВЕРЕ, когда игрок хочет взаимодействовать с объектом.
        /// </summary>
        void ServerInteract(PlayerInteraction interactor);

        /// <summary>
        /// Возвращает текст для UI (например: "Поднять Топор").
        /// </summary>
        string GetInteractText();
    }
}
