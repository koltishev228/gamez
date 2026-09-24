using ZombieGame.Core;
using ZombieGame.Data;

namespace ZombieGame.Sim.Systems
{
    public class StatsSystem : IIntervalSystem
    {
        public float IntervalSec => 2.5f; // 1 игровая минута

        private IServerContext _ctx;
        private NeedsConfig _config;

        public StatsSystem(NeedsConfig config)
        {
            _config = config;
        }

        public void Init(IServerContext ctx)
        {
            _ctx = ctx;
            ZomboidLogger.Log(LogCategory.Sim, "StatsSystem: Инициализировано.");
        }

        public void Run(in TickInfo t)
        {
            if (_config == null) return;

            var registry = _ctx.GetSystem<PlayerRegistry>();
            if (registry == null) return;

            float realSecondsPassed = IntervalSec;
            float worldMinutesPassed = realSecondsPassed * _ctx.Clock.MinutesPerRealSecond;
            float worldHoursPassed = worldMinutesPassed / 60f;

            foreach (var p in registry.GetAllPlayers())
            {
                p.Stats.Hunger += _config.HungerPerHour * worldHoursPassed;
                p.Stats.Thirst += _config.ThirstPerHour * worldHoursPassed;
                p.Stats.Fatigue += _config.FatiguePerHour * worldHoursPassed;

                if (p.Stats.Hunger > 1f) p.Stats.Hunger = 1f;
                if (p.Stats.Thirst > 1f) p.Stats.Thirst = 1f;
                if (p.Stats.Fatigue > 1f) p.Stats.Fatigue = 1f;

                ZomboidLogger.Log(LogCategory.Sim, $"[Stats] Игрок {p.ClientId}: Голод {p.Stats.Hunger:P2}, Жажда {p.Stats.Thirst:P2}");
            }
        }
    }
}
