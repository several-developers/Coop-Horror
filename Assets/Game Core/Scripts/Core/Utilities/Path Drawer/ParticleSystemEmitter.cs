using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;

namespace GameCore.Utilities
{
    public class ParticleSystemEmitter : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowIndexLabels = false)]
        [SerializeField]
        private Entry[] _entries;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void EmitAt(Vector3 position, Quaternion rotation)
        {
            for (var i = 0; i < _entries.Length; i++)
            {
                _entries[i].ParticleSystem.transform.position = position;
                _entries[i].ParticleSystem.transform.rotation = rotation;
                _entries[i].ParticleSystem.Emit(_entries[i].EmitCount);
            }
        }

        public void RemoveParticles(List<Vector3> positions)
        {
            foreach (var entry in _entries)
            {
                var array = new NativeArray<ParticleSystem.Particle>(entry.ParticleSystem.particleCount,
                    Allocator.Temp);
                var particles = entry.ParticleSystem.GetParticles(array);

                for (var i = 0; i < particles; i++)
                {
                    var particle = array[i];
                    var particlePosition = particle.position;
                    var particleSize = particle.GetCurrentSize(entry.ParticleSystem);

                    foreach (var position in positions)
                    {
                        if (Vector3.Distance(particlePosition, position) < particleSize)
                        {
                            particle.remainingLifetime = 0;
                        }
                    }
                }
            }
        }

        [Serializable]
        public class Entry
        {
            [HideLabel]
            [HorizontalGroup("1")]
            public ParticleSystem ParticleSystem;

            [HorizontalGroup("1")]
            [LabelWidth(90)]
            public int EmitCount = 1;
        }
    }
}