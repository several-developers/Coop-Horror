using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.EvilClown;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Factories.Monsters;
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
            DelayedLogic();
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

        private async void DelayedLogic()
        {
            GoodClownAIConfigMeta goodClownAIConfig = _goodClownEntity.GetGoodClownAIConfig();
            GoodClownAIConfigMeta.CommonSettings commonConfig = goodClownAIConfig.CommonConfig;
            float delayInSeconds = commonConfig.TransformationDelay;
            int delay = delayInSeconds.ConvertToMilliseconds();

            bool isCanceled = await UniTask
                .Delay(delay, cancellationToken: _goodClownEntity.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
            SpawnEvilClown();
            EnterDeathState();
        }
        
        private void SpawnEvilClown()
        {
            Transform transform = _goodClownEntity.transform;
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;

            bool isSpawned = _monstersFactory
                .SpawnMonster(MonsterType.EvilClown, position, rotation, out MonsterEntityBase monsterEntity);

            if (!isSpawned)
                return;

            PlayerEntity targetPlayer = _goodClownEntity.GetTargetPlayer();
            bool isTargetFound = targetPlayer != null;

            if (!isTargetFound)
                return;

            if (monsterEntity is not EvilClownEntity evilClownEntity)
                return;
            
            evilClownEntity.SetTargetPlayer(targetPlayer);
        }

        private void EnterDeathState() =>
            _goodClownEntity.EnterDeathState();
    }
}