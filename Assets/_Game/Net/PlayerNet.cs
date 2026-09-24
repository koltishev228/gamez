using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using ZombieGame.Sim;
using ZombieGame.Core;

namespace ZombieGame.Net
{
    // Компонент вешается на префаб игрока
    public class PlayerNet : NetworkBehaviour
    {
        private PlayerSim _sim;

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (ServerLocator.Context == null) return;

            var registry = ServerLocator.Context.GetSystem<PlayerRegistry>();
            if (registry != null)
            {
                _sim = new PlayerSim { ClientId = Owner.ClientId };
                registry.AddPlayer(Owner.ClientId, _sim);
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            if (ServerLocator.Context == null) return;

            var registry = ServerLocator.Context.GetSystem<PlayerRegistry>();
            registry?.RemovePlayer(Owner.ClientId);
        }
    }
}
