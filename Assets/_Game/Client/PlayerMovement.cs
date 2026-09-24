using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;
using ZombieGame.CameraSystem; // Для доступа к камере

namespace ZombieGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float rotationSpeed = 15f;
        
        private CharacterController _controller;
        private UnityEngine.Camera _cam;
        private Animator _animator;

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
        }

        // Этот метод вызывается FishNet'ом ТОЛЬКО когда мы подключились к серверу
        public override void OnStartClient()
        {
            base.OnStartClient();

            // Если этот персонаж - НАШ
            if (base.IsOwner)
            {
                _cam = UnityEngine.Camera.main;
                
                // Находим скрипт камеры на сцене и говорим ему следить за нами
                if (_cam != null)
                {
                    IsometricCamera isoCam = _cam.GetComponent<IsometricCamera>();
                    if (isoCam != null)
                    {
                        isoCam.target = this.transform;
                    }
                }
            }
        }

        private void Update()
        {
            if (!base.IsOwner) return;

            HandleMovementAndRotation();
        }

        private void HandleMovementAndRotation()
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) vertical += 1f;
                if (Keyboard.current.sKey.isPressed) vertical -= 1f;
                if (Keyboard.current.aKey.isPressed) horizontal -= 1f;
                if (Keyboard.current.dKey.isPressed) horizontal += 1f;
            }

            Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
            Vector3 moveDir = Vector3.zero;

            if (inputDir.magnitude >= 0.1f)
            {
                moveDir = inputDir;

                if (_cam != null)
                {
                    Vector3 camForward = _cam.transform.forward;
                    camForward.y = 0f;
                    camForward.Normalize();

                    Vector3 camRight = _cam.transform.right;
                    camRight.y = 0f;
                    camRight.Normalize();

                    moveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;
                }
            }

            if (_animator != null)
            {
                _animator.SetFloat("Speed", inputDir.magnitude);
            }

            Vector3 finalMove = moveDir * moveSpeed;
            if (!_controller.isGrounded) finalMove.y = -9.81f;
            
            _controller.Move(finalMove * Time.deltaTime);

            bool isAiming = Mouse.current != null && Mouse.current.rightButton.isPressed;

            if (isAiming)
            {
                HandleMouseAim();
            }
            else if (moveDir.sqrMagnitude > 0.05f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        private void HandleMouseAim()
        {
            if (_cam == null || Mouse.current == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = _cam.ScreenPointToRay(mousePos);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3 lookDirection = hitPoint - transform.position;
                lookDirection.y = 0f; 

                if (lookDirection.sqrMagnitude > 0.05f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * 2f * Time.deltaTime);
                }
            }
        }
    }
}
