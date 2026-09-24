using FishNet.Object;
using UnityEngine;
using Zomboid.Sim.Inventory;

namespace Zomboid.Sim.Systems.Interaction
{
    /// <summary>
    /// Компонент игрока для взаимодействия с миром (подбор предметов, открытие дверей).
    /// </summary>
    [RequireComponent(typeof(ItemContainer))]
    public class PlayerInteraction : NetworkBehaviour
    {
        [Tooltip("Радиус взаимодействия")]
        public float InteractRange = 2f;
        
        public ItemContainer Container { get; private set; }

        private void Awake()
        {
            Container = GetComponent<ItemContainer>();
        }

        private void Update()
        {
            if (!IsOwner) return;

            // Используем новую систему ввода (Keyboard.current), так как старая скорее всего отключена
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryInteract();
            }
        }

        [Client]
        private void TryInteract()
        {
            // Ищем ближайший интерактивный объект
            Collider[] hits = Physics.OverlapSphere(transform.position, InteractRange);
            
            float closestDist = float.MaxValue;
            NetworkObject closestNetObj = null;

            foreach (var hit in hits)
            {
                var interactable = hit.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    var netObj = hit.GetComponentInParent<NetworkObject>();
                    if (netObj != null)
                    {
                        float dist = Vector3.Distance(transform.position, hit.transform.position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestNetObj = netObj;
                        }
                    }
                }
            }

            if (closestNetObj != null)
            {
                // Отправляем RPC на сервер с запросом на взаимодействие
                ServerCmdInteract(closestNetObj);
            }
        }

        [ServerRpc]
        private void ServerCmdInteract(NetworkObject target)
        {
            if (target == null) return;

            // Валидация дистанции на сервере (защита от читов)
            float dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist > InteractRange + 0.5f) // Небольшой запас на рассинхрон
            {
                Debug.LogWarning($"[Interaction] Слишком далеко для взаимодействия: {dist}m");
                return;
            }

            var interactable = target.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.ServerInteract(this);
            }
        }

        // Отрисовка радиуса взаимодействия в редакторе
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, InteractRange);
        }
    }
}
