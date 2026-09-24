using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zomboid.Data.Items;
using Zomboid.Core.Localization;
using Zomboid.Sim.Inventory;

namespace Zomboid.Client.UI.Inventory
{
    /// <summary>
    /// Скрипт визуального слота в инвентаре (иконка, название, вес).
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        public Image IconImage;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI WeightText;
        public TextMeshProUGUI AmountText;

        public void Bind(ItemInstance instance)
        {
            var def = instance.GetDefinition();
            if (def == null) return;

            // Иконка
            if (def.Icon != null)
            {
                IconImage.sprite = def.Icon;
                IconImage.enabled = true;
            }
            else
            {
                IconImage.enabled = false;
            }

            // Название (через локализацию)
            NameText.text = LocalizationManager.Get(def.NameKey);

            // Вес
            WeightText.text = $"{instance.GetTotalWeight():F1} кг";

            // Количество (если стакается)
            if (instance.CurrentStackSize > 1)
            {
                AmountText.text = $"x{instance.CurrentStackSize}";
                AmountText.gameObject.SetActive(true);
            }
            else
            {
                AmountText.gameObject.SetActive(false);
            }
        }
    }
}
