using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Systems.Noise.Editor
{
    [CustomEditor(typeof(NoiseDrawer))]
    public class NoiseDrawerEditor : OdinEditor
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(
            GizmoType.NonSelected |
            GizmoType.Selected |
            GizmoType.InSelectionHierarchy |
            GizmoType.NotInSelectionHierarchy
        )]
        public static void RenderCustomGizmo(NoiseDrawer obj, GizmoType gizmo)
        {
            IReadOnlyList<NoiseDrawer.NoiseData> allNoiseData = obj.GetAllNoiseData();
            int noisesCount = allNoiseData.Count;
            bool canDraw = noisesCount > 0;

            if (!canDraw)
                return;

            float deltaTime = Time.deltaTime;

            for (int i = noisesCount - 1; i >= 0; i--)
            {
                NoiseDrawer.NoiseData noiseData = allNoiseData[i];
                Vector3 position = noiseData.NoisePosition;
                float radius = noiseData.NoiseRange;

                Color handlesColor = Gizmos.color;
                Gizmos.color = ColorsConstants.NoiseColor;
                Gizmos.DrawWireSphere(position, radius);
                Gizmos.color = handlesColor;

                bool isTimeOver = noiseData.DecreaseTime(deltaTime);

                if (!isTimeOver)
                    continue;

                obj.RemoveNoiseData(index: i);
            }
        }
    }
}