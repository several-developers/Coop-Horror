using GameCore.Gameplay.Observers;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Other
{
    public class GraphyListener : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGraphyStateObserver graphyStateObserver) =>
            _graphyStateObserver = graphyStateObserver;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private GameObject _graphyObject;

        // FIELDS: --------------------------------------------------------------------------------

        private IGraphyStateObserver _graphyStateObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _graphyStateObserver.OnStateChangedEvent += OnStateChanged;

        private void OnDestroy() =>
            _graphyStateObserver.OnStateChangedEvent -= OnStateChanged;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStateChanged(bool isEnabled) =>
            _graphyObject.SetActive(isEnabled);
    }
}