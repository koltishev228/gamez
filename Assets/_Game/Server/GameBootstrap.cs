using UnityEngine;
using FishNet.Object;
using FishNet.Managing.Timing;
using ZombieGame.Core;
using ZombieGame.Sim.Systems;
using ZombieGame.Data;
using ZombieGame.Sim;

namespace ZombieGame.Server
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Configs")]
        public NeedsConfig NeedsSettings;

        private ServerContext _context;
        private ServerScheduler _scheduler;
        private FishNet.Managing.NetworkManager _nm;

        private void Awake()
        {
            _nm = FindFirstObjectByType<FishNet.Managing.NetworkManager>();
            if (_nm == null) return;

            _context = new ServerContext();
            ServerLocator.Context = _context; // Открываем доступ для PlayerNet

            _scheduler = new ServerScheduler(_context);

            // Регистрируем базовые системы
            _scheduler.AddSystem(new PlayerRegistry());

            if (NeedsSettings != null)
            {
                _scheduler.AddSystem(new StatsSystem(NeedsSettings));
            }
            else
            {
                ZomboidLogger.LogWarning(LogCategory.Core, "GameBootstrap: NeedsConfig не назначен!");
            }
            
            _scheduler.Init();

            _nm.TimeManager.OnTick += OnNetworkTick;
            ZomboidLogger.Log(LogCategory.Core, "GameBootstrap: Серверное ядро успешно запущено!");
        }

        private void OnDestroy()
        {
            ServerLocator.Context = null;
            if (_nm != null && _nm.TimeManager != null)
                _nm.TimeManager.OnTick -= OnNetworkTick;
        }

        private void OnNetworkTick()
        {
            if (_nm.IsServerStarted)
            {
                float dt = (float)_nm.TimeManager.TickDelta;
                _scheduler.Tick(dt);
            }
        }
    }
}
