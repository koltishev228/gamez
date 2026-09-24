using System.Collections.Generic;
using UnityEngine;
using ZombieGame.Core;

namespace ZombieGame.Server
{
    public sealed class ServerScheduler
    {
        private class IntervalData
        {
            public IIntervalSystem System;
            public float Timer;
        }

        private readonly List<ITickSystem> _tickSystems = new List<ITickSystem>();
        private readonly List<IntervalData> _intervalSystems = new List<IntervalData>();
        private readonly ServerContext _ctx;
        private uint _currentTick;

        public ServerScheduler(ServerContext ctx)
        {
            _ctx = ctx;
        }

        public void AddSystem(IServerSystem sys)
        {
            _ctx.RegisterSystem(sys);
            if (sys is ITickSystem tickSys) _tickSystems.Add(tickSys);
            if (sys is IIntervalSystem intervalSys) _intervalSystems.Add(new IntervalData { System = intervalSys });
        }

        public void Init()
        {
            _ctx.InitAll();
            ZomboidLogger.Log(LogCategory.Core, $"ServerScheduler: Инициализировано {_tickSystems.Count + _intervalSystems.Count} систем.");
        }

        public void Tick(float deltaTime)
        {
            _currentTick++;
            _ctx.Clock.Advance(deltaTime);

            var tickInfo = new TickInfo { Tick = _currentTick, DeltaTime = deltaTime };

            // 1. Быстрые системы (20 Гц)
            for (int i = 0; i < _tickSystems.Count; i++)
            {
                _tickSystems[i].Tick(in tickInfo);
            }

            // 2. Медленные системы (интервалы)
            for (int i = 0; i < _intervalSystems.Count; i++)
            {
                var data = _intervalSystems[i];
                data.Timer += deltaTime;
                if (data.Timer >= data.System.IntervalSec)
                {
                    data.Timer -= data.System.IntervalSec;
                    data.System.Run(in tickInfo);
                }
            }
        }
    }
}
