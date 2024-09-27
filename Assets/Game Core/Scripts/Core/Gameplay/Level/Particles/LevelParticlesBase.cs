using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Level.Particles
{
    public abstract class LevelParticlesBase : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private PlayerTrigger _playerTrigger;
        
        [SerializeField, Required, Space(height: 5)]
        private List<ParticleSystem> _particlesList;

        // FIELDS: --------------------------------------------------------------------------------

        private bool _isParticlesEnabled;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Awake()
        {
            _playerTrigger.OnPlayerEnterEvent += OnPlayerEnter;
            _playerTrigger.OnPlayerExitEvent += OnPlayerExit;
        }

        protected virtual void OnDestroy()
        {
            _playerTrigger.OnPlayerEnterEvent -= OnPlayerEnter;
            _playerTrigger.OnPlayerExitEvent -= OnPlayerExit;
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            Vector3 thisPosition = transform.position;
            
            foreach (ParticleSystem system in _particlesList)
            {
                if (system == null)
                {
                    Debug.LogWarning(message: "Particle is missing!");
                    continue;
                }
                
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

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnPlayerEnter() => EnableParticles();

        private void OnPlayerExit() => DisableParticles();

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugEnableParticles() => EnableParticles();

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugDisableParticles() => DisableParticles();
    }
}