using UnityEngine;
using UnityEngine.InputSystem;
using ZombieGame.Simulation;
using FishNet.Object;

namespace ZombieGame.Client
{
    public class ZomboidInput : NetworkBehaviour, IMoveInputProvider
    {
        [Header("Перетащите сюда Assets/InputSystem_Actions.inputactions")]
        public InputActionAsset InputActionAsset;

        private InputAction _moveAction;
        private InputAction _sprintAction;
        private InputAction _crouchAction;
        private InputAction _attackAction;
        private InputAction _interactAction;

        private bool _isReady = false;

        private void Start()
        {
            if (InputActionAsset == null) return;
            var map = InputActionAsset.FindActionMap("Player");
            if (map == null) return;

            map.Enable();
            _moveAction = map.FindAction("Move");
            _sprintAction = map.FindAction("Sprint");
            _crouchAction = map.FindAction("Crouch");
            _attackAction = map.FindAction("Attack");
            _interactAction = map.FindAction("Interact");

            _isReady = true;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            // Если это наш локальный персонаж — привязываем к нему камеру!
            if (base.IsOwner && CameraController.Instance != null)
            {
                CameraController.Instance.Target = this.transform;
            }
        }

        private void OnDestroy()
        {
            if (InputActionAsset != null)
            {
                var map = InputActionAsset.FindActionMap("Player");
                map?.Disable();
            }
        }

        public MoveInput GatherInput()
        {
            if (!_isReady || !base.IsOwner) return new MoveInput();

            Vector2 lookDir = Vector2.zero;
            
            // Читаем мышку (New Input System или Legacy) и проецируем на пол
            if (CameraController.Instance != null)
            {
                Vector2 mouseScreenPos;
                if (Mouse.current != null && Mouse.current.position.ReadValue() != Vector2.zero) 
                    mouseScreenPos = Mouse.current.position.ReadValue();
                else 
                    mouseScreenPos = new Vector2(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y);
                    
                // Целимся в математическую плоскость на уровне груди персонажа (Y + 1 метр),
                // чтобы компенсировать изометрический сдвиг камеры
                Vector3 worldPos = CameraController.Instance.GetCursorWorldPosition(mouseScreenPos, transform.position.y + 1f);
                
                Vector3 dir3D = worldPos - transform.position;
                if (dir3D.sqrMagnitude > 0.01f)
                {
                    lookDir = new Vector2(dir3D.x, dir3D.z).normalized;
                }
            }

            var input = new MoveInput
            {
                Dir = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero,
                Look = lookDir,
                Flags = InputFlags.None
            };

            if (_sprintAction != null && _sprintAction.IsPressed()) input.Flags |= InputFlags.Sprint;
            if (_crouchAction != null && _crouchAction.IsPressed()) input.Flags |= InputFlags.Crouch;
            if (_attackAction != null && _attackAction.IsPressed()) input.Flags |= InputFlags.Attack;
            if (_interactAction != null && _interactAction.IsPressed()) input.Flags |= InputFlags.Interact;

            return input;
        }
    }
}
