using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Zomboid.Simulation.World;
using ZombieGame.Visibility;
using System.Collections.Generic;

namespace Zomboid.Simulation.Visibility
{
    public class EntityVisibilitySystem : MonoBehaviour
    {
        public static EntityVisibilitySystem Instance;

        public Transform Player;

        private void Awake()
        {
            Instance = this;
        }

        private void LateUpdate()
        {
            if (Player == null) return;
            if (WorldGrid.Instance == null) return;

            float viewRadius = FovSystem.Instance != null ? FovSystem.Instance.ViewRadius : 25f;
            float awareRadius = FovSystem.Instance != null ? FovSystem.Instance.AwareRadius : 2f;
            float viewAngle = FovSystem.Instance != null ? FovSystem.Instance.ViewAngle : 120f;
            float cosHalfAngle = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);

            Vector3 eyePos = Player.position + new Vector3(0, 1.6f, 0);

            // Обрабатываем все сущности (сферы, зомби), на которых висит VisibilityTarget
            foreach (var target in VisibilityTarget.AllTargets)
            {
                if (target == null) continue;
                
                Vector3 targetPos = target.transform.position + new Vector3(0, 0.5f, 0);

                bool visible = CheckVisibility(target, eyePos, targetPos, Player.forward, viewRadius, awareRadius, cosHalfAngle);
                target.SetVisible(visible);
            }
        }

        public void SetPlayer(Transform player)
        {
            Player = player;
        }

        private bool CheckVisibility(VisibilityTarget target, Vector3 from, Vector3 to, Vector3 forward, float vRadius, float aRadius, float cosHalf)
        {
            float dist = Vector3.Distance(from, to);
            if (dist > vRadius) return false;

            Vector3 dir = (to - from);
            dir.y = 0;
            if (dir.sqrMagnitude > 0.0001f) dir.Normalize();

            Vector3 fwd = forward;
            fwd.y = 0;
            if (fwd.sqrMagnitude > 0.0001f) fwd.Normalize();
            
            bool inCone = Vector3.Dot(dir, fwd) >= cosHalf;
            bool inAware = dist <= aRadius;

            if (!inCone && !inAware) return false;

            // Идеальная проверка: точка в полигоне (Point-in-Polygon)
            // Мы используем ТОТ ЖЕ полигон, который рендерится на экране, поэтому 100% точность!
            if (VisibilitySolver.Instance != null)
            {
                var poly = VisibilitySolver.Instance.GetPolygon();
                if (poly.IsCreated && poly.Length > 2)
                {
                    float2 pt = new float2(to.x, to.z);
                    bool inside = false;
                    for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
                    {
                        if (((poly[i].y > pt.y) != (poly[j].y > pt.y)) &&
                            (pt.x < (poly[j].x - poly[i].x) * (pt.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
                        {
                            inside = !inside;
                        }
                    }
                    if (!inside) return false; // За стеной (вне полигона)
                }
            }

            return true;
        }
    }
}
