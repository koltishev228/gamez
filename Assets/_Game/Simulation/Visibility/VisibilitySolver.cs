using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Zomboid.Simulation.World;
using System.Collections.Generic;

namespace Zomboid.Simulation.Visibility
{
    [BurstCompile]
    public struct VisibilityJob : IJob
    {
        public float2 Center;
        public float Radius;
        
        [ReadOnly] public NativeArray<Segment> Segments;
        
        // Точки многоугольника по кругу
        public NativeList<float2> OutputPoints;

        public void Execute()
        {
            NativeList<float> angles = new NativeList<float>(Segments.Length * 2 + 16, Allocator.Temp);
            
            // Базовые углы, чтобы круг был круглым, когда нет стен
            int circleSegments = 16;
            for(int i = 0; i < circleSegments; i++) 
            {
                angles.Add(math.PI * 2f * (i / (float)circleSegments));
            }

            // Добавляем углы к краям каждой стены
            for (int i = 0; i < Segments.Length; i++)
            {
                float2 a = Segments[i].A - Center;
                float2 b = Segments[i].B - Center;

                if (math.lengthsq(a) > 0.001f)
                {
                    float ang = math.atan2(a.y, a.x);
                    angles.Add(ang);
                    angles.Add(ang - 0.0001f);
                    angles.Add(ang + 0.0001f);
                }
                
                if (math.lengthsq(b) > 0.001f)
                {
                    float ang = math.atan2(b.y, b.x);
                    angles.Add(ang);
                    angles.Add(ang - 0.0001f);
                    angles.Add(ang + 0.0001f);
                }
            }

            angles.Sort();

            // Пускаем лучи во все эти углы и ищем ближайшее пересечение со стеной
            for (int i = 0; i < angles.Length; i++)
            {
                float ang = angles[i];
                float2 dir = new float2(math.cos(ang), math.sin(ang));
                float2 rayEnd = Center + dir * Radius;
                
                float minT = 1.0f; // 1.0 = полный радиус
                
                for (int s = 0; s < Segments.Length; s++)
                {
                    float t = RayCastSegment(Center, rayEnd, Segments[s].A, Segments[s].B);
                    if (t >= 0f && t < minT)
                    {
                        minT = t;
                    }
                }
                
                OutputPoints.Add(Center + dir * (Radius * minT));
            }
            
            angles.Dispose();
        }

        private float RayCastSegment(float2 p0, float2 p1, float2 p2, float2 p3)
        {
            float2 s1 = p1 - p0;
            float2 s2 = p3 - p2;

            float denom = (-s2.x * s1.y + s1.x * s2.y);
            if (math.abs(denom) < 0.0001f) return -1f;

            float s = (-s1.y * (p0.x - p2.x) + s1.x * (p0.y - p2.y)) / denom;
            float t = ( s2.x * (p0.y - p2.y) - s2.y * (p0.x - p2.x)) / denom;

            if (s >= 0f && s <= 1f && t >= 0f && t <= 1f)
            {
                return t;
            }

            return -1f;
        }
    }

    [ExecuteAlways]
    public class VisibilitySolver : MonoBehaviour
    {
        public static VisibilitySolver Instance;

        public Transform Eye;
        public float Radius = 15f;
        public Material FOVMaterial;

        private Mesh _mesh;
        private NativeList<Segment> _segments;
        private NativeList<float2> _outputPoints;

        private void OnEnable()
        {
            Instance = this;
            _segments = new NativeList<Segment>(Allocator.Persistent);
            _outputPoints = new NativeList<float2>(Allocator.Persistent);

            if (_mesh == null)
            {
                _mesh = new Mesh();
                _mesh.name = "FOV_Polygon";
                _mesh.MarkDynamic();
            }

            if (FOVMaterial == null)
            {
                Shader unlit = Shader.Find("HDRP/Unlit");
                if (unlit != null)
                {
                    FOVMaterial = new Material(unlit);
                    FOVMaterial.SetColor("_UnlitColor", Color.white);
                    FOVMaterial.SetColor("_BaseColor", Color.white);
                    FOVMaterial.SetColor("_Color", Color.white);
                }
            }
        }

        private void OnDisable()
        {
            if (_segments.IsCreated) _segments.Dispose();
            if (_outputPoints.IsCreated) _outputPoints.Dispose();
        }

        private float _lastUpdateTime;
        private Vector3 _lastEyePos;
        public float UpdateInterval = 0.05f; // 20 Hz
        public float MoveThreshold = 0.1f;

        public void SetPlayer(Transform player)
        {
            Eye = player;
        }

        public NativeArray<float2> GetPolygon()
        {
            if (_outputPoints.IsCreated) return _outputPoints.AsArray();
            return default;
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying) return;
            if (Eye == null || EdgeOccluderCache.Instance == null) return;

            // Проверяем троттлинг и движение
            bool forceUpdate = false; // Можно связать с EdgeOccluderCache.OnChanged, но для начала просто время/движение
            
            float time = Time.time;
            if (!forceUpdate && time - _lastUpdateTime < UpdateInterval) return;
            
            if (Vector3.Distance(Eye.position, _lastEyePos) < MoveThreshold && time - _lastUpdateTime < 1.0f) 
                return; // Обновляем хотя бы раз в секунду на всякий случай или если сдвинулись

            _lastUpdateTime = time;
            _lastEyePos = Eye.position;

            float2 center = new float2(Eye.position.x, Eye.position.z);
            int floor = 0;

            // Полигон должен всегда доставать хотя бы так же далеко, как радиус обзора в шейдере (FovSystem.ViewRadius),
            // иначе за пределами Radius маска будет чёрной (не видно), даже если шейдер думает, что это в конусе.
            float effectiveRadius = FovSystem.Instance != null ? Mathf.Max(Radius, FovSystem.Instance.ViewRadius) : Radius;

            _segments.Clear();
            _outputPoints.Clear();

            EdgeOccluderCache.Instance.Collect(center, effectiveRadius, floor, ref _segments);

            var job = new VisibilityJob
            {
                Center = center,
                Radius = effectiveRadius,
                Segments = _segments.AsArray(),
                OutputPoints = _outputPoints
            };
            job.Schedule().Complete();

            BuildMesh();
            
            if (_mesh != null && FOVMaterial != null && FovSystem.Instance != null && FovSystem.Instance.MaskCamera != null)
            {
                Graphics.DrawMesh(_mesh, transform.position, Quaternion.identity, FOVMaterial, 31, FovSystem.Instance.MaskCamera);
            }
        }

        private void BuildMesh()
        {
            if (_outputPoints.Length < 3)
            {
                _mesh.Clear();
                return;
            }

            Vector3[] vertices = new Vector3[_outputPoints.Length + 1];
            int[] triangles = new int[_outputPoints.Length * 3];

            vertices[0] = new Vector3(0, 0, 0);

            for (int i = 0; i < _outputPoints.Length; i++)
            {
                vertices[i + 1] = new Vector3(_outputPoints[i].x - Eye.position.x, 0, _outputPoints[i].y - Eye.position.z);
            }

            for (int i = 0; i < _outputPoints.Length; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = (i + 1) % _outputPoints.Length + 1;
                triangles[i * 3 + 2] = i + 1;
            }

            _mesh.Clear();
            _mesh.vertices = vertices;
            _mesh.triangles = triangles;
            _mesh.RecalculateBounds();
            
            transform.position = new Vector3(Eye.position.x, Eye.position.y + 0.1f, Eye.position.z);
        }
    }
}
