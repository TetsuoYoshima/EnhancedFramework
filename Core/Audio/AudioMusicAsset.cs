// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Determines how a music is interrupted while playing.
    /// </summary>
    public enum MusicInterruption {
        None    = 0,

        Pause   = 1,
        Stop    = 2,
    }

    /// <summary>
    /// <see cref="ScriptableObject"/> data holder for an audio-related music asset.
    /// </summary>
    [CreateAssetMenu(fileName = "MSC_MusicAsset", menuName = FrameworkUtility.MenuPath + "Audio/Music", order = FrameworkUtility.MenuOrder)]
    public sealed class AudioMusicAsset : EnhancedScriptableObject {
        /// <summary>
        /// Settings used to perform a music transition.
        /// </summary>
        [Serializable]
        public sealed class TransitionSettings {
            #region Global Members
            [Tooltip("If true, overrides transition fade out parameters")]
            public bool OverrideFadeOut = false;

            [Tooltip("Transition fade out duration (in seconds)")]
            [Enhanced, ShowIf(nameof(OverrideFadeOut)), Range(0f, 60f)] public float FadeOutDuration = 1f;

            [Tooltip("Transition fade out evaluation curve")]
            [Enhanced, ShowIf(nameof(OverrideFadeOut)), EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.Crimson)]
            public AnimationCurve FadeOutCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

            [Space(15f)]

            [Tooltip("If true, waits for the current music fade out before fading in the new one")]
            public bool WaitForFadeOut = true;

            [Tooltip("Delay before playing and fading in the music")]
            [Enhanced, Range(0f, 60f)] public float PlayDelay = 0f;

            [Space(15f)]

            [Tooltip("If true, overrides transition fade in parameters")]
            public bool OverrideFadeIn = false;

            [Tooltip("Transition fade in duration (in seconds)")]
            [Enhanced, ShowIf(nameof(OverrideFadeIn)), Range(0f, 60f)] public float FadeInDuration = 1f;

            [Tooltip("Transition fade in evaluation curve")]
            [Enhanced, ShowIf(nameof(OverrideFadeIn)), EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.Green)]
            public AnimationCurve FadeInCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            #endregion
        }

        #region Global Members
        [Section("Music Asset")]

        [Tooltip("Music audio asset to play")]
        [SerializeField, Enhanced, Required] private AudioAsset music = null;

        [Space(5f)]

        [Tooltip("Default mode used to determines how current music(s) are interrupted when starting to play this one")]
        [SerializeField] private MusicInterruption interruptionMode = MusicInterruption.Pause;

        [Tooltip("Default layer on which to play this music.\nOnly the music on the highest layer priority is actively played")]
        [SerializeField] private AudioLayer layer = AudioLayer.Default;

        [Space(15f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Space(10f, order = 0), Title("Play Transition", order = 1), Space(5f, order = 2)]

        [HelpBox("Transition settings used when interrupting the current music (not whenever played)\nFades out the current music, and fades in this one", MessageType.Info, false)]
        [SerializeField, Enhanced, Block] private TransitionSettings playSettings = new TransitionSettings();

        [Space(15f), HorizontalLine(SuperColor.Grey, 1f), Space(5f)]

        [Space(10f, order = 0), Title("Stop Transition", order = 1), Space(5f, order = 2)]

        [HelpBox("Transition settings used when stopping this music (not when interrupted)\nFades out this music, and fades in this new one", MessageType.Info, false)]
        [SerializeField, Enhanced, Block] private TransitionSettings stopSettings = new TransitionSettings();

        // -----------------------

        /// <summary>
        /// <see cref="AudioAsset"/> wrapped in this music.
        /// </summary>
        public AudioAsset Music {
            get { return music; }
        }

        /// <summary>
        /// Audio layer on which to play this music.
        /// </summary>
        public AudioLayer Layer {
            get { return layer; }
        }

        /// <summary>
        /// Mode used to interrupt the current music(s) when playing this one.
        /// </summary>
        public MusicInterruption InterruptionMode {
            get { return interruptionMode; }
        }

        /// <summary>
        /// Transition settings used when playing this music.
        /// </summary>
        public TransitionSettings PlaySettings {
            get { return playSettings; }
        }

        /// <summary>
        /// Transition settings used when stopping playing this music.
        /// </summary>
        public TransitionSettings StopSettings {
            get { return stopSettings; }
        }
        #endregion

        #region Behaviour
        [NonSerialized] private MusicHandler handler = default;

        // -----------------------

        /// <inheritdoc cref="PlayMusic(AudioLayer, MusicInterruption)"/>
        public MusicHandler PlayMusic() {
            return PlayMusic(layer, interruptionMode);
        }

        /// <inheritdoc cref="PlayMusic(AudioLayer, MusicInterruption)"/>
        public MusicHandler PlayMusic(AudioLayer _layer) {
            return PlayMusic(_layer, interruptionMode);
        }

        /// <summary>
        /// Plays this music.
        /// </summary>
        /// <inheritdoc cref="AudioManager.PlayMusic(AudioMusicAsset, AudioLayer, MusicInterruption)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MusicHandler PlayMusic(AudioLayer _layer, MusicInterruption _interruptionMode) {
            return AudioManager.Instance.PlayMusic(this, _layer, _interruptionMode);
        }

        // -------------------------------------------
        // Handler
        // -------------------------------------------

        /// <summary>
        /// Sets this music current handler.
        /// </summary>
        /// <param name="_handler">This music current handler.</param>
        internal void SetHandler(MusicHandler _handler) {
            handler = _handler;
        }

        /// <summary>
        /// Get the last created <see cref="MusicHandler"/> to play this music.
        /// </summary>
        /// <param name="_handler">Last created <see cref="MusicHandler"/> for this music.</param>
        /// <returns>True if this music handler is valid, false otherwise.</returns>
        public bool GetHandler(out MusicHandler _handler) {
            _handler = handler;
            return _handler.IsValid;
        }

        // -------------------------------------------
        // Button(s)
        // -------------------------------------------

        /// <summary>
        /// Plays this music (for editor use).
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.Green, IsDrawnOnTop = false), DisplayName("Play")]
        private void PlayDebug(bool _resume) {
            if (_resume && GetHandler(out MusicHandler _handler)) {
                _handler.Resume();
            } else {
                PlayMusic();
            }
        }

        /// <summary>
        /// Pauses this music (for editor use).
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.Orange, IsDrawnOnTop = false), DisplayName("Pause")]
        private void PauseDebug(bool _instant) {
            if (GetHandler(out MusicHandler _handler)) {
                _handler.Pause(_instant);
            }
        }

        /// <summary>
        /// Stops this music (for editor use).
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.Crimson, IsDrawnOnTop = false), DisplayName("Stop")]
        private void StopDebug(bool _instant, bool _fallAsleep) {
            if (GetHandler(out MusicHandler _handler)) {
                _handler.Stop(_instant, _fallAsleep);
            }
        }
        #endregion
    }
}
