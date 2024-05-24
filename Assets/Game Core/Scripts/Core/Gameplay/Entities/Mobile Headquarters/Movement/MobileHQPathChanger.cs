using Cinemachine;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    [RequireComponent(typeof(SphereCollider))]
    public class MobileHQPathChanger : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator) =>
            _gameManagerDecorator = gameManagerDecorator;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _drawSphere = true;

        [Title(Constants.References)]
        [SerializeField, Required]
        private CinemachinePath _targetPath;

        [SerializeField, Required]
        private SphereCollider _sphereCollider;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool DrawSphere => _drawSphere;

        // FIELDS: --------------------------------------------------------------------------------

        private IGameManagerDecorator _gameManagerDecorator;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnTriggerEnter(Collider other) => HandleTriggerEnter(other);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public float GetRadius() =>
            _sphereCollider == null ? 0f : _sphereCollider.radius;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleTriggerEnter(Collider other)
        {
            if (!NetworkHorror.IsTrueServer)
                return;
            
            bool isMobileHQ = other.TryGetComponent(out IMobileHeadquartersEntity mobileHeadquartersEntity);

            if (!isMobileHQ)
                return;
            
            Debug.LogWarning("Trying change the path...");

            if (_targetPath == null)
            {
                Log.PrintError(log: "Target Path <rb>not found</rb>!");
                return;
            }

            GameState gameState = _gameManagerDecorator.GetGameState();
            bool isGameStateValid = gameState == GameState.LeavingRoadLocation;

            if (!isGameStateValid)
                return;

            mobileHeadquartersEntity.ChangePath(_targetPath, stayAtSamePosition: true);
        }
    }
}