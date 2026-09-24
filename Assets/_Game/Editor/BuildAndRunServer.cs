using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using UnityEditor.Build.Profile;

namespace ZombieGame.Editor
{
    public static class BuildAndRunServer
    {
        [MenuItem("Zombie Game/🚀 Build & Run Server")]
        public static void BuildAndRun()
        {
            // Убеждаемся, что папка Builds существует
            string buildsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Builds");
            if (!Directory.Exists(buildsFolder))
            {
                Directory.CreateDirectory(buildsFolder);
            }

            string buildPath = Path.Combine(buildsFolder, "ZombieServer");
            
            // Настраиваем параметры сборки (Dedicated Server)
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/_Game/Scenes/Boot.unity", "Assets/_Game/Scenes/MainMenu.unity", "Assets/_Game/Scenes/World_Test.unity" },
                locationPathName = buildPath,
                target = BuildTarget.StandaloneOSX,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                options = BuildOptions.Development // Чтобы были логи!
            };

            UnityEngine.Debug.Log("Начинаем сборку сервера...");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                UnityEngine.Debug.Log($"Сервер успешно собран: {buildPath}");

                // Запускаем сервер
                string executablePath = Path.Combine(buildPath, "Zombie");
                if (File.Exists(executablePath))
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = executablePath,
                            UseShellExecute = true // Откроет в новом окне терминала (на Маке)
                        }
                    };
                    process.Start();
                    UnityEngine.Debug.Log("Сервер запущен в фоновом режиме!");
                }
                else
                {
                    UnityEngine.Debug.LogError("Исполняемый файл сервера не найден!");
                }
            }
            else
            {
                UnityEngine.Debug.LogError("Ошибка при сборке сервера.");
            }
        }
    }
}
