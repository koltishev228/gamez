using UnityEngine;
using ZombieGame.Simulation; 

namespace Zomboid.Simulation.World
{
    [RequireComponent(typeof(BoxCollider))]
    public class WindowPeekTrigger : MonoBehaviour
    {
        public Building TargetBuilding;
        public float PeekAngle = 60f;

        private PlayerControllerNet _localPlayerInZone;
        private bool _isCurrentlyPeeking = false;

        private void Start()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            if (TargetBuilding == null) 
                TargetBuilding = GetComponentInParent<Building>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<PlayerControllerNet>();
            if (player != null && player.IsOwner)
            {
                _localPlayerInZone = player;
                SetPeekingState(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var player = other.GetComponentInParent<PlayerControllerNet>();
            if (player != null && player == _localPlayerInZone)
            {
                _localPlayerInZone = null;
                SetPeekingState(false);
            }
        }

        private void SetPeekingState(bool state)
        {
            if (_isCurrentlyPeeking == state) return;
            
            _isCurrentlyPeeking = state;
            if (_isCurrentlyPeeking)
            {
                TargetBuilding.AddPeeker();
                Debug.Log($"[Окно] Игрок подошел к окну. Скрываем крышу!");
            }
            else
            {
                TargetBuilding.RemovePeeker();
                Debug.Log($"[Окно] Игрок отошел от окна.");
            }
        }

        private void OnDrawGizmos()
        {
            var col = GetComponent<BoxCollider>();
            if (col == null) return;

            Gizmos.color = _isCurrentlyPeeking ? new Color(0, 1f, 0, 0.4f) : new Color(0, 0.5f, 1f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);
            Gizmos.DrawWireCube(col.center, col.size);
        }
    }
}
