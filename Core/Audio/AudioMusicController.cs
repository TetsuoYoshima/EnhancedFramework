// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="AudioMusicAsset"/>-related <see cref="EnhancedBehaviour"/> controller.
    /// </summary>
    [AddComponentMenu(FrameworkUtility.MenuPath + "Audio/Music Controller"), DisallowMultipleComponent]
    public sealed class AudioMusicController : AudioControllerBehaviour {
        #region Global Members
        [Section("Music Controller"), PropertyOrder(0)]

        [Tooltip("Music wrapped with this controller")]
        [SerializeField, Enhanced, Required] private AudioMusicAsset music = null;

        [Space(5f)]

        [Tooltip("Music loop override")]
        [SerializeField] private LoopOverride loopOverride = LoopOverride.None;

        [Tooltip("If true, overrides the default play settings of this music")]
        [SerializeField] private bool overrideSettings = false;

        [Space(5f)]

        [Tooltip("Mode used to interrupt the current music(s) when playing this one")]
        [SerializeField, Enhanced, ShowIf(nameof(overrideSettings))] private MusicInterruption interruptionMode = MusicInterruption.Pause;

        [Tooltip("Audio layer on which to play this music")]
        [SerializeField, Enhanced, ShowIf(nameof(overrideSettings))] private AudioLayer layer = AudioLayer.Default;

        // -----------------------

        /// <summary>
        /// <see cref="AudioMusicAsset"/> wrapped in this object.
        /// </summary>
        public AudioMusicAsset Music {
            get { return music; }
        }

        /// <inheritdoc cref="AudioMusicAsset.Layer"/>
        public AudioLayer Layer {
            get { return overrideSettings ? layer : music.Layer; }
        }

        /// <inheritdoc cref="AudioMusicAsset.InterruptionMode"/>
        public MusicInterruption InterruptionMode {
            get { return overrideSettings ? interruptionMode : music.InterruptionMode; }
        }
        #endregion

        #region Behaviour
        protected override void OnActivation() {
            base.OnActivation();

            // Loop.
            MusicHandler _music = AudioManager.Instance.PlayMusic(Music, Layer, InterruptionMode);

            if (_music.GetHandle(out MusicPlayer _handle)) {
                _handle.Loop = loopOverride;
            }
        }

        protected override void OnDeactivation() {
            base.OnDeactivation();

            AudioManager.Instance.StopMusic(Music, Layer);
        }
        #endregion
    }
}
