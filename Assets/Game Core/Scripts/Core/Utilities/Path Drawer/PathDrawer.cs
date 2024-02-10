using System;
using System.Collections;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Utilities
{
    public class PathDrawer : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Color _gizmoColor;

        [SerializeField, Min(0)]
        private float _radius = 0.1f;

        [Title(Constants.References)]
        //[SerializeField, Required]
        //private ParticleSystemEmitter _mainEmitter;

        //[SerializeField]
        //private ParticleSystemEmitter _highlightEmitter;

        [ListDrawerSettings(ShowIndexLabels = false)]
        [SerializeField]
        private Entry[] _path;

        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void Start()
        {
            foreach (var entry in _path)
            {
                //StartCoroutine(Draw(entry, _mainEmitter));

                //if (entry.HighlightAtStart)
                //StartCoroutine(Draw(entry, _highlightEmitter));
            }
        }

        private void OnDrawGizmos()
        {
            foreach (Entry entry in _path)
            {
                if (entry == null)
                    continue;

                Gizmos.color = _gizmoColor;

                for (var i = 0f; i < 1; i += entry.Step)
                {
                    Vector3 position =
                        entry.Path.EvaluatePositionAtUnit(i, CinemachinePathBase.PositionUnits.Normalized);

                    Gizmos.DrawSphere(position, _radius);
                }
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private static IEnumerator Draw(Entry entry, ParticleSystemEmitter emitter)
        {
            for (var i = 0f; i < 1; i += entry.Step)
            {
                var position = entry.Path.EvaluatePositionAtUnit(i, CinemachinePathBase.PositionUnits.Normalized);
                var rotation =
                    entry.Path.EvaluateOrientationAtUnit(i, CinemachinePathBase.PositionUnits.Normalized);

                emitter.EmitAt(position, rotation);

                yield return null;
                yield return null;
                yield return null;
                yield return null;
            }
        }

        [Serializable]
        public class Entry
        {
            [HideLabel]
            [HorizontalGroup("0")]
            public CinemachinePath Path;

            [HorizontalGroup("0")]
            [Range(0.0001f, 0.1f)]
            [LabelWidth(70)]
            public float Step = 0.05f;

            public bool HighlightAtStart;
        }
    }
}