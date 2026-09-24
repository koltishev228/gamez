using UnityEngine;
using FishNet.Object;
using ZombieGame.Core;

namespace ZombieGame.Client
{
    public class ClientBootstrap : MonoBehaviour
    {
        private void Start()
        {
            var nm = FindFirstObjectByType<FishNet.Managing.NetworkManager>();
            if (nm == null) return;

#if UNITY_EDITOR
            // В редакторе для удобства тестов запускаем сразу Хост (Сервер + Клиент)
            nm.ServerManager.StartConnection();
            nm.ClientManager.StartConnection();
            ZomboidLogger.Log(LogCategory.Net, "ClientBootstrap: Запущен локальный Хост (Редактор).");
#elif !UNITY_SERVER
            // В сбилженном клиенте подключаемся к серверу
            if (!Application.isBatchMode)
            {
                nm.ClientManager.StartConnection();
                ZomboidLogger.Log(LogCategory.Net, "ClientBootstrap: Подключаемся к выделенному серверу...");
            }
#endif
        }
    }
}
