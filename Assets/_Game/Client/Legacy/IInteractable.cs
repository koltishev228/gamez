using UnityEngine;

namespace ZombieGame.Interaction
{
    // Интерфейс для ВСЕХ объектов, с которыми можно взаимодействовать (Лут, Двери, Машины)
    public interface IInteractable
    {
        // Какую подсказку показывать на экране (например: "Подобрать: Тушенка")
        string GetInteractionPrompt();
        
        // Что произойдет при клике
        void OnInteract(GameObject interactor);
    }
}
