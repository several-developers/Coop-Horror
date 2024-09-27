using System.Collections;
using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom
{
    public class MushroomSporesTrigger : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        public ParticleSystem _sporesPS;

        // FIELDS: --------------------------------------------------------------------------------

        private const float DestroyDelay = 20f;
        
        private MushroomAIConfigMeta.SporesConfig _sporesConfig;
        private Coroutine _stateCheckerCO;
        private bool _isHatNew;
        private bool _isEnabled;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnParticleCollision(GameObject other)
        {
            if (!_isEnabled)
                return;

            if (!other.TryGetComponent(out PlayerEntity playerEntity))
                return;

            HandlePlayerCollision(playerEntity);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(MushroomAIConfigMeta.SporesConfig sporesConfig, uint seed, bool isHatNew)
        {
            _sporesConfig = sporesConfig;
            _isHatNew = isHatNew;
            _sporesPS.randomSeed = seed;
            _isEnabled = true;
        }

        public void Start()
        {
            IEnumerator routine = LifeCycleCO();
            StartCoroutine(routine);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandlePlayerCollision(PlayerEntity playerEntity)
        {
            if (!IsLocalPlayer(playerEntity))
                return;
            
            Debug.Log("Hitted player");
        }

        private IEnumerator LifeCycleCO()
        {
            _sporesPS.Play();

            float duration = _sporesConfig.SporesDuration;

            if (!_isHatNew)
                duration *= 0.5f;
            
            yield return new WaitForSeconds(duration);
            
            _sporesPS.Stop();

            yield return new WaitForSeconds(DestroyDelay);
            
            Destroy(gameObject);
        }

        private static bool IsLocalPlayer(PlayerEntity playerEntity)
        {
            ulong targetClientID = playerEntity.OwnerClientId;
            bool isLocalPlayer = targetClientID == NetworkHorror.ClientID;
            return isLocalPlayer;
        }
    }
}