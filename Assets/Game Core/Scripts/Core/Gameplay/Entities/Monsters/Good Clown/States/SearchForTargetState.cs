using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.Utilities;
using GameCore.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown.States
{
    public class SearchForTargetState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SearchForTargetState(GoodClownEntity goodClownEntity)
        {
            _goodClownEntity = goodClownEntity;
            _transform = goodClownEntity.transform;
            _clownUtilities = goodClownEntity.GetClownUtilities();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const float SearchInterval = 1f;

        private readonly GoodClownEntity _goodClownEntity;
        private readonly Transform _transform;
        private readonly GoodClownUtilities _clownUtilities;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            DisableAgent();
            SetIdleAnimation();
            StartHunterSystem();
            SearchLogic();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DisableAgent()
        {
            NavMeshAgent agent = _goodClownEntity.GetAgent();
            agent.enabled = false;
        }

        private void SetIdleAnimation() =>
            _clownUtilities.SetIdleAnimation();

        private void StartHunterSystem()
        {
            HunterSystem hunterSystem = _goodClownEntity.GetHunterSystem();
            hunterSystem.Start();
        }
        
        private async void SearchLogic()
        {
            while (true)
            {
                int delay = SearchInterval.ConvertToMilliseconds();

                bool isCanceled = await UniTask
                    .Delay(delay, cancellationToken: _goodClownEntity.GetCancellationTokenOnDestroy())
                    .SuppressCancellationThrow();

                if (isCanceled)
                    return;

                Vector3 position = _transform.position;

                //bool isTargetFound = MonstersUtilities.TryGetClosestAlivePlayer(position, EntityLocation.Dungeon,
                //out PlayerEntity targetPlayer);

                bool isTargetFound =
                    MonstersUtilities.TryGetClosestAlivePlayer(position, out PlayerEntity targetPlayer);

                if (!isTargetFound)
                    continue;

                _goodClownEntity.SetTargetPlayer(targetPlayer);
                EnterFollowTargetState();
                break;
            }
        }

        private void EnterFollowTargetState() =>
            _goodClownEntity.EnterFollowTargetState();
    }
}