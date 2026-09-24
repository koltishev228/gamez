using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
[InitializeOnLoad]
public class LogDumper
{
    static LogDumper()
    {
        Application.logMessageReceived += HandleLog;
    }

    private static void HandleLog(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
        {
            try
            {
                File.AppendAllText("Assets/error_dump.txt", condition + "\n" + stackTrace + "\n");
            }
            catch {}
        }
    }
}
#endif
