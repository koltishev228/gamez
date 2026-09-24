using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Zomboid.Data.Items;

namespace Zomboid.Editor.Items
{
    [CustomEditor(typeof(ItemDefinition))]
    public class ItemDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Отрисовываем базовый инспектор
            DrawDefaultInspector();

            ItemDefinition def = (ItemDefinition)target;

            GUILayout.Space(15);
            EditorGUILayout.LabelField("Добавить модуль (Component):", EditorStyles.boldLabel);
            
            // Находим все классы, которые наследуются от ItemComponent
            var types = TypeCache.GetTypesDerivedFrom<ItemComponent>().Where(t => !t.IsAbstract).ToList();
            
            EditorGUILayout.BeginHorizontal();
            foreach (var type in types)
            {
                // Убираем слово Component для красоты на кнопке
                string btnName = type.Name.Replace("Component", "");
                if (GUILayout.Button(btnName, GUILayout.Height(30)))
                {
                    Undo.RecordObject(def, "Add " + type.Name);
                    
                    // Создаем экземпляр компонента и добавляем в список
                    def.Components.Add((ItemComponent)Activator.CreateInstance(type));
                    
                    // Говорим Unity, что файл изменился и его нужно сохранить
                    EditorUtility.SetDirty(def);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
