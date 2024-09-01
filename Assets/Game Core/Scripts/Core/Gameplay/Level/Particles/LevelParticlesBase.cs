using System.Collections.Generic;
using GameCore.Gameplay.Entities.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Level.Particles
{
    public abstract class LevelParticlesBase : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private List<ParticleSystem> _particlesList;

        // FIELDS: --------------------------------------------------------------------------------

        private bool _isParticlesEnabled;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerEntity _))
                return;

            EnableParticles();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out PlayerEntity _))
                return;

            DisableParticles();
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            Vector3 thisPosition = transform.position;
            
            foreach (ParticleSystem system in _particlesList)
            {
                Vector3 particlePosition = system.transform.position;
                Color color = ColorsConstants.ZoneColor;
                Color gizmosColor = Gizmos.color;

                Gizmos.color = color;
                Gizmos.DrawLine(thisPosition, particlePosition);
                Gizmos.color = gizmosColor;
            }
        }
#endif

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected virtual void EnableParticles() => ChangeParticlesState(isEnabled: true);

        protected virtual void DisableParticles() => ChangeParticlesState(isEnabled: false);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeParticlesState(bool isEnabled)
        {
            if (isEnabled && _isParticlesEnabled)
                return;

            if (!isEnabled && !_isParticlesEnabled)
                return;

            foreach (ParticleSystem system in _particlesList)
            {
                if (isEnabled)
                    system.Play();
                else
                    system.Stop();
            }
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugEnableParticles() => EnableParticles();

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugDisableParticles() => DisableParticles();
    }
}