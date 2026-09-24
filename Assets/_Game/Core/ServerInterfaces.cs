using System;

namespace ZombieGame.Core
{
    public interface IServerContext 
    {
        T GetSystem<T>() where T : class, IServerSystem;
        GameClock Clock { get; }
    }

    public struct TickInfo
    {
        public uint Tick;
        public float DeltaTime;
    }

    public interface IServerSystem
    {
        void Init(IServerContext ctx);
    }

    public interface ITickSystem : IServerSystem
    {
        void Tick(in TickInfo t);
    }

    public interface IIntervalSystem : IServerSystem
    {
        float IntervalSec { get; }
        void Run(in TickInfo t);
    }
}
