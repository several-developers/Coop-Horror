using Sirenix.OdinInspector;
using Sonity;
using UnityEngine;

namespace GameCore.Gameplay.Systems.Sound
{
    public class MainMenuMusicPlayer : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private SoundEvent _musicSE;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() =>
            _musicSE.Play(transform);

        private void OnDestroy() =>
            _musicSE.Stop(transform);
    }
}
