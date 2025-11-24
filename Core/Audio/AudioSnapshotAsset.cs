// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="ScriptableObject"/> encapsulated <see cref="AudioMixerSnapshot"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "ASP_AudioSnapshot", menuName = FrameworkUtility.MenuPath + "Audio/Snapshot", order = FrameworkUtility.MenuOrder)]
    public sealed class AudioSnapshotAsset : EnhancedScriptableObject, IWeightControl {
        #region Global Members
        [Section("Audio Snapshot")]

        [Tooltip("Snapshot encapsulated in this asset")]
        [SerializeField, Enhanced, Required] private AudioMixerSnapshot snapshot = null;

        [Space(10f)]

        [Tooltip("Default weight of this snapshot - 1 means fully active, 0 for inactive")]
        [SerializeField, Enhanced, Range(0f, 1f)] private float weight = 1f;

        [Tooltip("Default priority of this snapshot - used th blend snapshots when multiple are active")]
        [SerializeField, Enhanced, Range(0f, 99f)] private int priority = 0;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Duration used for fading in this snapshot (use 0 for instant)")]
        [SerializeField, Enhanced, Range(0f, 5f)] private float fadeInDuration = .5f;

        [Tooltip("Duration used for fading out this snapshot (use 0 for instant)")]
        [SerializeField, Enhanced, Range(0f, 5f)] private float fadeOutDuration = .2f;

        [Space(5f)]

        [Tooltip("Curve used for fading in this snapshot")]
        [SerializeField, Enhanced, EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.Green)]
        private AnimationCurve fadeInCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Tooltip("Curve used for fading out this snapshot")]
        [SerializeField, Enhanced, EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.Crimson)]
        private AnimationCurve fadeOutCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Space(10f)]

        [Tooltip("Duration used for updating this snapshot weight (use 0 for instant)")]
        [SerializeField, Enhanced, Range(0f, 5f)] private float updateDuration = .5f;

        // -----------------------

        /// <summary>
        /// Referenced <see cref="AudioMixerSnapshot"/> object.
        /// </summary>
        public AudioMixerSnapshot MixerSnapshot {
            get { return snapshot; }
        }

        /// <summary>
        /// Weight of this snapshot. 1 means fully active, 0 for inactive.
        /// </summary>
        public float Weight {
            get { return weight; }
        }

        /// <summary>
        /// Default priority of this snapshot.
        /// </summary>
        public int Priority {
            get { return priority; }
        }

        /// <summary>
        /// Default transition duration used to update this snapshot weight.
        /// </summary>
        public float UpdateDuration {
            get { return updateDuration; }
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

        #region Behaviour
        /// <inheritdoc cref="Push(float, int, bool)"/>
        [Button(ActivationMode.Play, SuperColor.Green, IsDrawnOnTop = false)]
        public void Push(bool _instant = false) {
            Push(weight, priority, _instant);
        }

        /// <inheritdoc cref="Push(float, int, bool)"/>
        public void Push(float _weight, bool _instant = false) {
            Push(_weight, priority, _instant);
        }

        /// <inheritdoc cref="AudioManager.PushSnapshot(AudioSnapshotAsset, float, int, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(float _weight, int _priority, bool _instant = false)  {
            AudioManager.Instance.PushSnapshot(this, _weight, _priority, _instant);
        }

        /// <inheritdoc cref="AudioManager.PopSnapshot(AudioSnapshotAsset, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Button(ActivationMode.Play, SuperColor.Crimson, IsDrawnOnTop = false)]
        public void Pop() {
            AudioManager.Instance.PopSnapshot(this);
        }
        #endregion
    }
}
