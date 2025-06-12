// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    // ===== Wrapper ===== \\
    
    /// <summary>
    /// <see cref="AudioAmbientSoundAsset"/> player wrapper.
    /// </summary>
    internal sealed class AmbientSoundPlayer : IPoolableObject {
        #region Global Members
        /// <summary>
        /// <see cref="AudioAmbientController"/> associated with this sound.
        /// </summary>
        public AudioAmbientController Ambient = null;

        /// <summary>
        /// <see cref="AudioAmbientSoundAsset"/> to play.
        /// </summary>
        public AudioAmbientSoundAsset Sound = null;

        /// <summary>
        /// <see cref="AudioHandler"/> of this sound current play operation.
        /// </summary>
        public AudioHandler Handler = default;

        /// <summary>
        /// <see cref="DelayHandler"/> of this sound current interval delay operation.
        /// </summary>
        public DelayHandler Delay = default;
        #endregion

        #region Behaviour
        private Action onCompleteCallback = null;
        private Action onPlayCallback     = null;

        // -----------------------

        /// <summary>
        /// Initializes this player for a specific ambient sound.
        /// </summary>
        /// <param name="_sound"><inheritdoc cref="Sound" path="/summary"/></param>
        /// <param name="_ambient"><inheritdoc cref="Ambient" path="/summary"/></param>
        public void Initialize(AudioAmbientSoundAsset _sound, AudioAmbientController _ambient) {
            Stop(true);

            Ambient = _ambient;
            Sound   = _sound;
        }

        /// <summary>
        /// Updates this sound player.
        /// </summary>
        /// <param name="_useRangeArea">Whether the associated ambient uses a range area or not.</param>
        /// <param name="_distance">Actor normalized distance from the ambient center area.</param>
        public void Update(bool _useRangeArea, float _distance) {

            if (_useRangeArea && !Sound.IsInRange(_distance)) {
                Delay.Cancel();
                return;
            }

            // Play interval.
            if (!Delay.IsValid && !GetPlayer(out EnhancedAudioPlayer _player)) {

                onPlayCallback ??= OnPlay;
                Delay = Delayer.Call(Sound.Interval, onPlayCallback, false);
            }
        }

        /// <summary>
        /// Stops playing this sound.
        /// </summary>
        /// <param name="_instant">If true, instantly stops this sound.</param>
        /// <param name="_instant">If true, release this object to pool on completion.</param>
        public void Stop(bool _instant = false, bool _sendToPool = false) {

            Action _callback;
            if (_sendToPool) {

                onCompleteCallback ??= OnStopped;
                _callback = onCompleteCallback;

            } else {
                _callback = null;
            }

            Handler.Stop(_instant, _callback);
            Delay.Cancel();
        }

        // -------------------------------------------
        // Delegate(s)
        // -------------------------------------------

        private void OnPlay() {

            // Play at position.
            Vector3 _position = Ambient.GetPlaySoundSettings(Sound.PlaySoundMode, out float _sourceVolume, out float _weightVolume);
            Handler = Sound.PlaySound(_position);

            // Volume modifier.
            if (GetPlayer(out EnhancedAudioPlayer _player)) {
                _player.PushVolumeModifier(AudioPlayerModifier.AmbientSource, _sourceVolume);
                _player.PushVolumeModifier(AudioPlayerModifier.AmbientWeight, _weightVolume);
            }
        }

        private void OnStopped() {
            AudioManager.Instance.ReleaseAmbientSoundToPool(this);
        }
        #endregion

        #region Pool
        void IPoolableObject.OnCreated(IObjectPool _pool) { }

        void IPoolableObject.OnRemovedFromPool() { }

        void IPoolableObject.OnSentToPool() {

            // Security.
            Stop(true);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get this sound associated <see cref="EnhancedAudioPlayer"/>.
        /// </summary>
        /// <param name="_player">Current <see cref="EnhancedAudioPlayer"/> of this sound.</param>
        /// <returns>True if this sound is currently playing, false otherwise.</returns>
        public bool GetPlayer(out EnhancedAudioPlayer _player) {
            return Handler.GetHandle(out _player);
        }
        #endregion
    }

    // ===== Component ===== \\

    /// <summary>
    /// <see cref="AudioAmbientAsset"/>-related <see cref="EnhancedBehaviour"/> controller.
    /// </summary>
    [AddComponentMenu(FrameworkUtility.MenuPath + "Audio/Ambient Controller"), DisallowMultipleComponent]
    public sealed class AudioAmbientController : AudioWeightControllerBehaviour, IWeightControl {
        #region Global Members
        [Section("Ambient Controller"), PropertyOrder(0)]

        [Tooltip("Ambient wrapped in this object")]
        [SerializeField, Enhanced, Required] private AudioAmbientAsset ambient = null;

        [Tooltip("All additionnal audios associated with this ambient")]
        [SerializeField] private AudioAssetController[] additionalSounds = new AudioAssetController[0];

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("If true, overrides the default priority of this ambient")]
        [SerializeField] private bool overridePriority = false;

        [Tooltip("Priority of this ambient")]
        [SerializeField, Enhanced, ShowIf(nameof(overridePriority)), Range(0f, 99f)] private int priority = 0;

        // -----------------------

        [PropertyOrder(10)]
        [SerializeField, Enhanced, ReadOnly, Range(0f, 1f)] private float ambientWeight = 0f;

        // -----------------------

        /// <summary>
        /// Priority of this ambient.
        /// </summary>
        public int Priority {
            get { return overridePriority ? priority : ambient.Priority; }
        }

        /// <inheritdoc cref="IWeightControl.FadeInDuration"/>
        public float FadeInDuration {
            get { return ambient.FadeInDuration; }
        }

        /// <inheritdoc cref="IWeightControl.FadeOutDuration"/>
        public float FadeOutDuration {
            get { return ambient.FadeOutDuration; }
        }

        /// <inheritdoc cref="IWeightControl.FadeInCurve"/>
        public AnimationCurve FadeInCurve {
            get { return ambient.FadeInCurve; }
        }

        /// <inheritdoc cref="IWeightControl.FadeOutCurve"/>
        public AnimationCurve FadeOutCurve {
            get { return ambient.FadeOutCurve; }
        }
        #endregion

        #region Enhanced Behaviour
        private void OnDestroy() {

            // Stop sounds, which might be delayed and try to access this ambient reference.

            ref List<AmbientSoundPlayer> _span = ref sounds;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {
                _span[i].Stop(true);
            }
        }
        #endregion

        #region Behaviour
        private const float TweenVolumeDuration = .2f;

        private readonly ManualCooldown soundUpdateCooldown = new ManualCooldown(.025f);
        private          List<AmbientSoundPlayer> sounds    = new List<AmbientSoundPlayer>();

        private AudioHandler mainHandler = default;

        // -------------------------------------------
        // Activation
        // -------------------------------------------

        protected override void OnActivation() {

            // Audio setup.
            int _count = ambient.SoundCount;

            for (int i = 0; i < _count; i++) {

                AmbientSoundPlayer _sound = AudioManager.Instance.GetAmbientSoundFromPool();
                _sound.Initialize(ambient.GetSoundAt(i), this);

                sounds.Add(_sound);
            }

            ref AudioAssetController[] _span = ref additionalSounds;

            for (int i = _span.Length; i-- > 0;) {
                _span[i].Activate(false);
            }

            mainHandler = ambient.MainAudio.PlayAudio(transform);

            // Activation and initial weight,
            // then registration and ambient weight setup.
            base.OnActivation();

            AudioManager.Instance.PushAmbient(this);
        }

        protected override void OnDeactivation() {
            base.OnDeactivation();

            AudioManager.Instance.PopAmbient(this, !isActiveAndEnabled);

            // Stop audio.
            ref List<AmbientSoundPlayer> _span = ref sounds;
            
            for (int i = _span.Count; i-- > 0;) {
                _span[i].Stop(false, true);
            }

            ref AudioAssetController[] _addSpan = ref additionalSounds;

            for (int i = _addSpan.Length; i-- > 0;) {
                _addSpan[i].Deactivate(false);
            }

            sounds.Clear();
            mainHandler.Stop(false);

            // In case object is being destroyed.
            if (!isActiveAndEnabled && mainHandler.GetHandle(out EnhancedAudioPlayer _player)) {
                _player.StopFollowTransform();
            }
        }

        // -------------------------------------------
        // Controller
        // -------------------------------------------

        protected override void ControllerUpdate() {
            base.ControllerUpdate();

            // Cooldown.
            if (!soundUpdateCooldown.Update(DeltaTime)) {
                return;
            }

            soundUpdateCooldown.Reload();

            // Outside area.
            if (!GetActorDistance(out float _actorDistance)) {
                _actorDistance = -1f;
            }

            // Actor distance.
            Vector2 _range = areaSettings.MinMaxDistance;
            _actorDistance = Mathm.NormalizedValue(_actorDistance, _range.y, _range.x);

            ref List<AmbientSoundPlayer> _span = ref sounds;

            for (int i = _span.Count; i-- > 0;) {
                _span[i].Update(useRangeArea, _actorDistance);
            }
        }

        protected override void SetWeight(float _weight) {
            base.SetWeight(_weight);

            // Don't use the area weight as it is.
            // Instead, use the calculated weight from the audio manager, using the ambients respective priority.
            AudioManager.Instance.SetAmbientWeight(this, _weight, TweenVolumeDuration);
        }

        /// <summary>
        /// Updates this ambient weight.
        /// <br/> Called from the <see cref="AudioManager"/> after calculating this ambient real weight.
        /// </summary>
        /// <param name="_weight">Real weight of this ambient, calculated according to all registered ambients and their priority.</param>
        internal void SetAmbientWeight(float _weight) {

            ambientWeight = _weight;

            // Volume modifier.
            if (mainHandler.GetHandle(out EnhancedAudioPlayer _player)) {
                SetVolume(_player, _weight);
            }

            ref List<AmbientSoundPlayer> _span = ref sounds;

            for (int i = _span.Count; i-- > 0;) {
                if (_span[i].GetPlayer(out _player)) {
                    SetVolume(_player, _weight);
                }
            }

            ref AudioAssetController[] _addSpan = ref additionalSounds;

            for (int i = _addSpan.Length; i-- > 0;) {
                _addSpan[i].SetVolume(AudioPlayerModifier.AmbientWeight, _weight, TweenVolumeDuration);
            }

            // ----- Local Method ----- \\

            static void SetVolume(EnhancedAudioPlayer _player, float _weight) {
                _player.TweenVolumeModifier(AudioPlayerModifier.AmbientWeight, _weight, TweenVolumeDuration);
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get informations used to play a sound from this ambient.
        /// </summary>
        /// <param name="_playMode">Mode used to determine the position where to play the sound.</param>
        /// <param name="_sourceVolume">Source volume modifier of the sound.</param>
        /// <param name="_weightVolume">Weight volume modifier of the sound.</param>
        /// <returns>Position where to play the sound.</returns>
        internal Vector3 GetPlaySoundSettings(AudioAmbientSoundAsset.PlayMode _playMode, out float _sourceVolume, out float _weightVolume) {

            Vector3 _position;
            switch (_playMode) {

                case AudioAmbientSoundAsset.PlayMode.FromListener:
                    _position = AudioManager.Instance.ListenerPosition;
                    break;

                case AudioAmbientSoundAsset.PlayMode.FromActor:
                    _position = isActor ? actor.position : transform.position;
                    break;

                case AudioAmbientSoundAsset.PlayMode.FromAmbientCenter:
                default:
                    _position = transform.position;
                    break;
            }

            _sourceVolume = ambient.SoundVolume;
            _weightVolume = ambientWeight;

            return _position;
        }
        #endregion
    }
}
