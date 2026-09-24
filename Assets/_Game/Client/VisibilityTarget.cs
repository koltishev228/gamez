using UnityEngine;
using System.Collections.Generic;

namespace ZombieGame.Visibility
{
    public class VisibilityTarget : MonoBehaviour
    {
        // Статический список всех объектов на сцене, которые нужно скрывать
        public static List<VisibilityTarget> AllTargets = new List<VisibilityTarget>();
        
        private Renderer[] _renderers;
        public bool IsVisible { get; private set; } = true;

        private void Awake()
        {
            // Находим все меши (кожу, одежду, расчлененку) на этом зомби
            _renderers = GetComponentsInChildren<Renderer>();
        }

        private void OnEnable() => AllTargets.Add(this);
        private void OnDisable() => AllTargets.Remove(this);

        public void SetVisible(bool visible)
        {
            // Чтобы не дергать рендеры каждый кадр, меняем только если статус изменился
            if (IsVisible == visible) return; 
            
            IsVisible = visible;
            
            foreach (var r in _renderers)
            {
                if (r != null) r.enabled = visible;
            }
        }
    }
}
