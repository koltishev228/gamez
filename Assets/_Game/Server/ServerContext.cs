using System;
using System.Collections.Generic;
using ZombieGame.Core;

namespace ZombieGame.Server
{
    public sealed class ServerContext : IServerContext
    {
        public GameClock Clock { get; } = new GameClock();
        
        private readonly Dictionary<Type, IServerSystem> _systems = new Dictionary<Type, IServerSystem>();

        public void RegisterSystem(IServerSystem system)
        {
            _systems[system.GetType()] = system;
        }

        public T GetSystem<T>() where T : class, IServerSystem
        {
            if (_systems.TryGetValue(typeof(T), out var sys))
                return sys as T;
            return null;
        }

        public void InitAll()
        {
            foreach (var sys in _systems.Values)
            {
                sys.Init(this);
            }
        }
    }
}
