using UnityEngine;
using FishNet.Object;
using FishNet.Component.Animating;

namespace ZombieGame.Simulation
{
    public class PlayerAnimationNet : NetworkBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterController _cc;
        [SerializeField] private PlayerControllerNet _controller;
        [SerializeField] private NetworkAnimator _networkAnimator;

        [Header("Settings")]
        [SerializeField] private float _dampTime = 0.1f;

        private int _hashSpeedX;
        private int _hashSpeedY;
        private int _hashCrouch;

        private void Awake()
        {
            _hashSpeedX = Animator.StringToHash("SpeedX");
            _hashSpeedY = Animator.StringToHash("SpeedY");
            _hashCrouch = Animator.StringToHash("IsCrouching");

            if (_cc == null) _cc = GetComponentInParent<CharacterController>();
            if (_controller == null) _controller = GetComponentInParent<PlayerControllerNet>();
            if (_networkAnimator == null) _networkAnimator = GetComponent<NetworkAnimator>();
            if (_animator == null) _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            // Анимацию рассчитывает только владелец объекта или сервер (для ботов)
            // У остальных игроков параметры синхронизируются через NetworkAnimator
            if (!base.IsOwner && !base.IsServerInitialized) return;

            if (_animator == null || _cc == null || _controller == null) return;

            // Берем текущую скорость и убираем гравитацию
            Vector3 velocity = _cc.velocity;
            velocity.y = 0;

            // Проецируем скорость в локальные координаты (с учетом поворота)
            // Чтобы понимать, идет ли персонаж вперед, назад или стрейфится
            Vector3 localVel = transform.InverseTransformDirection(velocity);

            // Отправляем в бленд-три
            _animator.SetFloat(_hashSpeedX, localVel.x, _dampTime, Time.deltaTime);
            _animator.SetFloat(_hashSpeedY, localVel.z, _dampTime, Time.deltaTime);

            _animator.SetBool(_hashCrouch, _controller.IsCrouching);
        }
    }
}
