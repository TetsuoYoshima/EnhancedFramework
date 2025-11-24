// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core.GameStates;
using System;
using UnityEngine;
using UnityEngine.Video;

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Enhanced behaviour used to manage a <see cref="VideoPlayer"/>.
    /// <para/>
    /// Allow to preview the video in the editor, and use additional callbacks and functionalities.
    /// </summary>
    [ScriptGizmos(false, true)]
    [RequireComponent(typeof(VideoPlayer))]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Player/Enhanced Video Player"), DisallowMultipleComponent]
    #pragma warning disable 0414
    public sealed class EnhancedVideoPlayer : EnhancedPlayer, ISkippableElement, IStableUpdate, ILoadingProcessor {
        public override UpdateRegistration UpdateRegistration {
            get {
                UpdateRegistration _value = base.UpdateRegistration;

                if (prepareOnInit) {
                    _value |= UpdateRegistration.Init;
                }

                if (playAfterLoading) {
                    _value |= UpdateRegistration.Play;
                }

                return _value;
            }
        }

        #region Loading Processor
        public override bool IsLoadingProcessor => true;

        // True while not yet prepared.
        public bool IsProcessing {
            get {

                if (!prepareOnInit)
                    return false;

                // Best solution is to perform this check here, as the video event can never be called.
                // When prepared, forget this behaviour.

                bool isProcessing = !IsPrepared;
                if (!isProcessing) {
                    prepareOnInit = false;
                }

                return isProcessing;
            }
        }
        #endregion

        #region Global Members
        [Section("Enhanced Video Player")]

        #if UNITY_EDITOR
        [SerializeField, Enhanced, DrawMember(nameof(VideoClip)), ValidationMember(nameof(VideoClip))]
        private VideoClip videoClip = null; // Should never be used outside of the inspector.
        #endif

        [Space(5f)]

        [Tooltip("If true, the video will start playing right after the scene finished loading")]
        [SerializeField, Enhanced, ValidationMember(nameof(PlayAfterLoading))]
        private bool playAfterLoading = false;

        [Tooltip("If true, the video will initiate playaback engine preparation on initialization")]
        [SerializeField] private bool prepareOnInit = true;

        [Tooltip("If true, disables the associated VideoPlayer when it stops playing")]
        [SerializeField] private bool disableOnStop = false;

        [Space(10f)]

        [HelpBox("Whether the video can be skipped or not.\nHas no effect by itself. Should be used from another script (like an IBoundGameState)", MessageType.Info)]
        [SerializeField] private bool isSkippable = false;

        [Tooltip("Time (in seconds) where skip is available and where to advance the video to when being skipped")]
        [SerializeField, Enhanced, ShowIf(nameof(isSkippable)), MinMax(nameof(playRange))] private Vector2 skipInterval = new Vector2();

        [Space(10f)]

        [Tooltip("Use this to automatically show a CanvasGroup when the video start playing, and hide it when it stops")]
        [SerializeField] private bool manageRenderingCanvas = false;
        [SerializeField, Enhanced, ShowIf(nameof(manageRenderingCanvas))] private CrossSceneReference<FadingObjectBehaviour> renderingCanvas = null;

        [Space(10f)]

        [Tooltip("The total range used to play the video (from start to end)")]
        [SerializeField, Enhanced/*, DrawMember("PlayRange")*/, MinMax(nameof(TimeRange))] private Vector2 playRange = new Vector2(0f, 0f);

        #if UNITY_EDITOR
        [SerializeField, Enhanced, DrawMember(nameof(Time)), Range(nameof(playRange)), ValidationMember(nameof(Time))]
        private double time = 0d; // Should never be used outside of the inspector.
        #endif

        [Space(10f)]

        [SerializeField, Tooltip("GameState that will automatically be created when the video start playing, and destroyed when it stops")]
        private SerializedType<GameState> boundGameState = new SerializedType<GameState>(null, SerializedTypeConstraint.Null,
                                                                                         new Type[] { typeof(IBoundGameState),
                                                                                                      typeof(IBoundGameState<ISkippableElement>),
                                                                                                      typeof(IBoundGameState<EnhancedVideoPlayer>) });
        // -----------------------

        [SerializeField, HideInInspector] private VideoPlayer videoPlayer = null;

        // -----------------------

        /// <inheritdoc/>
        public bool IsSkippable {
            get { return isSkippable; }
            set {
                isSkippable = value;
            }
        }

        /// <summary>
        /// <inheritdoc cref="VideoPlayer.clip"/>
        /// (From <see cref="VideoPlayer.clip"/>)
        /// </summary>
        public VideoClip VideoClip {
            get {
                return videoPlayer.clip;
            } set {
                videoPlayer.clip = value;

                Stop();
                playRange = TimeRange;

                if (isSkippable) {
                    SkipInterval = new Vector2(0f, playRange.y);
                }
            }
        }

        /// <summary>
        /// The time range used to play the video (start time as x value, end time as y value).
        /// </summary>
        public Vector2 PlayRange {
            get {
                float _time = (float)Time;
                playRange.x = Mathf.Min(_time, Mathm.CeilToDecimal(playRange.x, 2));
                playRange.y = Mathf.Max(_time, Mathm.FloorToDecimal(playRange.y, 2));

                return playRange;
            } set {
                playRange = value;
            }
        }

        /// <summary>
        /// Whether the Playable should start playing right after loading or not.
        /// </summary>
        public bool PlayAfterLoading {
            get { return playAfterLoading; }
            set {
                if (value) {
                    videoPlayer.playOnAwake = false;
                }

                playAfterLoading = value;
            }
        }

        /// <summary>
        /// The time (in seconds) where to advance the video to when being skipped.
        /// <para/>
        /// (See also <see cref="IsSkippable"/>)
        /// </summary>
        public Vector2 SkipInterval {
            get { return skipInterval; }
            set {
                Vector2 _range = playRange;
                skipInterval = new Vector2(Mathf.Clamp(value.x, _range.x, _range.y), Mathf.Clamp(value.y, _range.x, _range.y));
            }
        }

        /// <summary>
        /// <inheritdoc cref="VideoPlayer.time"/>
        /// (From <see cref="VideoPlayer.time"/>)
        /// </summary>
        public double Time {
            get { return videoPlayer.time; }
            set {
                value = (double)Mathf.Clamp((float)value, playRange.x, playRange.y);
                videoPlayer.time = value;
            }
        }

        /// <summary>
        /// The range of the video total time (betwwen 0 and its duration).
        /// </summary>
        public Vector2 TimeRange {
            get { return new Vector2(0f, (float)Duration); }
        }

        // -----------------------

        #if UNITY_EDITOR
        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, DrawMember(nameof(Duration)), ReadOnly] private double duration    = 0d;

        [Space(10f)]

        [SerializeField, Enhanced, DrawMember(nameof(IsPlaying)),  ReadOnly] private bool isPlaying   = false;
        [SerializeField, Enhanced, DrawMember(nameof(IsPaused)),   ReadOnly] private bool isPaused    = false;
        [SerializeField, Enhanced, DrawMember(nameof(IsPrepared)), ReadOnly] private bool isPrepared  = false;
        #endif

        /// <summary>
        /// The total duration of the video.
        /// </summary>
        public double Duration {
            get { return videoPlayer.length; }
        }

        /// <summary>
        /// Whether the video is currently playing or not.
        /// </summary>
        public bool IsPlaying {
            get { return videoPlayer.isPlaying; }
        }

        /// <summary>
        /// Whether the video is currently paused or not.
        /// </summary>
        public bool IsPaused {
            get { return videoPlayer.isPaused; }
        }

        /// <summary>
        /// Whether the video is already prepared or not.
        /// </summary>
        public bool IsPrepared {
            get { return videoPlayer.isPrepared || !videoPlayer.isActiveAndEnabled; }
        }

        // -----------------------

        /// <summary>
        /// Called when the video starts playing.
        /// </summary>
        public event Action<VideoPlayer> Started    = null;

        /// <summary>
        /// Called when the video is paused while playing.
        /// </summary>
        public event Action<VideoPlayer> Paused     = null;

        /// <summary>
        /// Called when the video is resumed after being paused.
        /// </summary>
        public event Action<VideoPlayer> Resumed    = null;

        /// <summary>
        /// Called when the video is stopped or completed.
        /// </summary>
        public event Action<VideoPlayer> Stopped    = null;
        #endregion

        #region Enhanced Behaviour
        protected override void OnInit() {
            base.OnInit();

            // Prepare the video.
            if (prepareOnInit) {

                Time = playRange.x;
                Prepare();
            }
        }

        protected override void OnPlay() {
            base.OnPlay();

            // Play once loading is over.
            if (playAfterLoading) {

                playAfterLoading = false;
                PlayFromStart();
            }
        }

        void IStableUpdate.Update() {
            OnPlayerUpdate();
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            if (isActive) {
                Stop();
            }
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        protected override void OnValidate() {
            base.OnValidate();

            // Reference.
            if (!videoPlayer) {
                videoPlayer = GetComponent<VideoPlayer>();
                videoPlayer.playOnAwake = false;
            }

            // Range initialization.
            if (playRange == Vector2.zero) {
                playRange = TimeRange;
            }
        }
        #endif
        #endregion

        #region Player
        private VideoPlayer.FrameReadyEventHandler sendFrameReadyEventsCallback = null;
        private VideoPlayer.EventHandler onLoopPointReachedCallback = null;

        private GameState gameState = null;
        private bool isActive       = false;

        // -----------------------

        /// <summary>
        /// <inheritdoc cref="VideoPlayer.Play"/>
        /// (From <see cref="VideoPlayer.Play"/>)
        /// </summary>
        public override void Play() {
            // Do not execute if already playing.
            if (IsPlaying) {
                return;
            }

            if (!IsPrepared) {
                Prepare(Play);
                return;
            }

            // Enables the video, just in case.
            videoPlayer.enabled = true;

            // Ensure that the video time is correct.
            double _time = Time;
            if ((playRange.y - _time) < .1d) {
                _time = 0d;
            }

            Time = _time;
            videoPlayer.Play();

            // Active while started playing and not stopped yet.
            if (isActive) {
                Resumed?.Invoke(videoPlayer);
                return;
            }

            isActive = true;
            Started?.Invoke(videoPlayer);

            SetRenderCanvasAlpha(true);

            // Stop callback.
            onLoopPointReachedCallback ??= OnStop;

            videoPlayer.loopPointReached -= onLoopPointReachedCallback;
            videoPlayer.loopPointReached += onLoopPointReachedCallback;

            #if UNITY_EDITOR
            // Editor behaviour.
            if (!Application.isPlaying) {

                sendFrameReadyEventsCallback ??= OnFrameReady;
                videoPlayer.sendFrameReadyEvents = true;

                videoPlayer.frameReady -= sendFrameReadyEventsCallback;
                videoPlayer.frameReady += sendFrameReadyEventsCallback;
                return;
            }
            #endif

            // Game State.
            Type _gameStateType = boundGameState.Type;

            if ((_gameStateType != null) && !gameState.IsActive()) {
                gameState = GameState.CreateState(_gameStateType);
                
                // Bound this player.
                switch (gameState) {
                    case IBoundGameState<EnhancedVideoPlayer> _boundPlayer:
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

            UpdateManager.Instance.Register(this, UpdateRegistration.Stable);
        }

        /// <summary>
        /// <inheritdoc cref="VideoPlayer.Pause"/>
        /// (From <see cref="VideoPlayer.Pause"/>)
        /// </summary>
        public override void Pause() {
            if (IsPaused) {
                return;
            }

            videoPlayer.Pause();
            Paused?.Invoke(videoPlayer);
        }

        /// <summary>
        /// <inheritdoc cref="VideoPlayer.Stop"/>
        /// (From <see cref="VideoPlayer.Stop"/>)
        /// </summary>
        public override void Stop() {
            videoPlayer.Stop();

            // Stopping the video does not call the "loopPointReached" event,
            // so call its callback manually.
            OnStop(videoPlayer);
        }

        /// <summary>
        /// Skips the video by setting its current time to <see cref="SkipTime"/>.
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

        private void OnFrameReady(VideoPlayer _source, long _frameIdx) {
            OnPlayerUpdate();
        }

        private void OnPlayerUpdate() {

            if (Time >= playRange.y) {
                Stop();

                if (videoPlayer.isLooping) {
                    PlayFromStart();
                }
            }
        }

        // -----------------------

        private void OnStop(VideoPlayer _player) {

            // Not active if never being played or already stopped.
            if (!isActive || GameManager.IsQuittingApplication) {
                return;
            }

            isActive = false;
            Stopped?.Invoke(videoPlayer);

            _player.loopPointReached -= onLoopPointReachedCallback;

            SetRenderCanvasAlpha(false);

            // Clear the render texture to avoid displaying the last frame when playing a new video.
            if (videoPlayer.renderMode == VideoRenderMode.RenderTexture) {
                videoPlayer.targetTexture.Release();
            }

            #if UNITY_EDITOR
            // Editor behaviour.
            if (!Application.isPlaying) {

                videoPlayer.sendFrameReadyEvents = false;
                videoPlayer.frameReady -= sendFrameReadyEventsCallback;
                return;
            }
            #endif

            // Game State.
            if (gameState.IsActive()) {
                gameState.RemoveState();
                gameState = null;
            }

            UpdateManager.Instance.Unregister(this, UpdateRegistration.Stable);

            // Disables the video to make sure nothing is rendered anymore.
            if (disableOnStop) {
                videoPlayer.enabled = false;
            }
        }
        #endregion

        #region Utility
        private VideoPlayer.EventHandler onPrepareCompletedCallback = null;
        private Action onPreparedCallback = null;

        // -----------------------

        /// <summary>
        /// Plays this <see cref="VideoPlayer"/> from the start.
        /// </summary>
        public void PlayFromStart() {
            Time = playRange.x;
            Play();
        }

        /// <summary>
        /// <inheritdoc cref="VideoPlayer.Prepare"/>
        /// (From <see cref="VideoPlayer.Prepare"/>)
        /// </summary>
        /// <param name="_onPrepared">Called when the video has been fully prepared.</param>
        public void Prepare(Action _onPrepared = null) {

            if (IsPrepared) {
                _onPrepared?.Invoke();
                return;
            }

            if (_onPrepared != null) {

                onPreparedCallback = _onPrepared;
                onPrepareCompletedCallback ??= OnPrepared;

                videoPlayer.prepareCompleted -= onPrepareCompletedCallback;
                videoPlayer.prepareCompleted += onPrepareCompletedCallback;
            }

            videoPlayer.Prepare();

            // ----- Local Method ----- \\

            void OnPrepared(VideoPlayer _player){
                videoPlayer.prepareCompleted -= onPrepareCompletedCallback;
                onPreparedCallback.Invoke();
            }
        }

        // -----------------------

        /// <summary>
        /// Sets the alpha value of this object rendering <see cref="CanvasGroup"/>.
        /// </summary>
        /// <param name="_isVisible">Whether the rendering canvas should be visible or not.</param>
        public void SetRenderCanvasAlpha(bool _isVisible) {
            if (manageRenderingCanvas) {
                renderingCanvas.GetReference().SetVisibility(_isVisible);
            }
        }

        /// <summary>
        /// Sets the clip of the associated <see cref="VideoPlayer"/>.
        /// </summary>
        /// <param name="_clip">The new clip of the video player.</param>
        /// <param name="_play">Whether to start this clip from the start or not.</param>
        /// <param name="_prepare">Whether to prepare the video or not.</param>
        public void SetClip(VideoClip _clip, bool _play = true, bool _prepare = true) {
            videoPlayer.clip = _clip;

            if (_play) {
                PlayFromStart();
            } else if (_prepare) {
                Prepare();
            } 
        }

        /// <summary>
        /// Get the <see cref="VideoPlayer"/> associated with this object.
        /// </summary>
        public VideoPlayer GetVideoPlayer() {
            return videoPlayer;
        }
        #endregion
    }
}
