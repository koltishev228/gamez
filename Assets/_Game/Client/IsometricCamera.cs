using UnityEngine;
using UnityEngine.InputSystem;

namespace ZombieGame.CameraSystem
{
    public class IsometricCamera : MonoBehaviour
    {
        [Header("Target Setup")]
        public Transform target;
        [Tooltip("Плавность следования камеры")]
        public float smoothSpeed = 8f;

        [Header("Camera Angles (Isometric)")]
        [Tooltip("Наклон камеры вниз (X)")]
        public float pitch = 45f;
        [Tooltip("Поворот камеры вокруг персонажа (Y)")]
        public float yaw = 45f;

        [Header("Zoom Settings")]
        public float baseDistance = 12f;
        public float minDistance = 5f;
        public float maxDistance = 20f;
        public float zoomSpeed = 1.5f;
        
        [Header("Aim Panning")]
        public float maxAimPanDistance = 6f;
        public float aimPanMultiplier = 0.35f;
        public float panSmoothSpeed = 3.5f;

        private UnityEngine.Camera _cam;
        private float _currentDistance;
        private Vector3 _currentPanOffset = Vector3.zero;

        private void OnEnable()
        {
            ZombieGame.Simulation.PlayerControllerNet.OnLocalPlayerSpawned += SetTarget;
        }

        private void OnDisable()
        {
            ZombieGame.Simulation.PlayerControllerNet.OnLocalPlayerSpawned -= SetTarget;
        }

        private void SetTarget(Transform newTarget)
        {
            target = newTarget;
            Debug.Log($"[IsometricCamera] Автоматически привязана к локальному игроку {newTarget.name}");
        }

        private void Start()
        {
            _cam = GetComponent<UnityEngine.Camera>();
            _currentDistance = baseDistance;

            // Сужаем FOV для эффекта классической изометрии (менее искаженная перспектива)
            if (!_cam.orthographic && _cam.fieldOfView > 35f)
            {
                _cam.fieldOfView = 30f; 
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            HandleZoom();

            // 1. Фиксированный изометрический поворот
            Quaternion camRotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.rotation = camRotation;

            // 2. Смещение при прицеливании (зажата ПКМ)
            Vector3 targetPanOffset = GetAimPanOffset();
            _currentPanOffset = Vector3.Lerp(_currentPanOffset, targetPanOffset, panSmoothSpeed * Time.deltaTime);

            // 3. Вычисляем нужную позицию: фокусная точка минус луч назад на текущую дистанцию
            Vector3 focusPoint = target.position + _currentPanOffset;
            Vector3 desiredPosition = focusPoint - (camRotation * Vector3.forward * _currentDistance);

            // 4. Плавно двигаем камеру
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        private void HandleZoom()
        {
            if (Mouse.current != null)
            {
                float scrollDelta = Mouse.current.scroll.ReadValue().y;
                if (scrollDelta > 0)
                    _currentDistance -= zoomSpeed;
                else if (scrollDelta < 0)
                    _currentDistance += zoomSpeed;
                
                _currentDistance = Mathf.Clamp(_currentDistance, minDistance, maxDistance);
            }
        }

        private Vector3 GetAimPanOffset()
        {
            if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            {
                Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                Plane groundPlane = new Plane(Vector3.up, new Vector3(0, target.position.y, 0));

                if (groundPlane.Raycast(ray, out float enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    Vector3 dirToMouse = hitPoint - target.position;
                    dirToMouse.y = 0f; 

                    Vector3 panOffset = dirToMouse * aimPanMultiplier;
                    if (panOffset.magnitude > maxAimPanDistance)
                    {
                        panOffset = panOffset.normalized * maxAimPanDistance;
                    }
                    return panOffset;
                }
            }
            return Vector3.zero;
        }
    }
}
