// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core.GameStates;
using System;
using UnityEngine;
using UnityEngine.Playables;

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Enhanced behaviour used to manage a <see cref="PlayableDirector"/>.
    /// </summary>
    [ScriptGizmos(false, true)]
    [RequireComponent(typeof(PlayableDirector))]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Player/Enhanced Playable Player"), DisallowMultipleComponent]
    #pragma warning disable 0414
    public sealed class EnhancedPlayablePlayer : EnhancedPlayer, ISkippableElement {
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
        [Section("Enhanced Video Player")]

        [Tooltip("If true, the Playable will start playing right after the scene finished loading")]
        [SerializeField, Enhanced, ValidationMember(nameof(PlayAfterLoading))]
        private bool playAfterLoading = false;

        [Space(10f)]

        [HelpBox("Whether the Playable can be skipped or not.\nHas no effect by itself. Should be used from another script (like an IBoundGameState).", MessageType.Info)]
        [SerializeField] private bool isSkippable = false;

        [Tooltip("The time (in seconds) where to advance the Playable to when being skipped.")]
        [SerializeField, Enhanced, ShowIf(nameof(IsSkippable)), MinMax(nameof(TimeRange))] private Vector2 skipInterval = new Vector2();

        [Space(10f)]

        #if UNITY_EDITOR
        [SerializeField, Enhanced, DrawMember(nameof(Time)), Range(nameof(TimeRange)), ValidationMember(nameof(Time))]
        private double time = 0d; // Should never be used outside of the inspector.
        #endif

        [SerializeField, Tooltip("GameState that will automatically be created and added on stack when the Playable starts playing, and destroyed when it stops")]
        private SerializedType<GameState> boundGameState = new SerializedType<GameState>(null, SerializedTypeConstraint.Null,
                                                                                         new Type[] { typeof(IBoundGameState),
                                                                                                      typeof(IBoundGameState<ISkippableElement>),
                                                                                                      typeof(IBoundGameState<EnhancedPlayablePlayer>) });
        // -----------------------

        [SerializeField, HideInInspector] private PlayableDirector playableDirector = null;

        // -----------------------

        /// <inheritdoc/>
        public bool IsSkippable {
            get { return isSkippable; }
            set {
                isSkippable = value;
            }
        }

        /// <summary>
        /// <inheritdoc cref="PlayableDirector.playableAsset"/>
        /// (From <see cref="PlayableDirector.playableAsset"/>)
        /// </summary>
        public PlayableAsset Playable {
            get {
                return playableDirector.playableAsset;
            }
            set {
                Stop();
                playableDirector.playableAsset = value;

                if (isSkippable) {
                    SkipInterval = new Vector2(0f, (float)Duration);
                }
            }
        }

        /// <summary>
        /// Whether the Playable should start playing right after loading or not.
        /// </summary>
        public bool PlayAfterLoading {
            get { return playAfterLoading; }
            set {
                if (value) {
                    playableDirector.playOnAwake = false;
                }

                playAfterLoading = value;
            }
        }

        /// <summary>
        /// The time (in seconds) where to advance the Playable to when being skipped.
        /// <para/>
        /// (See also <see cref="IsSkippable"/>)
        /// </summary>
        public Vector2 SkipInterval {
            get { return skipInterval; }
            set {
                Vector2 _range = TimeRange;
                skipInterval = new Vector2(Mathf.Clamp(value.x, _range.x, _range.y), Mathf.Clamp(value.y, _range.x, _range.y));
            }
        }

        /// <summary>
        /// <inheritdoc cref="PlayableDirector.time"/>
        /// (From <see cref="PlayableDirector.time"/>)
        /// </summary>
        public double Time {
            get { return playableDirector.time; }
            set {
                playableDirector.time = value;
            }
        }

        /// <summary>
        /// The range of the Playable total time (betwwen 0 and its duration).
        /// </summary>
        public Vector2 TimeRange {
            get { return new Vector2(0f, (float)Duration); }
        }

        // -----------------------

        #if UNITY_EDITOR
        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, DrawMember(nameof(Duration)), ReadOnly] private double duration  = 0d;

        [Space(10f)]

        [SerializeField, Enhanced, DrawMember(nameof(IsPlaying)), ReadOnly] private bool isPlaying  = false;
        [SerializeField, Enhanced, DrawMember(nameof(IsPaused)), ReadOnly] private bool isPaused    = false;
        #endif

        /// <summary>
        /// <inheritdoc cref="PlayableDirector.duration"/>
        /// (From <see cref="PlayableDirector.duration"/>)
        /// </summary>
        public double Duration {
            get { return playableDirector.duration; }
        }

        /// <summary>
        /// Whether the Playable is currently playing or not.
        /// </summary>
        public bool IsPlaying {
            get { return playableDirector.state == PlayState.Playing; }
        }

        /// <summary>
        /// Whether the Playable is currently paused or not.
        /// </summary>
        public bool IsPaused {
            get { return playableDirector.state == PlayState.Paused; }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnPlay() {
            base.OnPlay();

            // Play once loading is over.
            if (playAfterLoading) {

                playAfterLoading = false;
                PlayFromStart();
            }
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();
            Stop();
        }

        // -------------------------------------------
        // Editor
        // -------------------------------------------

        #if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            // Reference.
            if (!playableDirector) {
                playableDirector = GetComponent<PlayableDirector>();
                playableDirector.playOnAwake = false;
            }
        }
        #endif
        #endregion

        #region Player
        private Action<PlayableDirector> onStopCallback = null;

        private GameState gameState = null;
        private bool isActive       = false;

        // -----------------------

        /// <summary>
        /// <inheritdoc cref="PlayableDirector.Play"/>
        /// (From <see cref="PlayableDirector.Play"/>)
        /// </summary>
        public override void Play() {
            // Do not execute if already playing.
            if (IsPlaying) {
                return;
            }

            playableDirector.Play();

            // Active while started playing and not stopped yet.
            if (isActive) {
                return;
            }

            isActive = true;

            // Stop and pause callback.
            onStopCallback ??= OnStop;

            playableDirector.stopped -= onStopCallback;
            playableDirector.stopped += onStopCallback;

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
            #endif

            // Game State.
            Type _gameStateType = boundGameState.Type;

            if ((_gameStateType != null) && !gameState.IsActive()) {
                gameState = GameState.CreateState(_gameStateType);

                // Bound this player.
                switch (gameState) {
                    case IBoundGameState<EnhancedPlayablePlayer> _boundPlayer:
                        _boundPlayer.Bound(this);
                        break;

                    case IBoundGameState<ISkippableElement> _boundSkip:
                        _boundSkip.Bound(this);
                        break;

                    case IBoundGameState _bound:
                        _bound.Bound(this);
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="PlayableDirector.Pause"/>
        /// (From <see cref="PlayableDirector.Pause"/>)
        /// </summary>
        public override void Pause() {
            playableDirector.Pause();
        }

        /// <summary>
        /// <inheritdoc cref="PlayableDirector.Stop"/>
        /// (From <see cref="PlayableDirector.Stop"/>)
        /// </summary>
        public override void Stop() {
            playableDirector.Stop();

            // Might happen if PlayMode changed while the isActive value was true.
            if (isActive) {
                OnStop(playableDirector);
            }
        }

        /// <summary>
        /// Skips the Playable by setting its current time to <see cref="SkipTime"/>.
        /// </summary>
        /// <returns>True if this playable could be successfully skipped, false otherwise.</returns>
        [Button(nameof(IsSkippable), ConditionType.True, SuperColor.Lavender)]
        public bool Skip() {
            if (IsSkippable && skipInterval.Contains((float)Time)) {
                Time = skipInterval.y;
                return true;
            }

            return false;
        }

        // -----------------------

        private void OnStop(PlayableDirector _playable) {

            // As strange as it seems, the stopped event of the playable
            // is not called when it reaches its end point in editor,
            // but works perfectly fine on play.

            // Not active if never being played or already stopped.
            if (!isActive || GameManager.IsQuittingApplication) {
                return;
            }

            isActive = false;

            _playable.stopped -= onStopCallback;

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
            #endif

            // Game State.
            if (gameState.IsActive()) {

                gameState.RemoveState();
                gameState = null;
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Plays this <see cref="PlayableAsset"/> from the start.
        /// </summary>
        public void PlayFromStart() {
            Time = 0d;
            Play();
        }

        /// <summary>
        /// Sets the Playable of the associated <see cref="PlayableDirector"/>.
        /// </summary>
        /// <param name="_playable">The new <see cref="PlayableAsset"/> to use.</param>
        /// <param name="_play">Whether to start playing this playable from the start or not.</param>
        public void SetPlayable(PlayableAsset _playable, bool _play = true) {
            Playable = _playable;

            if (_play) {
                PlayFromStart();
            }
        }

        /// <summary>
        /// Get the <see cref="PlayableDirector"/> associated with this object.
        /// </summary>
        public PlayableDirector GetPlayableDirector() {
            return playableDirector;
        }
        #endregion
    }
}
