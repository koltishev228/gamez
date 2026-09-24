using System.Collections.Generic;
using ZombieGame.Core;

namespace ZombieGame.Sim
{
    public class PlayerRegistry : IServerSystem
    {
        private readonly Dictionary<int, PlayerSim> _players = new Dictionary<int, PlayerSim>();

        public void Init(IServerContext ctx)
        {
            ZomboidLogger.Log(LogCategory.Sim, "PlayerRegistry: Инициализировано.");
        }

        public void AddPlayer(int clientId, PlayerSim player)
        {
            _players[clientId] = player;
            ZomboidLogger.Log(LogCategory.Sim, $"PlayerRegistry: Игрок {clientId} подключен к симуляции.");
        }

        public void RemovePlayer(int clientId)
        {
            _players.Remove(clientId);
            ZomboidLogger.Log(LogCategory.Sim, $"PlayerRegistry: Игрок {clientId} удален из симуляции.");
        }

        public PlayerSim GetPlayer(int clientId)
        {
            _players.TryGetValue(clientId, out var p);
            return p;
        }

        public IEnumerable<PlayerSim> GetAllPlayers() => _players.Values;
    }
}
