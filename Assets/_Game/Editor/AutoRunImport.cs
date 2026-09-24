using UnityEngine;
using UnityEditor;

namespace Zomboid.Editor.Items
{
    public static class AutoRunImport
    {
        [InitializeOnLoadMethod]
        public static void RunOnce()
        {
            if (!SessionState.GetBool("HasRunPrefabGen_Once", false))
            {
                SessionState.SetBool("HasRunPrefabGen_Once", true);
                Debug.Log("🤖 [AutoRun] Братишка-AI генерирует префабы...");
                
                ItemPrefabGenerator.GeneratePrefabs();
                
                Debug.Log("🤖 [AutoRun] Все префабы успешно сгенерированы!");
            }
        }
    }
}
