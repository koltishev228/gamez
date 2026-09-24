using UnityEngine;
using System.Text;

namespace ZombieGame.Core
{
    public enum LogCategory { Core, Net, Sim, World, AI, UI, Editor }

    public static class ZomboidLogger
    {
        public static void Log(LogCategory category, string message)
        {
#if UNITY_EDITOR || DEBUG
            Debug.Log($"<color=#569CD6>[{category}]</color> {message}");
#endif
        }

        public static void LogWarning(LogCategory category, string message)
        {
#if UNITY_EDITOR || DEBUG
            Debug.LogWarning($"<color=#DCDCAA>[{category}]</color> {message}");
#endif
        }

        public static void LogError(LogCategory category, string message)
        {
            Debug.LogError($"<color=#F44336>[{category}]</color> {message}");
        }
    }
}
