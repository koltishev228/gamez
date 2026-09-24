using UnityEngine;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;

namespace ZombieGame.Simulation
{
    public struct ReconcileData : IReconcileData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float VerticalVelocity;

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;

        public ReconcileData(Vector3 pos, Quaternion rot, float yVel)
        {
            Position = pos;
            Rotation = rot;
            VerticalVelocity = yVel;
            _tick = 0;
        }
    }

    [RequireComponent(typeof(CharacterController))]
    public class PlayerControllerNet : NetworkBehaviour
    {
        [Header("Movement Settings")]
        public float MoveSpeed = 4f;
        public float SprintSpeed = 7f;
        public float CrouchSpeed = 2f;
        public float RotationSpeed = 15f;
        public float Gravity = -9.81f;

        private CharacterController _cc;
        private IMoveInputProvider _inputProvider;
        private float _verticalVelocity;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _inputProvider = GetComponent<IMoveInputProvider>();
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            TimeManager.OnTick += TimeManager_OnTick;
        }

        public static event System.Action<Transform> OnLocalPlayerSpawned;

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (base.IsOwner)
            {
                OnLocalPlayerSpawned?.Invoke(this.transform);
            }
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            if (TimeManager != null)
            {
                TimeManager.OnTick -= TimeManager_OnTick;
            }
        }

        private void TimeManager_OnTick()
        {
            MoveInput input = _inputProvider != null ? _inputProvider.GatherInput() : default;

            Move(input, FishNet.Object.Prediction.ReplicateState.Invalid, FishNet.Transporting.Channel.Unreliable);
            
            // В FishNet V4 и клиент, и сервер должны сохранять откат!
            CreateReconcile();
        }

        public override void CreateReconcile()
        {
            ReconcileData rd = new ReconcileData(transform.position, transform.rotation, _verticalVelocity);
            Reconciliation(rd, FishNet.Transporting.Channel.Unreliable);
        }

        public bool IsCrouching { get; private set; }

        [Replicate]
        private void Move(MoveInput input, FishNet.Object.Prediction.ReplicateState state, FishNet.Transporting.Channel channel)
        {
            IsCrouching = (input.Flags & InputFlags.Crouch) != 0;
            
            float speed = MoveSpeed;
            if ((input.Flags & InputFlags.Sprint) != 0) speed = SprintSpeed;
            if (IsCrouching) speed = CrouchSpeed;
            
            Vector3 moveDir = new Vector3(input.Dir.x, 0f, input.Dir.y).normalized;

            if (_cc.isGrounded)
            {
                _verticalVelocity = -1f; 
            }
            else
            {
                _verticalVelocity += Gravity * (float)TimeManager.TickDelta;
            }

            Vector3 velocity = moveDir * speed;
            velocity.y = _verticalVelocity;

            _cc.Move(velocity * (float)TimeManager.TickDelta);

            if (input.Look != Vector2.zero)
            {
                Vector3 look3D = new Vector3(input.Look.x, 0f, input.Look.y);
                Quaternion targetRot = Quaternion.LookRotation(look3D);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotationSpeed * (float)TimeManager.TickDelta);
            }
            else if (moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotationSpeed * (float)TimeManager.TickDelta);
            }
        }

        [Reconcile]
        private void Reconciliation(ReconcileData rd, FishNet.Transporting.Channel channel)
        {
            transform.position = rd.Position;
            transform.rotation = rd.Rotation;
            _verticalVelocity = rd.VerticalVelocity;
        }
    }
}
