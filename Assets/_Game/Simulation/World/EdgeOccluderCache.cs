using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;

namespace Zomboid.Simulation.World
{
    public struct Segment
    {
        public float2 A;
        public float2 B;

        public Segment(float2 a, float2 b)
        {
            A = a;
            B = b;
        }
    }

    // Хранится как обычный Vector2, а не float2 — Unity умеет сериализовать Vector2 из коробки,
    // а Segment/float2 (Unity.Mathematics) в инспекторе не сохранится.
    [System.Serializable]
    public struct SerializedSegment
    {
        public Vector2 A;
        public Vector2 B;
    }

    // Раньше сегменты-окклюдеры строились рейкастами по TileData-гриду — хрупко: ломалось от Scale,
    // дробных координат дома, скошенных крыш, отсутствующих коллайдеров. Теперь источник — запечённый
    // NavMesh: границы ходибельной зоны (Tools → Zomboid → Bake NavMesh Occluders) и есть стены.
    // NavMesh печётся по реальным рендер-мешам без коллайдеров и сам корректно фильтрует крыши по углу наклона.
    public class EdgeOccluderCache : MonoBehaviour
    {
        public static EdgeOccluderCache Instance { get; private set; }

        [SerializeField] private List<SerializedSegment> _bakedSegments = new List<SerializedSegment>();
        private NativeArray<Segment> _segments;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            BuildRuntimeArray();
        }

        private void OnDestroy()
        {
            if (_segments.IsCreated) _segments.Dispose();
        }

        private void BuildRuntimeArray()
        {
            if (_segments.IsCreated) _segments.Dispose();

            _segments = new NativeArray<Segment>(_bakedSegments.Count, Allocator.Persistent);
            for (int i = 0; i < _bakedSegments.Count; i++)
            {
                var s = _bakedSegments[i];
                _segments[i] = new Segment(new float2(s.A.x, s.A.y), new float2(s.B.x, s.B.y));
            }
        }

        // floor пока не используется — NavMesh печём только для одного этажа (Y около 0..3),
        // так же как VisibilitySolver сейчас жёстко читает floor=0. Когда появятся этажи,
        // тут нужно будет хранить сегменты по этажам отдельно (несколько NavMeshSurface с разной высотой).
        public void Collect(float2 center, float radius, int floor, ref NativeList<Segment> result)
        {
            if (!_segments.IsCreated) return;

            float r2 = radius * radius;
            for (int i = 0; i < _segments.Length; i++)
            {
                Segment seg = _segments[i];
                if (DistPointSegmentSq(center, seg.A, seg.B) <= r2)
                {
                    result.Add(seg);
                }
            }
        }

        private static float DistPointSegmentSq(float2 p, float2 a, float2 b)
        {
            float2 ab = b - a;
            float t = math.dot(p - a, ab) / math.max(math.dot(ab, ab), 0.0001f);
            t = math.clamp(t, 0f, 1f);
            float2 closest = a + ab * t;
            return math.distancesq(p, closest);
        }

#if UNITY_EDITOR
        // Вызывается из NavMeshOccluderBaker после запекания NavMesh.
        public void SetBakedSegments(List<SerializedSegment> segments)
        {
            _bakedSegments = segments;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            foreach (var s in _bakedSegments)
            {
                Vector3 a = new Vector3(s.A.x, 0.2f, s.A.y);
                Vector3 b = new Vector3(s.B.x, 0.2f, s.B.y);
                Gizmos.DrawLine(a, b);
                Gizmos.DrawSphere(a, 0.1f);
                Gizmos.DrawSphere(b, 0.1f);
            }
        }
#endif
    }
}
