using GameCore.Infrastructure.StateMachine;
using UnityEngine;
using Zenject;

namespace GameCore.Infrastructure.ScenesManagers.GameplayScene
{
    public class GameplaySceneManager : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameStateMachine gameStateMachine) =>
            _gameStateMachine = gameStateMachine;

        // FIELDS: --------------------------------------------------------------------------------
        
        private IGameStateMachine _gameStateMachine;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() => Init();
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Init()
        {
        }
    }
}