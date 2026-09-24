using UnityEngine;
using ZombieGame.Health;
using FishNet.Object;

namespace ZombieGame.Gore
{
    [System.Serializable]
    public class DismemberBone
    {
        public BodyPartType partType;
        [Tooltip("Перетащите сюда соответствующую кость из иерархии")]
        public Transform bone;
        [HideInInspector] public bool isSevered = false;
    }

    public class GoreSystem : NetworkBehaviour
    {
        [Header("Настройка расчлененки")]
        public DismemberBone[] severableBones;

        [Header("Визуальные эффекты")]
        [Tooltip("Префаб брызг крови (Particle System)")]
        public GameObject bloodSplatterPrefab;

        // Вызов отсечения с сервера
        [ServerRpc(RequireOwnership = false)]
        public void SeverLimbServer(BodyPartType type)
        {
            SeverLimbObserver(type);
        }

        // Выполнение отсечения у всех игроков
        [ObserversRpc]
        private void SeverLimbObserver(BodyPartType type)
        {
            foreach (var b in severableBones)
            {
                if (b.partType == type && !b.isSevered)
                {
                    if (b.bone != null)
                    {
                        // ТА САМАЯ МАГИЯ: Схлопываем кость в абсолютный ноль
                        b.bone.localScale = Vector3.zero;
                        b.isSevered = true;

                        // Спавним кровь на месте отрыва
                        if (bloodSplatterPrefab != null)
                        {
                            Instantiate(bloodSplatterPrefab, b.bone.position, Quaternion.identity);
                        }

                        Debug.Log($"[GoreSystem] Оторвана часть тела: {type}");
                    }
                    break;
                }
            }
        }

        private void Update()
        {
            // Используем новую систему ввода (InputSystem), так как старая отключена!
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                // Нажмите 'X', чтобы оторвать левую руку
                if (UnityEngine.InputSystem.Keyboard.current.xKey.wasPressedThisFrame)
                {
                    SeverLimbServer(BodyPartType.LeftArm);
                }
                
                // Нажмите 'Z', чтобы оторвать голову
                if (UnityEngine.InputSystem.Keyboard.current.zKey.wasPressedThisFrame)
                {
                    SeverLimbServer(BodyPartType.Head);
                }
            }
        }

        [ContextMenu("✨ Магия: Привязать все кости автоматически")]
        public void AutoAssignBones()
        {
            var bonesList = new System.Collections.Generic.List<DismemberBone>();
            Transform[] allBones = GetComponentsInChildren<Transform>();

            foreach (Transform t in allBones)
            {
                string n = t.name.ToLower();
                BodyPartType? type = null;

                // Ищем кости по стандартным именам Mixamo
                if (n.Contains("head") && !n.Contains("top")) type = BodyPartType.Head;
                else if (n.Contains("leftarm") && !n.Contains("fore")) type = BodyPartType.LeftArm;
                else if (n.Contains("rightarm") && !n.Contains("fore")) type = BodyPartType.RightArm;
                else if (n.Contains("leftupleg")) type = BodyPartType.LeftLeg;
                else if (n.Contains("rightupleg")) type = BodyPartType.RightLeg;

                if (type.HasValue)
                {
                    bonesList.Add(new DismemberBone { partType = type.Value, bone = t });
                }
            }

            severableBones = bonesList.ToArray();
            Debug.Log($"[GoreSystem] Автоматически найдено и привязано костей: {severableBones.Length}");
        }
    }
}
