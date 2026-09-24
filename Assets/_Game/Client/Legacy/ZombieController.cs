using UnityEngine;
using UnityEngine.AI;

namespace ZombieGame.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class ZombieController : MonoBehaviour
    {
        [Header("Targeting")]
        public Transform target;
        
        [Header("Movement Settings")]
        public float walkSpeed = 1.5f; 
        public float runSpeed = 4.0f;  
        public bool isSprinter = false;

        private NavMeshAgent _agent;

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            
            _agent.speed = isSprinter ? runSpeed : walkSpeed;
            _agent.stoppingDistance = 1.2f;
            _agent.acceleration = 8f;
            _agent.radius = 0.3f;
            _agent.height = 1.8f;
        }

        private void Update()
        {
            // Так как игрок теперь спавнится по сети не сразу, 
            // зомби должен постоянно проверять, не появился ли игрок на карте
            if (target == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            if (target != null && _agent.isOnNavMesh)
            {
                _agent.SetDestination(target.position);
            }
        }
    }
}
