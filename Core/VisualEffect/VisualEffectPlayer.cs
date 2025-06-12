// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using UnityEngine;
using UnityEngine.VFX;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="VisualEffectPlayer"/>-related wrapper for a single play operation.
    /// </summary>
    public struct VisualEffectHandler : IHandler<VisualEffectPlayer> {
        #region Global Members
        private Handler<VisualEffectPlayer> handler;

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

        /// <inheritdoc cref="VisualEffectHandler(VisualEffectPlayer, int)"/>
        public VisualEffectHandler(VisualEffectPlayer _player) {
            handler = new Handler<VisualEffectPlayer>(_player);
        }

        /// <param name="_player"><see cref="VisualEffectPlayer"/> used for playing effects.</param>
        /// <param name="_id">ID of the associated effect play operation.</param>
        /// <inheritdoc cref="VisualEffectHandler"/>
        public VisualEffectHandler(VisualEffectPlayer _player, int _id) {
            handler = new Handler<VisualEffectPlayer>(_player, _id);
        }
        #endregion

        #region Utility
        public bool GetHandle(out VisualEffectPlayer _player) {
            return handler.GetHandle(out _player) && (_player.Status != VisualEffectPlayer.State.Inactive);
        }

        /// <summary>
        /// Plays or resumes this handle associated <see cref="VisualEffectPlayer"/>.
        /// </summary>
        /// <inheritdoc cref="VisualEffectPlayer.Play()"/>
        public bool Play() {
            if (!GetHandle(out VisualEffectPlayer _player)) {
                return false;
            }

            return _player.Play();
        }

        /// <summary>
        /// Pauses this handle associated <see cref="VisualEffectPlayer"/>.
        /// </summary>
        /// <inheritdoc cref="VisualEffectPlayer.Pause"/>
        public bool Pause() {
            if (!GetHandle(out VisualEffectPlayer _player)) {
                return false;
            }

            return _player.Pause();
        }

        /// <summary>
        /// Stops this handle associated <see cref="VisualEffectPlayer"/>.
        /// </summary>
        /// <inheritdoc cref="VisualEffectPlayer.Stop"/>
        public bool Stop(ParticleSystemStopBehavior _behaviour = ParticleSystemStopBehavior.StopEmitting) {
            if (!GetHandle(out VisualEffectPlayer _player)) {
                return false;
            }

            return _player.Stop(_behaviour);
        }

        // -------------------------------------------
        // Event
        // -------------------------------------------

        /// <inheritdoc cref="SendEvent(int)"/>
        /// <inheritdoc cref="VisualEffectPlayer.SendEvent(string)"/>
        public bool SendEvent(string _eventName) {
            if (!GetHandle(out VisualEffectPlayer _player)) {
                return false;
            }

            return _player.SendEvent(_eventName);
        }

        /// <summary>
        /// Sends an event to this handle associated <see cref="VisualEffectPlayer"/>.
        /// </summary>
        /// <inheritdoc cref="VisualEffectPlayer.SendEvent(int)"/>
        public bool SendEvent(int _eventId) {
            if (!GetHandle(out VisualEffectPlayer _player)) {
                return false;
            }

            return _player.SendEvent(_eventId);
        }

        // -------------------------------------------
        // Other
        // -------------------------------------------

        /// <summary>
        /// Sets the local follow offset of this handle associated <see cref="VisualEffectPlayer"/>.
        /// </summary>
        /// <inheritdoc cref="VisualEffectPlayer.SetFollowOffset(Vector3)"/>
        public bool SetFollowOffset(Vector3 _localOffset) {
            if (!GetHandle(out VisualEffectPlayer _player)) {
                return false;
            }

            _player.SetFollowOffset(_localOffset);
            return true;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="IPoolableObject"/> utility component used to play any <see cref="VisualEffectAsset"/>.
    /// </summary>
    [AddComponentMenu(FrameworkUtility.MenuPath + "Visual Effects/Visual Effect Player"), DisallowMultipleComponent]
    #pragma warning disable CS0414
    public sealed class VisualEffectPlayer : EnhancedPoolableObject, IHandle {
        #region State
        /// <summary>
        /// References all available states for an <see cref="VisualEffectPlayer"/>.
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
        [Section("Visual Effect Player")]

        [SerializeField, Enhanced, Required] private VisualEffectAsset effectAsset = null;

        [Space(5f)]

        [Tooltip("If true, automatically starts playing this effect right after the scene finished loading")]
        [SerializeField] private bool playAfterLoading = false;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, ReadOnly] private State state = State.Inactive;
        [SerializeField, Enhanced, ReadOnly] private int playID = 0;

        [Space(10f)]

        [SerializeField, Enhanced, ReadOnly] private bool followTransform = false;
        [SerializeField, Enhanced, ReadOnly] private Vector3 followOffset = Vector3.zero;
        [SerializeField, Enhanced, ReadOnly] private Transform referenceTransform = null;

        // -----------------------

        [HideInInspector] private VisualEffect[] visualEffects = new VisualEffect[0];

        // -----------------------

        /// <summary>
        /// Current state of this player.
        /// </summary>
        public State Status {
            get { return state; }
        }

        /// <summary>
        /// ID of the current particle play operation, especially used for <see cref="VisualEffectHandler"/> references.
        /// </summary>
        int IHandle.ID {
            get { return playID; }
        }

        /// <summary>
        /// The <see cref="VisualEffectAsset"/> configured in this player.
        /// </summary>
        public VisualEffectAsset EffectAsset {
            get { return effectAsset; }
        }

        /// <summary>
        /// Determines if this effect pool should be cleared when performing a loading operation.
        /// </summary>
        public bool ClearOnLoading {
            get { return effectAsset.ClearOnLoading; }
        }

        /// <summary>
        /// Get if this effect players should keep playing while paused.
        /// </summary>
        public bool IgnorePause {
            get { return effectAsset.IgnorePause; }
        }

        // -----------------------

        #if UNITY_EDITOR
        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, DrawMember(nameof(IsEffectPlaying)),    ReadOnly] private bool isPlaying = false;
        [SerializeField, Enhanced, DrawMember(nameof(AliveParticleCount)), ReadOnly] private int aliveParticleCount = 0;
        #endif

        /// <summary>
        /// Get if any <see cref="VisualEffect"/> is currently playing or not.
        /// </summary>
        public bool IsEffectPlaying {
            get {
                ref VisualEffect[] _span = ref visualEffects;
                for (int i = _span.Length; i-- > 0;) {

                    if (_span[i].HasAnySystemAwake()) {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Get if there is currently any alive particle in this effect.
        /// </summary>
        public int AliveParticleCount {
            get {
                int _count = 0;

                ref VisualEffect[] _span = ref visualEffects;
                for (int i = _span.Length; i-- > 0;) {
                    _count += _span[i].aliveParticleCount;
                }

                return _count;
            }
        }
        #endregion

        #region Enhanced Behaviour
        private bool hasPlayed = false;

        // -----------------------

        protected override void OnPlay() {
            base.OnPlay();

            // Play once loading is over.
            if (playAfterLoading) {
                playAfterLoading = false;
                Play();
            }
        }

        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Registration.
            VisualEffectManager.RegisterPlayer(this);
        }

        /// <summary>
        /// Called from the <see cref="VisualEffectManager"/> to update this player.
        /// </summary>
        internal void EffectUpdate() {
            // State update.
            switch (state) {
                // Stop detection.
                case State.Playing:

                    bool _isAlive = false;
                    ref VisualEffect[] _span = ref visualEffects;

                    for (int i = 0; i < _span.Length; i++) {

                        VisualEffect _effect = _span[i];
                        bool _isPlaying = _effect.HasAnySystemAwake() || (_effect.aliveParticleCount > 0);

                        if (_isPlaying) {

                            _isAlive  = true;
                            hasPlayed = true;
                            break;
                        }
                    }

                    if (!_isAlive && hasPlayed) {
                        Stop(ParticleSystemStopBehavior.StopEmittingAndClear);
                    }

                    break;

                // Ignore.
                case State.Inactive:
                case State.Paused:
                case State.Delay:
                default:
                    hasPlayed = false;
                    return;
            }

            // Follow.
            UpdateFollow();
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Stop playing.
            Stop(ParticleSystemStopBehavior.StopEmittingAndClear);

            // Unregistration.
            VisualEffectManager.UnregisterPlayer(this);
        }
        #endregion

        #region Behaviour
        private static int lastPlayID = 0;

        private Action onPlayCallback = null;
        private DelayHandler delay = default;

        // -----------------------


        /// <inheritdoc cref="VisualEffectManager.Play(VisualEffectAsset, Vector3)"/>
        public VisualEffectHandler Play(Vector3 _position) {
            VisualEffectHandler _player = DoPlay();
            transform.position = _position + effectAsset.LocalOffset.Rotate(transform.rotation);

            return _player;
        }

        /// <inheritdoc cref="VisualEffectManager.Play(VisualEffectAsset, Transform)"/>
        public VisualEffectHandler Play(Transform _transform) {
            VisualEffectHandler _player = DoPlay();
            FollowTransform(_transform);

            return _player;
        }

        private VisualEffectHandler DoPlay() {
            Stop(ParticleSystemStopBehavior.StopEmitting, false);

            // Stop previous player.
            if (effectAsset.AvoidDuplicate && effectAsset.GetHandler(out VisualEffectHandler _currentHandler)) {
                this.LogWarning($"Interrupting previous effect - {effectAsset.name.Bold()}");
                _currentHandler.Stop();
            }

            // ID.
            playID = ++lastPlayID;

            #if UNITY_EDITOR
            // Hierachy utility.
            if (IsFromPool) {
                gameObject.name = $"{gameObject.name.GetPrefix()}{effectAsset.name.RemovePrefix()} - [{playID}]";
            }
            #endif

            // Delay.
            float _delay = effectAsset.Delay;
            if (_delay != 0f) {

                SetState(State.Delay);

                onPlayCallback ??= OnPlay;
                delay = Delayer.Call(_delay, onPlayCallback, true);

            } else {
                OnPlay();
            }

            // Play.
            VisualEffectHandler _handler = new VisualEffectHandler(this, playID);
            effectAsset.SetHandler(_handler);

            return _handler;

            // ----- Local Method ----- \\

            void OnPlay() {

                // Play.
                SetState(State.Playing);

                ref VisualEffect[] _span = ref visualEffects;
                int _count = _span.Length;

                for (int i = 0; i < _count; i++) {
                    _span[i].Play();
                }
            }
        }

        // -------------------------------------------
        // Initialization
        // -------------------------------------------

        /// <summary>
        /// Initializes this player for a specific <see cref="VisualEffectAsset"/>.
        /// <br/> Should only be called once, on creation.
        /// </summary>
        /// <param name="_particle"><see cref="VisualEffectAsset"/> to initialize this player with.</param>
        public void Initialize(VisualEffectAsset _particle) {
            effectAsset   = _particle;
            visualEffects = GetComponentsInChildren<VisualEffect>();
        }

        // -------------------------------------------
        // Core
        // -------------------------------------------

        /// <summary>
        /// Plays or resumes all this player pre-configured effects.
        /// </summary>
        /// <returns>True if this player could be successfully played, false otherwise.</returns>
        [Button(SuperColor.Green)]
        public bool Play() {

            #if UNITY_EDITOR
            if (!Application.isPlaying && (state == State.Playing)) {
                Stop(ParticleSystemStopBehavior.StopEmittingAndClear, false);
            }
            #endif

            switch (state) {
                // Ignore.
                case State.Playing:
                    this.LogWarningMessage("Effects are already being played");
                    return false;

                // Resume.
                case State.Paused:
                    SetState(State.Playing);

                    ref VisualEffect[] _span = ref visualEffects;
                    int _count = _span.Length;

                    for (int i = 0; i < _count; i++) {
                        VisualEffect _effect = _span[i];

                        if (_effect.pause) {
                            _effect.pause = false;
                        } else {
                            _effect.Play();
                        }
                    }
                    break;

                case State.Delay:
                    delay.Resume();
                    return true;

                // Play from start.
                case State.Inactive:
                    DoPlay();
                    return true;

                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// Pauses this effect player.
        /// </summary>
        /// <returns>True if this player could be successfully paused, false otherwise.</returns>
        [Button(SuperColor.Orange)]
        public bool Pause() {
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

            // Pause.
            SetState(State.Paused);

            ref VisualEffect[] _span = ref visualEffects;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                _span[i].pause = true;
            }

            return true;
        }

        /// <summary>
        /// Stops playing this effect player.
        /// </summary>
        /// <param name="_behaviour">Behaviour applied when stopping the effect(s).</param>
        /// <returns>True if this player could be successfully stopped, false otherwise.</returns>
        [Button(SuperColor.Crimson)]
        public bool Stop(ParticleSystemStopBehavior _behaviour = ParticleSystemStopBehavior.StopEmitting, bool _sendToPool = true) {
            switch (state) {
                // Ignore.
                case State.Inactive:
                    return false;

                // Cancel delay.
                case State.Delay:
                    delay.Cancel();
                    break;

                case State.Playing:
                case State.Paused:
                default:
                    break;
            }

            ref VisualEffect[] _span = ref visualEffects;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                _span[i].Stop();
            }

            switch (_behaviour) {
                // Stop.
                case ParticleSystemStopBehavior.StopEmittingAndClear:

                    StopFollowTransform();
                    SetState(State.Inactive);

                    hasPlayed = false;

                    // Send back to pool.
                    if (IsFromPool && _sendToPool) {
                        Release();
                    }

                    break;

                case ParticleSystemStopBehavior.StopEmitting:
                default:
                    break;
            }

            return true;
        }
        #endregion

        #region Event
        /// <param name="_eventName">Name of the event to send.</param>
        /// <inheritdoc cref="SendEvent(int)"/>
        public bool SendEvent(string _eventName) {
            return SendEvent(Shader.PropertyToID(_eventName));
        }

        /// <summary>
        /// Sends an event on this event system.
        /// </summary>
        /// <param name="_eventId">Id of the event to send.</param>
        /// <returns>True if the event was send to the associated <see cref="VisualEffect"/>s, false otherwise.</returns>
        public bool SendEvent(int _eventId) {
            switch (state) {
                // Ignore.
                case State.Inactive:
                    return false;

                case State.Playing:
                case State.Paused:
                case State.Delay:
                default:
                    break;
            }

            ref VisualEffect[] _span = ref visualEffects;
            for (int i = _span.Length; i-- > 0;) {
                _span[i].SendEvent(_eventId);
            }

            return true;
        }
        #endregion

        #region Poolable
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
            followOffset       = Vector3.zero;

            UpdateFollow();
        }

        /// <summary>
        /// Set this player follow target local offset.
        /// </summary>
        /// <param name="_localOffset">Follow target local offset.</param>
        public void SetFollowOffset(Vector3 _localOffset) {
            followOffset = _localOffset;
        }

        /// <summary>
        /// Stops following any reference <see cref="Transform"/>.
        /// </summary>
        public void StopFollowTransform() {
            referenceTransform = null;
            followTransform    = false;
            followOffset       = Vector3.zero;
        }

        /// <summary>
        /// Updates this player to follow its reference transform.
        /// </summary>
        private void UpdateFollow() {
            if (followTransform) {

                try {
                    Quaternion rotation = referenceTransform.rotation;
                    transform.SetPositionAndRotation(referenceTransform.position + (effectAsset.LocalOffset + followOffset).Rotate(rotation), rotation);

                } catch (Exception e) {

                    if ((e is MissingReferenceException) || (e is NullReferenceException)) {
                        this.LogException(e);
                        StopFollowTransform();
                    } else {
                        throw;
                    }
                }
            }
        }
        #endregion
    }
}
