using ZombieGame.Core;

namespace ZombieGame.Sim
{
    public sealed class PlayerSim
    {
        public int ClientId;
        public Stats Stats = new Stats();
    }

    public sealed class Stats
    {
        public float Hunger; 
        public float Thirst; 
        public float Fatigue; 
        public float Endurance = 1f;
    }
}
