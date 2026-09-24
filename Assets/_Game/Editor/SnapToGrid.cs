using UnityEngine;
using UnityEditor;

namespace Zomboid.Editor
{
    // WorldGrid/WorldGridBaker/EdgeOccluderCache считают, что 1 тайл = 1 юнит мира по X/Z,
    // а по высоте — что "этаж 0" лежит около мировой Y=0..3 (VisibilitySolver пока жёстко
    // читает только floor=0, других этажей рантайм не видит). Если дом сдвинут на дробные
    // X/Z или поднят/опущен по Y — бейкер физически мажет мимо стен рейкастами.
    // Scale НЕ трогаем — масштаб коллайдерам не мешает, раскастам всё равно, какого они размера.
    public static class SnapToGrid
    {
        [MenuItem("Tools/Zomboid/Snap Selected To Grid")]
        public static void Snap()
        {
            var selected = Selection.transforms;
            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog("Snap To Grid", "Ничего не выделено. Выдели дом(а) в иерархии.", "Ок");
                return;
            }

            int changed = 0;
            foreach (var t in selected)
            {
                Undo.RecordObject(t, "Snap To Grid");

                Vector3 p = t.position;
                p.x = Mathf.Round(p.x);
                p.z = Mathf.Round(p.z);
                p.y = 0f; // этаж 0 у бейкера всегда около мировой Y=0..3 — дом должен стоять именно тут
                t.position = p;

                changed++;
            }

            Debug.Log($"[SnapToGrid] Выровнено объектов: {changed} (X/Z округлены, Y=0). Scale не трогали. Не забудь заново нажать Bake World Grid.");
        }
    }
}
