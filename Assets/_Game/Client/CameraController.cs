using UnityEngine;

namespace ZombieGame.Client
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        public Transform Target;
        
        [Header("Settings (Project Zomboid Style)")]
        public float Distance = 15f;
        public float Pitch = 50f;
        public float Yaw = 45f;
        public float SmoothTime = 0.1f;
        
        [Header("Zoom Settings")]
        public float MinDistance = 5f;
        public float MaxDistance = 30f;
        public float ZoomSpeed = 2f;
        
        private Vector3 _currentVelocity;
        private Camera _cam;

        private void Awake()
        {
            Instance = this;
            _cam = GetComponent<Camera>();

            // Настраиваем узкий FOV (как в диздоке) для псевдо-изометрии
            if (!_cam.orthographic)
            {
                _cam.fieldOfView = 25f;
            }
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                float scroll = UnityEngine.InputSystem.Mouse.current.scroll.ReadValue().y;
                if (scroll > 0) Distance -= ZoomSpeed;
                else if (scroll < 0) Distance += ZoomSpeed;
                
                Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);
            }
        }

        private void LateUpdate()
        {
            if (Target == null) return;

            // 1. Изометрический угол
            transform.rotation = Quaternion.Euler(Pitch, Yaw, 0f);

            // 2. Идеальная позиция (от цели назад по взгляду камеры)
            Vector3 desiredPos = Target.position - transform.forward * Distance;

            // 3. Жестко следуем за целью (цель уже сглаживается через NetworkTickSmoother)
            transform.position = desiredPos;
        }

        // Математическое пересечение луча с полом без коллайдеров
        public Vector3 GetCursorWorldPosition(Vector2 screenPosition, float floorHeight = 0f)
        {
            if (_cam == null) return Vector3.zero;

            Ray ray = _cam.ScreenPointToRay(screenPosition);
            
            if (Mathf.Abs(ray.direction.y) < 0.001f) return Vector3.zero;
            
            float distance = (floorHeight - ray.origin.y) / ray.direction.y;
            if (distance < 0) return Vector3.zero;
            
            return ray.origin + ray.direction * distance;
        }
    }
}
