using UnityEngine;

namespace ZombieGame.Core
{
    public sealed class GameClock
    {
        public double WorldMinutes;
        public float MinutesPerRealSecond = 0.4f;

        public void Advance(float realDt) => WorldMinutes += realDt * MinutesPerRealSecond;

        public int Day => (int)(WorldMinutes / 1440);
        public float HourOfDay => (float)(WorldMinutes % 1440) / 60f;
    }
}
