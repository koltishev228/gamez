using UnityEngine;
using UnityEngine.InputSystem;

namespace ZombieGame.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Панели Интерфейса")]
        public GameObject inventoryPanel;
        public GameObject healthPanel;
        public GameObject skillsPanel;
        public GameObject craftingPanel;

        [Header("Управление мышью")]
        public bool isAnyPanelOpen = false;

        private void Update()
        {
            if (Keyboard.current == null) return;

            // Кнопка I или TAB - Инвентарь
            if (Keyboard.current.iKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)
            {
                TogglePanel(inventoryPanel);
            }

            // Кнопка H - Здоровье (Медицина)
            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                TogglePanel(healthPanel);
            }

            // Кнопка K или U - Прокачка (Навыки)
            if (Keyboard.current.kKey.wasPressedThisFrame || Keyboard.current.uKey.wasPressedThisFrame)
            {
                TogglePanel(skillsPanel);
            }

            // Кнопка B или C - Крафт (Создание)
            if (Keyboard.current.bKey.wasPressedThisFrame || Keyboard.current.cKey.wasPressedThisFrame)
            {
                TogglePanel(craftingPanel);
            }
        }

        private void TogglePanel(GameObject panel)
        {
            if (panel == null) return;

            bool isActive = panel.activeSelf;
            
            // Если мы открываем панель, скрываем все остальные (по желанию можно убрать)
            if (!isActive)
            {
                CloseAllPanels();
            }

            panel.SetActive(!isActive);
            CheckPanelsState();
        }

        public void CloseAllPanels()
        {
            if (inventoryPanel) inventoryPanel.SetActive(false);
            if (healthPanel) healthPanel.SetActive(false);
            if (skillsPanel) skillsPanel.SetActive(false);
            if (craftingPanel) craftingPanel.SetActive(false);
            CheckPanelsState();
        }

        private void CheckPanelsState()
        {
            // Проверяем, открыто ли хоть одно окно
            isAnyPanelOpen = (inventoryPanel != null && inventoryPanel.activeSelf) ||
                             (healthPanel != null && healthPanel.activeSelf) ||
                             (skillsPanel != null && skillsPanel.activeSelf) ||
                             (craftingPanel != null && craftingPanel.activeSelf);

            // Если открыто окно - освобождаем курсор мыши, чтобы можно было кликать по кнопкам
            if (isAnyPanelOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // Если все закрыто - прячем курсор (если игра требует прицеливания)
                // Для Zomboid курсор обычно всегда виден, поэтому можете закомментировать эти строки:
                // Cursor.lockState = CursorLockMode.Locked; 
                // Cursor.visible = false;
            }
        }
    }
}
