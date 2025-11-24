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
    /// <summary>
    /// <see cref="EnhancedAudioPlayer"/>-related modifiers used to apply coefficients to various audio parameters (volume, pitch...).
    /// </summary>
    public enum AudioPlayerModifier {
        None    = 0,

        Asset   = 1,
        Fade    = 2,

        ControllerSource    = 5,
        ControllerFadeOut   = 6,

        MusicSource         = 7,
        MusicInterruption   = 8,

        AmbientSource       = 9,
        AmbientWeight       = 10,
    }

    /// <summary>
    /// <see cref="EnhancedAudioPlayer"/>-related wrapper for a single play operation.
    /// </summary>
    public struct AudioHandler : IHandler<EnhancedAudioPlayer> {
        #region Global Members
        private Handler<EnhancedAudioPlayer> handler;

        // -----------------------

        public int ID {
            get { return handler.ID; }
        }

        public bool IsValid {
            get { return GetHandle(out _); }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="AudioHandler(EnhancedAudioPlayer, int)"/>
        public AudioHandler(EnhancedAudioPlayer _player) {
            handler = new Handler<EnhancedAudioPlayer>(_player);
        }

        /// <param name="_player"><see cref="EnhancedAudioPlayer"/> used for playing audio.</param>
        /// <param name="_id">ID of the associated player play operation.</param>
        /// <inheritdoc cref="AudioHandler"/>
        public AudioHandler(EnhancedAudioPlayer _player, int _id) {
            handler = new Handler<EnhancedAudioPlayer>(_player, _id);
        }
        #endregion

        #region Utility
        public bool GetHandle(out EnhancedAudioPlayer _player) {
            return handler.GetHandle(out _player) && (_player.Status != EnhancedAudioPlayer.State.Inactive);
        }

        /// <summary>
        /// Plays or resumes this handle associated <see cref="EnhancedAudioPlayer"/>.
        /// </summary>
        /// <inheritdoc cref="EnhancedAudioPlayer.Play()"/>
        public bool Play() {
            if (!GetHandle(out EnhancedAudioPlayer _player)) {
                return false;
            }

            return _player.Play();
        }

        /// <summary>
        /// Pauses this handle associated <see cref="EnhancedAudioPlayer"/>.
        /// </summary>
        /// <inheritdoc cref="EnhancedAudioPlayer.Pause(bool, Action)"/>
        public bool Pause(bool _instant = false, Action _onComplete = null) {
            if (!GetHandle(out EnhancedAudioPlayer _player)) {
                _onComplete?.Invoke();
                return false;
            }

            return _player.Pause(_instant, _onComplete);
        }

        /// <summary>
        /// Stops this handle associated <see cref="EnhancedAudioPlayer"/>.
        /// </summary>
        /// <inheritdoc cref="EnhancedAudioPlayer.Stop(bool, Action)"/>
        public bool Stop(bool _instant = false, Action _onComplete = null) {
            if (!GetHandle(out EnhancedAudioPlayer _player)) {
                _onComplete?.Invoke();
                return false;
            }

            return _player.Stop(_instant, _onComplete);
        }
        #endregion
    }

    /// <summary>
    /// <see cref="IPoolableObject"/> utility component used to play any <see cref="Core.AudioAsset"/>.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Audio/Audio Player"), DisallowMultipleComponent]
    #pragma warning disable
    public sealed class EnhancedAudioPlayer : EnhancedPoolableObject, IHandle {
        #region State
        /// <summary>
        /// References all available states for an <see cref="EnhancedAudioPlayer"/>.
        /// </summary>
        public enum State {
            Inactive    = 0,

            Playing     = 1,
            Paused      = 2,

            Delay       = 5,
        }
        #endregion

        public override UpdateRegistration UpdateRegistration {
            get {
                UpdateRegistration _value = base.UpdateRegistration;
                if (playAfterLoading) {
                    _value |= UpdateRegistration.Play;
                }

                return _value;
            }
        }

        #region Global Members
        [Section("Enhanced Audio Player")]

        [Tooltip("Audio currently playing")]
        [SerializeField, Enhanced, Required] private AudioAsset audioAsset = null;

        [Space(5f)]

        [Tooltip("If true, automatically starts playing this audio right after the scene finished loading")]
        [SerializeField] private bool playAfterLoading = false;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Current state of this player")]
        [SerializeField, Enhanced, ReadOnly] private State state = State.Inactive;

        [Tooltip("Unique identifier of this player")]
        [SerializeField, Enhanced, ReadOnly] private int playID = 0;

        [Space(10f)]

        [SerializeField, Enhanced, ReadOnly] private bool followTransform = false;
        [SerializeField, Enhanced, ReadOnly] private Transform referenceTransform = null;

        // -----------------------

        [SerializeField, HideInInspector] private AudioSource audioSource = null;

        // -----------------------

        /// <summary>
        /// Get if this audio is currently affected by the game general audio pause.
        /// </summary>
        public bool IsAudioPaused {
            get { return !IgnorePause && AudioManager.IsPaused; }
        }

        /// <summary>
        /// Current state of this player.
        /// </summary>
        public State Status {
            get {

                State _state = state;

                if ((_state == State.Delay) && delay.GetHandle(out DelayedCall _call) && (_call.Status == DelayedCall.State.Paused)) {
                    _state = State.Paused;
                }

                return _state;
            }
        }

        /// <summary>
        /// ID of the current audio play operation, especially used for <see cref="AudioHandler"/> references.
        /// </summary>
        int IHandle.ID {
            get { return playID; }
        }

        /// <summary>
        /// The <see cref="Core.AudioAsset"/> configured in this player.
        /// </summary>
        public AudioAsset AudioAsset {
            get { return audioAsset;  }
        }

        /// <summary>
        /// <see cref="UnityEngine.AudioSource"/> wrapped in this player.
        /// </summary>
        public AudioSource AudioSource {
            get { return audioSource; }
        }

        /// <summary>
        /// Determines if this player should keeps playing while performing a loading operation.
        /// </summary>
        public bool IsPersistent {
            get { return audioAsset.IsPersistent; }
        }

        /// <summary>
        /// Get if this audio players ignores audio pause.
        /// </summary>
        public bool IgnorePause {
            get { return audioSource.ignoreListenerPause; }
        }

        /// <summary>
        /// This audio loop mode.
        /// </summary>
        public bool Loop {
            get { return audioSource.loop; }
            set { audioSource.loop = value; }
        }

        // -----------------------

        #if UNITY_EDITOR
        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, DrawMember(nameof(Time)), Range(nameof(TimeRange)), ValidationMember(nameof(Time))]
        private float time = 0f;

        [Space(10f)]

        [SerializeField, Enhanced, DrawMember(nameof(Duration)), ReadOnly] private float duration = 0f;
        [SerializeField, Enhanced, DrawMember(nameof(IsAudioSourcePlaying)), ReadOnly] private bool isPlaying = false;
        #endif

        /// <summary>
        /// <inheritdoc cref="AudioSource.time"/>
        /// (From <see cref="AudioSource.time"/>)
        /// </summary>
        public float Time {
            get { return audioSource.time; }
            set {
                value = Mathf.Clamp(value, TimeRange.x, TimeRange.y);
                audioSource.time = value;
            }
        }

        /// <summary>
        /// The range of the <see cref="AudioSource"/> current clip total time (betwwen 0 and its duration).
        /// </summary>
        public Vector2 TimeRange {
            get { return new Vector2(0f, Duration); }
        }

        /// <summary>
        /// The total duration of the current <see cref="AudioSource"/> clip (in seconds).
        /// </summary>
        public float Duration {
            get {
                AudioClip _clip = audioSource.clip;
                return _clip.IsValid()
                     ? _clip.length
                     : 0f;
            }
        }

        /// <summary>
        /// Whether the <see cref="AudioSource"/> is currently playing or not.
        /// </summary>
        public bool IsAudioSourcePlaying {
            get { return audioSource.isPlaying; }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Registration.
            AudioManager.RegisterPlayer(this);
        }

        protected override void OnPlay() {
            base.OnPlay();

            // Play once loading is over.
            if (playAfterLoading) {

                playAfterLoading = false;
                Play();
            }
        }

        /// <summary>
        /// Called from the <see cref="AudioManager"/> to update this player.
        /// </summary>
        internal void AudioUpdate() {

            // State update.
            switch (state) {

                // Stop detection.
                case State.Playing:

                    float _time = Time;

                    // Instant.
                    if (!audioSource.isPlaying && !IsAudioPaused) {

                        Stop(true);
                        return;
                    }

                    // Play range end.
                    if (_time >= (audioAsset.PlayRange.y - audioAsset.FadeOutDuration)) {

                        bool _instant = _time >= audioAsset.PlayRange.y;

                        if (_instant || !Loop || !audioAsset.LoopSeemless) {

                            Stop(_instant, null, true, false);
                            return;
                        }
                    }
                    break;

                // Ignore.
                case State.Inactive:
                case State.Paused:
                case State.Delay:
                default:
                    return;
            }

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
            #endif

            // Follow.
            UpdateFollow();
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Stop playing.
            Stop(true);

            // Unregistration.
            AudioManager.UnregisterPlayer(this);
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        protected override void OnValidate() {
            base.OnValidate();

            // Initialize references.
            if (!audioSource) {
                audioSource = GetComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
        #endif
        #endregion

        #region Behaviour
        private static int lastPlayID = 0;

        private Action<float> onFadeVolumeSetValue  = null;
        private Action<bool> onFadeVolumeComplete   = null;
        private Action onFadeVolumeCompleteCallback = null;

        private TweenHandler volumeTween = default;
        private DelayHandler delay       = default;

        private Action onPlayCallback = null;
        private bool isFadingOut      = false;

        // -----------------------

        /// <inheritdoc cref="AudioManager.Play(AudioAsset, AudioAssetSettings, Vector3)"/>
        public AudioHandler Play(AudioAsset _audio, AudioAssetSettings _settings, Vector3 _position) {
            AudioHandler _player = Play(_audio, _settings);
            transform.position = _position;

            return _player;
        }

        /// <inheritdoc cref="AudioManager.Play(AudioAsset, AudioAssetSettings, Transform)"/>
        public AudioHandler Play(AudioAsset _audio, AudioAssetSettings _settings, Transform _transform) {
            AudioHandler _player = Play(_audio, _settings);
            FollowTransform(_transform);

            return _player;
        }

        /// <inheritdoc cref="AudioManager.Play(AudioAsset, AudioAssetSettings, Transform)"/>
        public AudioHandler Play(AudioAsset _audio, AudioAssetSettings _settings) {

            // Security.
            Stop(true, null, false);

            // Get current player.
            if (_audio.AvoidDuplicate && _audio.GetHandler(out AudioHandler _currentHandler)) {

                _currentHandler.Play();
                return _currentHandler;
            }

            // Audio setup.
            audioAsset = _audio;
            _audio.SetupAudioSource(audioSource, _settings);

            // ID.
            playID = ++lastPlayID;

            // Modifiers.
            ClearVolumeModifiers();
            ClearPitchModifiers();

            PushVolumeModifier(AudioPlayerModifier.Asset, _audio.Volume);
            PushPitchModifier (AudioPlayerModifier.Asset, _audio.Pitch);

            #if UNITY_EDITOR
            // Hierachy utility.
            if (IsFromPool) {
                gameObject.name = $"{gameObject.name.GetPrefix()}{audioSource.clip.name.RemovePrefix()} - [{playID}]";
            }
            #endif

            // Delay.
            float _delay = _audio.Delay;
            if (_delay  != 0f) {

                SetState(State.Delay);

                onPlayCallback ??= OnPlay;
                delay = Delayer.Call(_delay, onPlayCallback, true);

            } else {
                OnPlay();
            }

            // Play.
            AudioHandler _handler = new AudioHandler(this, playID);
            _audio.SetHandler(_handler);

            return _handler;

            // ----- Local Method ----- \\

            void OnPlay() {

                // Fade.
                FadeVolume(0f, 1f, audioAsset.FadeInDuration, audioAsset.FadeInCurve, false);

                // Play.
                SetState(State.Playing);
                audioSource.Play();
            }
        }

        // -------------------------------------------
        // Core
        // -------------------------------------------

        /// <summary>
        /// Plays or resumes this player pre-configured audio.
        /// </summary>
        /// <returns>True if this player could be successfully played, false otherwise.</returns>
        [Button(SuperColor.Green)]
        public bool Play() {

            // Don't play while paused.
            if (IsAudioPaused) {
                return true;
            }

            #if UNITY_EDITOR
            if (!Application.isPlaying && (state == State.Playing)) {
                Stop(true, null, false);
            }
            #endif

            StopFadeVolume();

            switch (state) {

                // Fade in.
                case State.Playing:
                    FadeVolume(GetVolumeModifier(AudioPlayerModifier.Fade, 0f), 1f, audioAsset.FadeInDuration, audioAsset.FadeInCurve, false);
                    return true;

                // Resume.
                case State.Paused:

                    FadeVolume(GetVolumeModifier(AudioPlayerModifier.Fade, 0f), 1f, audioAsset.FadeInDuration, audioAsset.FadeInCurve, false);
                    SetState(State.Playing);

                    audioSource.Play();
                    return true;

                case State.Delay:
                    delay.Resume();
                    return true;

                // Play from start.
                case State.Inactive:
                    Play(audioAsset, audioAsset.Settings, Vector3.zero);
                    return true;

                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// Pauses this audio player.
        /// </summary>
        /// <param name="_instant">If true, instantly pauses this player, ignoring any fade.</param>
        /// <param name="_onComplete">Called once the player is paused.</param>
        /// <returns>True if this player could be successfully paused, false otherwise.</returns>
        [Button(SuperColor.Orange)]
        public bool Pause(bool _instant = false, Action _onComplete = null) {

            switch (state) {

                // Ignore.
                case State.Inactive:
                case State.Paused:
                    return false;

                case State.Delay:
                    delay.Pause();
                    return true;

                case State.Playing:
                default:
                    break;
            }

            // Stop.
            if (_instant) {

                OnPause();

            } else if (!isFadingOut) {

                // Fade out.
                FadeVolume(0f, GetVolumeModifier(AudioPlayerModifier.Fade, 1f), audioAsset.FadeOutDuration, audioAsset.FadeOutCurve, true, OnPause);
            }

            return true;

            // ----- Local Method ----- \\

            void OnPause() {

                StopFadeVolume();
                SetState(State.Paused);

                audioSource.Pause();

                _onComplete?.Invoke();
            }
        }

        /// <summary>
        /// Stops playing this audio player.
        /// </summary>
        /// <param name="_instant">If true, instantly stops this player, ignoring any fade.</param>
        /// <param name="_onComplete">Called once the player is stopped.</param>
        /// <param name="_sendToPool">Whether to send the object back to the pool or not (you should ignore this parameter).</param>
        /// <param name="_stopLoop">If true, do not replay the audio once stopped if it is looping.</param>
        /// <returns>True if this player could be successfully stopped, false otherwise.</returns>
        [Button(SuperColor.Crimson)]
        public bool Stop(bool _instant = false, Action _onComplete = null, bool _sendToPool = true, bool _stopLoop = true) {

            switch (state) {

                // Ignore.
                case State.Inactive:
                    return false;

                // Instant.
                case State.Paused:
                    OnStop();
                    return true;

                // Cancel delay.
                case State.Delay:
                    delay.Cancel();
                    OnStop();
                    return true;

                case State.Playing:
                default:
                    break;
            }

            // Stop.
            if (_instant) {

                OnStop();

            } else if (!isFadingOut || (_onComplete != null)) {

                // Fade out.
                FadeVolume(0f, GetVolumeModifier(AudioPlayerModifier.Fade, 1f), audioAsset.FadeOutDuration, audioAsset.FadeOutCurve, true, OnStop);
            }

            return true;

            // ----- Local Method ----- \\

            void OnStop() {

                // Restart loop.
                if (!_stopLoop && Loop) {

                    _onComplete?.Invoke();
                    Time = audioAsset.PlayRange.x;

                    Play();
                    return;
                }

                // Stop.
                StopFadeVolume();

                audioSource.Stop();

                StopFollowTransform();
                SetState(State.Inactive);

                // Send back to pool.
                if (IsFromPool && _sendToPool) {
                    AudioManager.Instance.ReleaseAudioPlayerToPool(this);
                }

                _onComplete?.Invoke();
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        private void FadeVolume(float _min, float _max, float _duration, AnimationCurve _curve, bool _fadeOut = false, Action _onComplete = null) {

            // Delegates.
            if (onFadeVolumeSetValue == null) {
                onFadeVolumeSetValue = Set;
                onFadeVolumeComplete = OnComplete;
            }

            // Tween.
            StopFadeVolume(false);

            onFadeVolumeCompleteCallback = _onComplete;

            isFadingOut = _fadeOut;
            volumeTween = Tweener.Tween(_min, _max, onFadeVolumeSetValue, _duration, _curve, true, onFadeVolumeComplete);

            // ----- Local Methods ----- \\

            void Set(float _value) {
                PushVolumeModifier(AudioPlayerModifier.Fade, _value);
            }

            void OnComplete(bool _completed) {

                isFadingOut = false;

                if (_completed) {
                    onFadeVolumeCompleteCallback?.Invoke();
                }
            }
        }

        private void StopFadeVolume(bool _resetModifier = true) {

            volumeTween.Stop();

            // Pop modifier.
            if (_resetModifier) {
                PopVolumeModifier(AudioPlayerModifier.Fade);
            }
        }
        #endregion

        #region Modifiers
        private const float DefaultVolume   = 1f;
        private const float DefaultPitch    = 1f;

        private readonly PairCollection<AudioPlayerModifier, float> volumeModifiers = new PairCollection<AudioPlayerModifier, float>();
        private readonly PairCollection<AudioPlayerModifier, float> pitchModifiers  = new PairCollection<AudioPlayerModifier, float>();

        private readonly PairCollection<AudioPlayerModifier, TweenHandler> volumeTweens = new PairCollection<AudioPlayerModifier, TweenHandler>();
        private readonly PairCollection<AudioPlayerModifier, TweenHandler> pitchTweens  = new PairCollection<AudioPlayerModifier, TweenHandler>();

        // -------------------------------------------
        // Volume
        // -------------------------------------------

        /// <summary>
        /// Pushes and applies a new volume modifier in buffer.
        /// <br/> Modifier value must be betwwen 0 and 1.
        /// </summary>
        /// <param name="_modifier">Modifier to apply.</param>
        /// <param name="_value">Modifier coefficient value (between 0 and 1).</param>
        public void PushVolumeModifier(AudioPlayerModifier _modifier, float _value) {
            volumeModifiers.Set(_modifier, _value);
            UpdateVolume();
        }

        /// <summary>
        /// Pops and removes an previously pushed volume modifier from the buffer.
        /// </summary>
        /// <param name="_modifier">Modifier to remove (same as the one used to push it).</param>
        public void PopVolumeModifier(AudioPlayerModifier _modifier) {
            volumeModifiers.Remove(_modifier);
            UpdateVolume();
        }

        /// <summary>
        /// Tweens a volume modifier to a specific value.
        /// </summary>
        /// <param name="_duration">Tween duration (in seconds).</param>
        /// <returns>Volume tween operation handler.</returns>
        /// <inheritdoc cref="PushVolumeModifier(AudioPlayerModifier, float)"/>
        public TweenHandler TweenVolumeModifier(AudioPlayerModifier _modifier, float _value, float _duration) {

            // Stop previous.
            if (volumeTweens.TryGetValue(_modifier, out TweenHandler _tween)) {
                _tween.Stop();
            }

            // Tween value.
            float _currentValue = GetVolumeModifier(_modifier, 1f);
            _tween = Tweener.Tween(_currentValue, _value, Set, _duration, true, OnComplete);

            volumeTweens.Add(_modifier, _tween);
            return _tween;

            // ----- Local Methods ----- \\

            void Set(float _value) {
                PushVolumeModifier(_modifier, _value);
            }

            void OnComplete(bool _completed) {
                volumeTweens.Remove(_modifier);
            }
        }

        // -------------------------------------------
        // Pitch
        // -------------------------------------------

        /// <summary>
        /// Pushes and applies a new pitch modifier in buffer.
        /// <br/> Modifier value must be betwwen -3 and 3.
        /// </summary>
        /// <param name="_modifier">Modifier to apply.</param>
        /// <param name="_value">Modifier coefficient value (between -3 and 3).</param>
        public void PushPitchModifier(AudioPlayerModifier _modifier, float _value) {
            pitchModifiers.Set(_modifier, _value);
            UpdatePitch();
        }

        /// <summary>
        /// Pops and removes an previously pushed pitch modifier from the buffer.
        /// </summary>
        /// <param name="_modifier">Modifier to remove (same as the one used to push it).</param>
        public void PopPitchModifier(AudioPlayerModifier _modifier) {
            pitchModifiers.Remove(_modifier);
            UpdatePitch();
        }

        /// <summary>
        /// Tweens a pitch modifier to a specific value.
        /// </summary>
        /// <param name="_duration">Tween duration (in seconds).</param>
        /// <returns>Pitch tween operation handler.</returns>
        /// <inheritdoc cref="PushPitchModifier(AudioPlayerModifier, float)"/>
        public TweenHandler TweenPitchModifier(AudioPlayerModifier _modifier, float _value, float _duration) {

            // Stop previous.
            if (pitchTweens.TryGetValue(_modifier, out TweenHandler _tween)) {
                _tween.Stop();
            }

            // Tween value.
            float _currentValue = GetPitchModifier(_modifier, 1f);
            _tween = Tweener.Tween(Mathf.Min(_currentValue, _value), Mathf.Max(_currentValue, _value), Set, _duration, true, OnComplete);

            pitchTweens.Add(_modifier, _tween);
            return _tween;

            // ----- Local Methods ----- \\

            void Set(float _value) {
                PushPitchModifier(_modifier, _value);
            }

            void OnComplete(bool _completed) {
                pitchTweens.Remove(_modifier);
            }
        }

        // -------------------------------------------
        // Update
        // -------------------------------------------

        private void UpdateVolume() {

            ref List<Pair<AudioPlayerModifier, float>> _span = ref volumeModifiers.collection;
            float _volume = DefaultVolume;

            for (int i = _span.Count; i-- > 0;) {
                _volume *= _span[i].Second;
            }

            audioSource.volume = _volume;
        }

        private void UpdatePitch() {

            ref List<Pair<AudioPlayerModifier, float>> _span = ref pitchModifiers.collection;
            float _pitch = DefaultPitch;

            for (int i = _span.Count; i-- > 0;) {
                _pitch *= _span[i].Second;
            }

            audioSource.pitch = _pitch;
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get a specific volume modifier value.
        /// </summary>
        /// <param name="_modifier">Modifier to get the associated value.</param>
        /// <param name="_defaultValue">Default value if the modifier is not active.</param>
        /// <returns>Current modifier value or default.</returns>
        public float GetVolumeModifier(AudioPlayerModifier _modifier, float _defaultValue) {
            if (!volumeModifiers.TryGetValue(_modifier, out float _value)) {
                _value = _defaultValue;
            }

            return _value;
        }

        /// <summary>
        /// Get a specific pitch modifier value.
        /// </summary>
        /// <param name="_modifier">Modifier to get the associated value.</param>
        /// <param name="_defaultValue">Default value if the modifier is not active.</param>
        /// <returns>Current modifier value or default.</returns>
        public float GetPitchModifier(AudioPlayerModifier _modifier, float _defaultValue) {
            if (!pitchModifiers.TryGetValue(_modifier, out float _value)) {
                _value = _defaultValue;
            }

            return _value;
        }

        /// <summary>
        /// Clears all pushed in buffer volume modifiers, and reset the audio volume back to default.
        /// </summary>
        public void ClearVolumeModifiers() {

            ref List<Pair<AudioPlayerModifier, TweenHandler>> _span = ref volumeTweens.collection;

            for (int i = _span.Count; i-- > 0;) {
                _span[i].Second.Stop();
            }

            volumeTweens.Clear();
            volumeModifiers.Clear();

            audioSource.volume = DefaultVolume;
        }

        /// <summary>
        /// Clears all pushed in buffer pitch modifiers, and reset the audio pitch back to default.
        /// </summary>
        public void ClearPitchModifiers() {

            ref List<Pair<AudioPlayerModifier, TweenHandler>> _span = ref pitchTweens.collection;
            
            for (int i = _span.Count; i-- > 0;) {
                _span[i].Second.Stop();
            }

            pitchTweens.Clear();
            pitchModifiers.Clear();

            audioSource.pitch = DefaultPitch;
        }
        #endregion

        #region Poolable
        public override void OnCreated(IObjectPool _pool) {
            base.OnCreated(_pool);

            // Component reference.
            AudioSource _audioSource = gameObject.AddComponentIfNone<AudioSource>();
            _audioSource.playOnAwake = false;

            audioSource = _audioSource;
        }

        public override void OnRemovedFromPool() {
            base.OnRemovedFromPool();

            #if UNITY_EDITOR
            // Push on top of the hierarchy.
            transform.SetAsFirstSibling();
            #endif
        }

        public override void OnSentToPool() {
            base.OnSentToPool();

            #if UNITY_EDITOR
            // Push to the hierarchy bottom.
            transform.SetAsLastSibling();
            #endif
        }

        // -----------------------

        public override void Release() {

            if (IsFromPool) {
                AudioManager.Instance.ReleaseAudioPlayerToPool(this);
                return;
            }

            base.Release();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Sets the current state of this player.
        /// </summary>
        /// <param name="_state">New state of this player.</param>
        private void SetState(State _state) {
            state = _state;
        }

        // -------------------------------------------
        // Follow
        // -------------------------------------------

        /// <summary>
        /// Makes this player follow a specific <see cref="Transform"/> reference.
        /// </summary>
        /// <param name="_transform"><see cref="Transform"/> reference to follow.</param>
        public void FollowTransform(Transform _transform) {
            referenceTransform = _transform;
            followTransform    = true;

            UpdateFollow();
        }

        /// <summary>
        /// Stops following any reference <see cref="Transform"/>.
        /// </summary>
        public void StopFollowTransform() {
            referenceTransform = null;
            followTransform    = false;
        }

        /// <summary>
        /// Updates this player to follow its reference transform.
        /// </summary>
        private void UpdateFollow() {

            if (followTransform) {

                try {
                    transform.position = referenceTransform.position;
                } catch (MissingReferenceException e) {
                    this.LogException(e);
                    StopFollowTransform();
                }
            }
        }
        #endregion
    }
}
