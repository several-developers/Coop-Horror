using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.EvilClown;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Factories.Monsters;
using GameCore.Gameplay.Utilities;
using GameCore.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown.States
{
    public class RespawnAsEvilClownState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public RespawnAsEvilClownState(GoodClownEntity goodClownEntity, IMonstersFactory monstersFactory)
        {
            _goodClownEntity = goodClownEntity;
            _monstersFactory = monstersFactory;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GoodClownEntity _goodClownEntity;
        private readonly IMonstersFactory _monstersFactory;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            DisableAgent();
            SetAgonizeAnimation();
            AwaitAndCreateEvilClown();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DisableAgent()
        {
            NavMeshAgent agent = _goodClownEntity.GetAgent();
            agent.speed = 0f;
        }

        private void SetAgonizeAnimation()
        {
            GoodClownUtilities clownUtilities = _goodClownEntity.GetClownUtilities();
            clownUtilities.SetAgonizeAnimation();
        }

        private async void AwaitAndCreateEvilClown()
        {
            bool isCanceled = false;
            
            await Delay();
            
            if (isCanceled)
                return;
            
            Create();
            
            // LOCAL METHODS: -----------------------------

            async UniTask Delay()
            {
                GoodClownAIConfigMeta goodClownAIConfig = _goodClownEntity.GetGoodClownAIConfig();
                GoodClownAIConfigMeta.CommonSettings commonConfig = goodClownAIConfig.CommonConfig;
                
                float delayInSeconds = commonConfig.TransformationDelay;
                int delay = delayInSeconds.ConvertToMilliseconds();

                isCanceled = await UniTask
                    .Delay(delay, cancellationToken: _goodClownEntity.GetCancellationTokenOnDestroy())
                    .SuppressCancellationThrow();
            }

            void Create()
            {
                Transform transform = _goodClownEntity.transform;
                Vector3 position = transform.position;
                Quaternion rotation = transform.rotation;

                var spawnParams = new SpawnParams<EvilClownEntity>.Builder()
                    .SetSpawnPosition(position)
                    .SetRotation(rotation)
                    .SetSuccessCallback(evilClownEntity =>
                    {
                        EvilClownSpawned(evilClownEntity);
                        EnterDeathState();
                    })
                    .Build();

                _monstersFactory.CreateMonsterDynamic(MonsterType.EvilClown, spawnParams);
            }
        }

        private void EvilClownSpawned(EvilClownEntity evilClownEntity)
        {
            PlayerEntity targetPlayer = _goodClownEntity.GetTargetPlayer();
            bool isTargetFound = targetPlayer != null;

            if (!isTargetFound)
                return;
            
            EntityLocation goodClownLocation = _goodClownEntity.EntityLocation;
            Floor goodClownFloor = _goodClownEntity.CurrentFloor;

            evilClownEntity.SetTargetPlayer(targetPlayer);
            evilClownEntity.SetEntityLocation(goodClownLocation);
            evilClownEntity.SetFloor(goodClownFloor);
        }

        private void EnterDeathState() =>
            _goodClownEntity.EnterDeathState();
    }
}