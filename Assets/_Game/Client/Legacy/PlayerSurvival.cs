using UnityEngine;
using UnityEngine.UI;
using ZombieGame.Data;
using FishNet.Object;

namespace ZombieGame.Health
{
    // Отвечает за Голод, Жажду и применение еды/лекарств
    public class PlayerSurvival : NetworkBehaviour
    {
        [Header("Показатели (0 - отлично, 100 - смерть)")]
        public float hunger = 0f;
        public float thirst = 0f;

        [Header("Скорость роста в секунду")]
        public float hungerRate = 0.5f; 
        public float thirstRate = 0.8f;

        [Header("UI Элементы (Перетащите сюда Slider или Image)")]
        [Tooltip("Если используете UI Slider")]
        public Slider hungerSlider;
        public Slider thirstSlider;
        public Slider healthSlider; // Для вывода общего ХП из HealthSystem
        
        [Tooltip("Если используете Image с типом Filled")]
        public Image hungerFillImage;
        public Image thirstFillImage;
        public Image healthFillImage;

        private HealthSystem _healthSystem;

        private void Start()
        {
            _healthSystem = GetComponent<HealthSystem>();
        }

        private void Update()
        {
            if (!base.IsOwner) return; // Считаем только для локального игрока

            // Со временем персонаж хочет есть и пить
            hunger += hungerRate * Time.deltaTime;
            thirst += thirstRate * Time.deltaTime;

            // Ограничиваем до 100
            hunger = Mathf.Clamp(hunger, 0, 100);
            thirst = Mathf.Clamp(thirst, 0, 100);

            // Если голод или жажда на 100, персонаж начинает терять ХП (можно добавить позже)
            // if (hunger >= 100 || thirst >= 100) _healthSystem.ReceiveDamage(...);

            UpdateUI();
        }

        // Метод поедания предмета
        public void Consume(ConsumableData food)
        {
            if (food == null) return;

            // Еда утоляет голод (поэтому минус)
            hunger += food.hungerChange;
            thirst += food.thirstChange;

            hunger = Mathf.Clamp(hunger, 0, 100);
            thirst = Mathf.Clamp(thirst, 0, 100);

            Debug.Log($"[Survival] Съедено: {food.itemName}. Голод: {hunger:F1}/100, Жажда: {thirst:F1}/100");
            
            UpdateUI();
        }

        private void UpdateUI()
        {
            // Обновляем Слайдеры (если они есть)
            // Мы делим на 100, так как стандартный fillAmount идет от 0 до 1
            if (hungerSlider != null) hungerSlider.value = hunger / 100f;
            if (thirstSlider != null) thirstSlider.value = thirst / 100f;

            if (_healthSystem != null && healthSlider != null)
            {
                healthSlider.value = _healthSystem.overallHealth / 100f;
            }

            // Обновляем Картинки (если вы используете Image.FillAmount вместо Slider)
            if (hungerFillImage != null) hungerFillImage.fillAmount = hunger / 100f;
            if (thirstFillImage != null) thirstFillImage.fillAmount = thirst / 100f;
            
            if (_healthSystem != null && healthFillImage != null)
            {
                healthFillImage.fillAmount = _healthSystem.overallHealth / 100f;
            }
        }
    }
}
