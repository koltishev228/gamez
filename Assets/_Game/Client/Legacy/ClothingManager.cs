using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace ZombieGame.Equipment
{
    public class ClothingManager : MonoBehaviour
    {
        [Header("Настройки")]
        [Tooltip("Сюда можно временно перетащить префаб куртки для теста")]
        public GameObject testClothingPrefab;

        // Словарь для быстрого поиска костей нашего героя по имени
        private Dictionary<string, Transform> _boneMap = new Dictionary<string, Transform>();

        private void Start()
        {
            // Собираем все кости нашего главного героя (к кому прикреплен скрипт)
            Transform[] allBones = GetComponentsInChildren<Transform>();
            foreach (Transform bone in allBones)
            {
                if (!_boneMap.ContainsKey(bone.name))
                {
                    _boneMap.Add(bone.name, bone);
                }
            }
        }

        private void Update()
        {
            // ВРЕМЕННЫЙ ТЕСТ: Нажмите клавишу J, чтобы надеть куртку (Используем новую систему ввода!)
            if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame && testClothingPrefab != null)
            {
                EquipClothing(testClothingPrefab);
            }
        }

        public void EquipClothing(GameObject clothingPrefab)
        {
            Debug.Log($"Надеваем одежду: {clothingPrefab.name}");

            // 1. Создаем копию куртки и делаем её дочерней к нашему игроку
            GameObject clothingInstance = Instantiate(clothingPrefab, transform);
            
            // 2. Ищем на ней компонент SkinnedMeshRenderer (её "кожу")
            SkinnedMeshRenderer clothingRenderer = clothingInstance.GetComponentInChildren<SkinnedMeshRenderer>();
            
            if (clothingRenderer == null)
            {
                Debug.LogError("На этой одежде нет SkinnedMeshRenderer! Она не может сгибаться.");
                return;
            }

            // 3. Создаем массив для новых костей
            Transform[] newBones = new Transform[clothingRenderer.bones.Length];

            // 4. Проходим по каждой кости куртки и ищем такую же кость у нас в теле
            for (int i = 0; i < clothingRenderer.bones.Length; i++)
            {
                string boneName = clothingRenderer.bones[i].name;
                if (_boneMap.TryGetValue(boneName, out Transform myBone))
                {
                    newBones[i] = myBone; // Привязываем кость куртки к кости тела
                }
                else
                {
                    Debug.LogWarning($"Осторожно: Кость {boneName} от куртки не найдена в скелете героя!");
                }
            }

            // 5. Жестко пришиваем нашу настоящую скелетную базу к куртке
            clothingRenderer.bones = newBones;
            
            if (clothingRenderer.rootBone != null && _boneMap.TryGetValue(clothingRenderer.rootBone.name, out Transform myRoot))
            {
                clothingRenderer.rootBone = myRoot;
            }
            
            Debug.Log("Одежда успешно привязана к костям!");
        }
    }
}
