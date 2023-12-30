using Sirenix.OdinInspector.Editor;
using Unity.Netcode;
using UnityEditor;

namespace GameCore.Gameplay.Network.Editor
{
    [CustomEditor(typeof(NetworkBehaviour))]
    [CanEditMultipleObjects]
    public class NetworkBehaviourEditor : OdinEditor 
    {
    }
}