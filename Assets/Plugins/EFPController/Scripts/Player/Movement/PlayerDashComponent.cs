using System.Collections;
using EFPController.Utils;
using UnityEngine;

namespace EFPController
{
    public class PlayerDashComponent
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayerDashComponent(Player player, PlayerDashConfig dashConfig)
        {
            _player = player;
            _dashConfig = dashConfig;

            _playerMovement = player.controller;
            
            //dashActiveTime = Mathf.Min(dashActiveTime, dashTime); // Vlad, check this
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerDashConfig DashConfig => _dashConfig;
        public Vector3 DashDirection => _dashDirection;
        public float DashCurrentForce => _dashCurrentForce;
        public bool DashActive { get; set; } // Vlad, set was private
        public bool DashState { get; private set; } // Vlad, was private field

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Player _player;
        private readonly PlayerMovement _playerMovement;
        private readonly PlayerDashConfig _dashConfig;

        private Coroutine _dashCO;
        private Vector3 _dashDirection;
        private float _dashCurrentForce;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartDashCoroutine()
        {
            StopDashCoroutine();

            _dashCO = _playerMovement.StartCoroutine(routine: DashCoroutine());
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StopDashCoroutine()
        {
            if (_dashCO == null)
                return;
            
            _playerMovement.StopCoroutine(_dashCO);
        }
        
        private IEnumerator DashCoroutine()
        {
            DashActive = true;
            DashState = true;

            _playerMovement.sprint = false;
            _dashDirection = _playerMovement.moveDirection.normalized;

            yield return new WaitForEndOfFrame();

            float dashSoundVolume = _dashConfig.dashSoundVolume;
            float dashTime = _dashConfig.dashTime;
            float dashSpeed = _dashConfig.dashSpeed;
            float dashActiveTime = _dashConfig.dashActiveTime;
            float dashCooldown = _dashConfig.dashCooldown;
            AnimationCurve dashSpeedCurve = _dashConfig.dashSpeedCurve;
            
            if (_playerMovement.dashSounds.Length > 0)
                _player.effectsAudioSource.PlayOneShot(_playerMovement.dashSounds.Random(), dashSoundVolume);
            
            _player.cameraControl.SetEffectFilter(CameraControl.ScreenEffectProfileType.Dash, 1f, dashTime - 0.2f, 0.2f);

            float elapsed = 0f;
            float time = dashTime;
            
            while (elapsed < time)
            {
                float curveTime = elapsed / time;
                _dashCurrentForce = dashSpeed * dashSpeedCurve.Evaluate(curveTime);
                elapsed += Time.deltaTime;
                
                if (elapsed > dashActiveTime)
                    DashActive = false;
                
                yield return null;
            }

            _dashCurrentForce = 0f;
            DashActive = false;

            yield return new WaitForSeconds(dashCooldown);
            DashState = false;
        }
    }
}