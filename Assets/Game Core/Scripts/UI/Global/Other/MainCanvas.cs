using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.UI.Global.Other
{
    public class MainCanvas : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct() => Setup();

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Canvas _canvas;

        // PROPERTIES: ----------------------------------------------------------------------------

        public static Canvas Canvas { get; private set; }
        public static Transform Transform { get; private set; }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Setup()
        {
            Canvas = _canvas;
            Transform = _canvas.transform;
        }
    }
}