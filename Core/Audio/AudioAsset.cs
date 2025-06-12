// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Override for an audio loop.
    /// </summary>
    public enum LoopOverride {
        None    = 0,

        Loop    = 1,
        NoLoop  = 2,
    }

    /// <summary>
    /// <see cref="ScriptableObject"/> data holder for an audio asset.
    /// </summary>
    [CreateAssetMenu(fileName = "AD_AudioAsset", menuName = FrameworkUtility.MenuPath + "Audio/Audio Asset", order = FrameworkUtility.MenuOrder)]
    public sealed class AudioAsset : EnhancedAssetFeedback, IEnhancedAnimationEvent {
        #region Global Members
        [Section("Audio Asset")]

        [Tooltip("Clips wrapped in this audio asset")]
        [SerializeField] private BlockArray<AudioClip> clips = new BlockArray<AudioClip>();

        [Space(5f)]

        [Tooltip("Mixer group to plug this audio in")]
        [SerializeField] private AudioMixerGroup mixerGroup = null;

        [Tooltip("Settings used to play this audio")]
        [SerializeField] private AudioAssetSettings settings = null;

        [Space(10f)]

        [Tooltip("If true, automatically loops when reaching the end")]
        [SerializeField, Enhanced, DisplayName("Loop")] private bool isLooping = false;

        [Tooltip("If true, do not fade out the audio when completing a loop")]
        [SerializeField, Enhanced, DisplayName("Loop Seemless")] private bool loopSeemless = true;

        [Space(5f)]

        [Tooltip("If true, keeps this audio playing while performing a scene loading operation")]
        [SerializeField, Enhanced, DisplayName("Persistent")] private bool isPersistent = false;

        [Tooltip("If true, ensures that only one instance of this sound is playing at a time")]
        [SerializeField] private bool avoidDuplicate = false;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Priority of this audio. Note that a sound with a high priority is more likely to be stolen by sounds with a lower priority")]
        [SerializeField, Enhanced, Range(0f, 256f)] private int priority        = 128;

        [Tooltip("Overall default volume of this audio")]
        [SerializeField, Enhanced, Range(0f, 1f)] private float volume          = 1f;

        [Tooltip("Frequency range of this audio - use this to slow down or speed it up")]
        [SerializeField, Enhanced, MinMax(-3, 3f)] private Vector2 pitch        = Vector2.one;

        [Tooltip("Determines how much this audio is affected by 3D spatialisation calculations (attenuation, doppler etc) - 0.0 makes it full 2D, 1.0 makes it full 3D")]
        [SerializeField, Enhanced, Range(0f, 1f)] private float spatialBlend    = 1f;

        [Tooltip("Determines how much of the signal this audio is mixing into the global reverb associated with zones." +
                 "[0, 1] is a linear range (like volume), while [1, 1.1] lets you boost the reverb mix by 10 dB")]
        [SerializeField, Enhanced, Range(0f, 1.1f)] private float reverbZoneMix = .5f;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Duration used for fading in this audio (use 0 for instant)")]
        [SerializeField, Enhanced, Range(0f, 5f)] private float fadeInDuration  = 0f;

        [Tooltip("Duration used for fading out this audio (use 0 for instant)")]
        [SerializeField, Enhanced, Range(0f, 5f)] private float fadeOutDuration = 0f;

        [Space(5f)]

        [Tooltip("Curve used for fading in this audio")]
        [SerializeField, Enhanced, EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.Green)]
        private AnimationCurve fadeInCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Tooltip("Curve used for fading out this audio")]
        [SerializeField, Enhanced, EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.Crimson)]
        private AnimationCurve fadeOutCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Space(10f)]

        [Tooltip("If true, use a specific range to play this audio")]
        [SerializeField, Enhanced, DisplayName("Range")] private bool usePlayRange = false;

        [Tooltip("Play range used of this audio (start and end time)")]
        [SerializeField, Enhanced, ShowIf(nameof(usePlayRange)), MinMax(nameof(AudioRange))] private Vector2 playRange = new Vector2(0f, 1f);

        // -----------------------

        /// <summary>
        /// Volume of this audio.
        /// </summary>
        public float Volume {
            get { return volume * settings.VolumeMultiplier; }
        }

        /// <summary>
        /// Random pitch of this audio.
        /// </summary>
        public float Pitch {
            get { return pitch.Random(); }
        }

        /// <summary>
        /// Minimum duration of this audio clip, in second(s).
        /// </summary>
        public float ClipDuration {
            get {
                ref AudioClip[] _span = ref clips.Array;
                float _duration = 0f;

                for (int i = _span.Length; i-- > 0;) {

                    AudioClip _clip = _span[i];

                    if (_clip != null) {
                        _duration = (_duration == 0f)
                                  ? _clip.length
                                  : Mathf.Min(_duration, _clip.length);
                    }
                }

                return _duration;
            }
        }

        /// <summary>
        /// Minimum duration of this audio asset, in seconds.
        /// </summary>
        public float Duration {
            get { return PlayRange.y;  }
        }

        /// <summary>
        /// Maximum sample duration of this audio, in second(s).
        /// </summary>
        public float SampleDuration {
            get {
                ref AudioClip[] _span = ref clips.Array;
                int _count = _span.Length;

                if (_count == 0) {
                    return 0f;
                }

                AudioClip _audio = null;
                for (int i = 0; i < _count; i++) {

                    AudioClip _clip = _span[i];
                    if ((_clip != null) && (_audio.IsNull() || (_audio.length < _clip.length))) {
                        _audio = _clip;
                    }
                }

                return _audio.samples / _audio.frequency;
            }
        }

        /// <summary>
        /// Whether to make the audio looping or not.
        /// </summary>
        public bool Loop {
            get { return isLooping; }
        }

        /// <summary>
        /// Whether to seemlessly loop or to perform a fade out at the end.
        /// </summary>
        public bool LoopSeemless {
            get { return loopSeemless; }
        }

        /// <summary>
        /// Range of this audio (between 0 and its duration).
        /// </summary>
        public Vector2 AudioRange {
            get { return new Vector2(0f, ClipDuration); }
        }

        /// <summary>
        /// Play range of this audio.
        /// </summary>
        public Vector2 PlayRange {
            get { return usePlayRange ? playRange : AudioRange; }
        }

        /// <summary>
        /// Determines if this audio should keeps playing while performing a scene loading operation.
        /// </summary>
        public bool IsPersistent {
            get { return isPersistent; }
        }

        /// <summary>
        /// Determines if only one instance of this audio should be played at a time.
        /// </summary>
        public bool AvoidDuplicate {
            get { return avoidDuplicate; }
        }

        /// <summary>
        /// Default settings used to play this audio.
        /// </summary>
        public AudioAssetSettings Settings {
            get { return settings; }
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
        [NonSerialized] private AudioHandler handler = default;

        // -----------------------

        /// <inheritdoc cref="PlayAudio(Vector3, AudioAssetSettings)"/>
        public AudioHandler PlayAudio() {
            return AudioManager.Instance.Play(this, settings);
        }

        /// <inheritdoc cref="PlayAudio(Vector3, AudioAssetSettings)"/>
        public AudioHandler PlayAudio(Vector3 _position) {
            return PlayAudio(_position, settings);
        }

        /// <param name="_position"><inheritdoc cref="AudioManager.Play(AudioAsset, AudioAssetSettings, Vector3)" path="/param[@name='_position']"/></param>
        /// <inheritdoc cref="PlayAudio(Transform, AudioAssetSettings)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AudioHandler PlayAudio(Vector3 _position, AudioAssetSettings _settings) {
            return AudioManager.Instance.Play(this, _settings, _position);
        }

        /// <inheritdoc cref="PlayAudio(Transform, AudioAssetSettings)"/>
        public AudioHandler PlayAudio(Transform _transform) {
            return PlayAudio(_transform, settings);
        }

        /// <summary>
        /// Plays this audio.
        /// </summary>
        /// <inheritdoc cref="AudioManager.Play(AudioAsset, AudioAssetSettings, Transform)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AudioHandler PlayAudio(Transform _transform, AudioAssetSettings _settings) {
            return AudioManager.Instance.Play(this, _settings, _transform);
        }

        // -------------------------------------------
        // Handler
        // -------------------------------------------

        /// <summary>
        /// Sets this audio current handler.
        /// </summary>
        /// <param name="_handler">This audio current handler.</param>
        internal void SetHandler(AudioHandler _handler) {
            handler = _handler;
        }

        /// <summary>
        /// Get the last created <see cref="AudioHandler"/> to play this audio.
        /// </summary>
        /// <param name="_handler">Last created <see cref="AudioHandler"/> for this audio.</param>
        /// <returns>True if this audio handler is valid, false otherwise.</returns>
        public bool GetHandler(out AudioHandler _handler) {
            _handler = handler;
            return _handler.IsValid;
        }

        // -------------------------------------------
        // Feedback
        // -------------------------------------------

        protected override void DoPlay(FeedbackPlayOptions _options, Transform _transform, Vector3 _position) {
            // Instant play (delay managed in the player).
            OnPlay(_options,  _transform, _position);
        }

        protected override void OnPlay(FeedbackPlayOptions _options, Transform _transform, Vector3 _position) {
            switch (_options) {

                // Play at position.
                case FeedbackPlayOptions.PlayAtPosition:
                    PlayAudio(_position);
                    break;

                // Follow transform.
                case FeedbackPlayOptions.FollowTransform:
                    PlayAudio(_transform);
                    break;

                // Vanilla.
                case FeedbackPlayOptions.None:
                default:
                    PlayAudio();
                    break;
            }
        }

        protected override void OnStop() {
            handler.Stop();
        }

        // -------------------------------------------
        // Button(s)
        // -------------------------------------------

        /// <summary>
        /// Previews this audio asset (for editor use).
        /// </summary>
        [Button(SuperColor.Green, IsDrawnOnTop = false), DisplayName("Preview (2D)")]
        private void PlayPreview() {
            AudioManager.Instance.PlayPreview(this);
        }

        /// <summary>
        /// Pauses this audio asset preview (for editor use).
        /// </summary>
        [Button(SuperColor.Orange, IsDrawnOnTop = false), DisplayName("Pause")]
        private void PausePreview() {
            AudioManager.Instance.PausePreview(this);
        }

        /// <summary>
        /// Stops this audio asset preview (for editor use).
        /// </summary>
        [Button(SuperColor.Crimson, IsDrawnOnTop = false), DisplayName("Stop")]
        private void StopPreview() {
            AudioManager.Instance.StopPreview(this);
        }
        #endregion

        #region Event
        private const float EventDelay = .2f;
        private readonly UnscaledCooldown eventCooldown = new UnscaledCooldown(); // 1 frame cooldown.

        // -----------------------

        /// <inheritdoc cref="IEnhancedAnimationEvent.Invoke(EnhancedBehaviour)"/>
        public void Invoke(EnhancedBehaviour _behaviour) {

            if (!eventCooldown.IsValid) {
                return;
            }

            PlayAudio(_behaviour.transform.position);
            eventCooldown.Reload();
        }
        #endregion

        #region Utility
        private int lastRandomIndex = -1;

        // -----------------------

        /// <inheritdoc cref="SetupAudioSource(AudioSource, AudioAssetSettings)"/>
        public void SetupAudioSource(AudioSource _source) {
            SetupAudioSource(_source, settings);
        }

        /// <summary>
        /// Setups a given <see cref="AudioSource"/> for this audio.
        /// </summary>
        /// <param name="_source"><see cref="AudioSource"/> instance to setup.</param>
        /// <param name="_settings"><see cref="AudioAssetSettings"/> used for setup.</param>
        public void SetupAudioSource(AudioSource _source, AudioAssetSettings _settings) {
            _source.outputAudioMixerGroup   = mixerGroup;
            _source.reverbZoneMix           = reverbZoneMix;
            _source.spatialBlend            = spatialBlend;

            _source.priority    = priority;
            _source.volume      = Volume;
            _source.pitch       = Pitch;
            _source.loop        = isLooping;

            _source.clip = GetClip();

            _source.ignoreListenerPause = AudioManager.Instance.IgnorePause(mixerGroup);
            _source.time = PlayRange.x;

            _settings.ApplyValuesOn(_source);
        }

        /// <summary>
        /// Get a random clip from this audio.
        /// </summary>
        public AudioClip GetClip() {
            return clips.Array.Random(ref lastRandomIndex);
        }
        #endregion
    }
}
