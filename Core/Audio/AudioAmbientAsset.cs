// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="ScriptableObject"/> data holder for an audio-related ambient asset.
    /// </summary>
    [CreateAssetMenu(fileName = "AMB_Ambient", menuName = FrameworkUtility.MenuPath + "Audio/Ambient", order = FrameworkUtility.MenuOrder)]
    public sealed class AudioAmbientAsset : EnhancedScriptableObject, IWeightControl {
        #region Global Members
        [Section("Audio Ambient Asset")]

        [Tooltip("Main audio of this ambient, played in background")]
        [SerializeField, Enhanced, Required] private AudioAsset mainAudio = null;

        [Tooltip("All sounds to randomly play within this ambient")]
        [SerializeField] private AudioAmbientSoundAsset[] sounds = new AudioAmbientSoundAsset[0];

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Priority of this ambient - ambients with a lower priority can be overriden by those with a higher priority")]
        [SerializeField, Enhanced, Range(0f, 99f)] private int priority = 0;

        [Space(10f)]

        [Tooltip("Modifier applied to the volume of this ambient main audio")]
        [SerializeField, Enhanced, Range(0f, 1f)] private float mainVolume = 1f;

        [Tooltip("Modifier applied to the volume of each sounds in this ambient")]
        [SerializeField, Enhanced, Range(0f, 1f)] private float soundVolume = 1f;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Duration used for fading in this ambient (0 for instant)")]
        [SerializeField, Enhanced, Range(0f, 5f)] private float fadeInDuration = .5f;

        [Tooltip("Duration used for fading out this ambient (0 for instant)")]
        [SerializeField, Enhanced, Range(0f, 5f)] private float fadeOutDuration = .2f;

        [Space(5f)]

        [Tooltip("Curve used for fading in this ambient")]
        [SerializeField, Enhanced, EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.Green)]
        private AnimationCurve fadeInCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Tooltip("Curve used for fading out this ambient")]
        [SerializeField, Enhanced, EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.Crimson)]
        private AnimationCurve fadeOutCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        // -----------------------

        /// <summary>
        /// Main audio of this ambient, played in background.
        /// </summary>
        public AudioAsset MainAudio {
            get { return mainAudio; }
        }

        /// <summary>
        /// Total count of sounds associated with this ambient.
        /// </summary>
        public int SoundCount {
            get { return sounds.Length; }
        }

        /// <summary>
        /// Modifier applied to the volume of this ambient main audio.
        /// </summary>
        public float MainVolume {
            get { return mainVolume; }
        }

        /// <summary>
        /// Modifier applied to the volume of each sound in this ambient.
        /// </summary>
        public float SoundVolume {
            get { return soundVolume; }
        }

        /// <summary>
        /// Default priority of this ambient.
        /// </summary>
        public int Priority {
            get { return priority; }
        }

        /// <inheritdoc cref="IWeightControl.FadeInDuration"/>
        public float FadeInDuration {
            get { return fadeInDuration; }
        }

        /// <inheritdoc cref="IWeightControl.FadeOutDuration"/>
        public float FadeOutDuration {
            get { return fadeOutDuration; }
        }

        /// <inheritdoc cref="IWeightControl.FadeInCurve"/>
        public AnimationCurve FadeInCurve {
            get { return fadeInCurve; }
        }

        /// <inheritdoc cref="IWeightControl.FadeOutCurve"/>
        public AnimationCurve FadeOutCurve {
            get { return fadeOutCurve; }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get an <see cref="AudioAmbientSoundAsset"/> from this ambient at a specific index.
        /// <br/> Use <see cref="SoundCount"/> to get the total count of sounds within this ambient.
        /// </summary>
        /// <param name="_index">Index of the sound to get.</param>
        /// <returns>This ambient <see cref="AudioAmbientSoundAsset"/> at the given index.</returns>
        public AudioAmbientSoundAsset GetSoundAt(int _index) {
            return sounds[_index];
        }
        #endregion
    }
}
