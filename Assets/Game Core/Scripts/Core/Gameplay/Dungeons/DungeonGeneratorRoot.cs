using DunGen;
using GameCore.Enums.Gameplay;
using GameCore.Observers.Gameplay.LevelManager;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Dungeons
{
    public class DungeonGeneratorRoot : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ILevelProviderObserver levelProviderObserver) =>
            _levelProviderObserver = levelProviderObserver;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Floor _floor = Floor.One;

        [Title(Constants.References)]
        [SerializeField, Required]
        private RuntimeDungeon _runtimeDungeon;

        // FIELDS: --------------------------------------------------------------------------------
        
        private ILevelProviderObserver _levelProviderObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => RegisterDungeon();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RegisterDungeon()
        {
            DungeonWrapper dungeonWrapper = new(_runtimeDungeon, _floor);
            _levelProviderObserver.RegisterDungeon(dungeonWrapper);
        }
    }
}