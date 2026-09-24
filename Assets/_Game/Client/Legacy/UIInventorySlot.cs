using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Обязательно для отслеживания кликов мыши
using ZombieGame.Data;
using ZombieGame.Inventory;

namespace ZombieGame.UI
{
    // Добавляем IPointerClickHandler, чтобы слот реагировал на мышку
    public class UIInventorySlot : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("Перетащите сюда компонент Image, который будет показывать иконку")]
        public Image iconImage;

        private ItemData _currentItem;
        private InventoryManager _inventoryManager;

        // Теперь слот запоминает, какой в нем предмет и кто его менеджер
        public void SetItem(ItemData item, InventoryManager manager)
        {
            _currentItem = item;
            _inventoryManager = manager;

            if (iconImage == null) return;

            if (item != null && item.icon != null)
            {
                iconImage.sprite = item.icon;
                iconImage.color = Color.white; 
                iconImage.enabled = true;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.color = new Color(1, 1, 1, 0); 
                iconImage.enabled = false;
            }
        }

        // Этот метод автоматически срабатывает при клике по слоту
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_currentItem != null && _inventoryManager != null)
            {
                // Передаем команду "Использовать" в менеджер
                _inventoryManager.UseItem(_currentItem);
            }
        }
    }
}
