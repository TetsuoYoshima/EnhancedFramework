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
    /// Various options used to play an <see cref="IEnhancedFeedback"/>.
    /// </summary>
    public enum FeedbackPlayOptions {
        [Tooltip("Plays the feedback with no option")]
        None = 0,

        [Separator(SeparatorPosition.Top)]

        [Tooltip("Plays the feedback at a specific position")]
        PlayAtPosition = 1,

        [Tooltip("Plays the feedback and parent it to a specific Transform")]
        FollowTransform = 2,
    }

    /// <summary>
    /// Base interface for enhanced easy-to-implement feedbacks.
    /// </summary>
    public interface IEnhancedFeedback {
        #region Content
        /// <summary>
        /// Plays this feedback using a specific transform.
        /// </summary>
        /// <param name="_transform"><see cref="Transform"/> used to play this feedback.</param>
        /// <param name="_options">Options used to play this feedback.</param>
        void Play(Transform _transform, FeedbackPlayOptions _options);

        /// <summary>
        /// Plays this feedback at a given position.
        /// </summary>
        void Play(Vector3 _position);

        /// <summary>
        /// Plays this feedback.
        /// </summary>
        void Play();

        /// <summary>
        /// Stops playing this feedback.
        /// </summary>
        void Stop();
        #endregion
    }

    /// <summary>
    /// Holder for multiple <see cref="EnhancedAssetFeedback"/> and <see cref="EnhancedSceneFeedback"/>.
    /// </summary>
    [Serializable]
    public sealed class EnhancedFeedbacks : IEnhancedFeedback, ISerializationCallbackReceiver {
        #region Global Members
        [SerializeField] private EnhancedAssetFeedback[] assetFeedbacks = new EnhancedAssetFeedback[0];
        [SerializeField] private EnhancedSceneFeedback[] sceneFeedbacks = new EnhancedSceneFeedback[0];
        #endregion

        #region Serialization
        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            //RemoveNullReferences();
        }

        // -----------------------

        private void RemoveNullReferences() {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                // Remove null references to avoid any error.
                ArrayUtility.RemoveNulls(ref assetFeedbacks);
                ArrayUtility.RemoveNulls(ref sceneFeedbacks);
            }
            #endif
        }
        #endregion

        #region Enhanced Feedback
        /// <inheritdoc cref="IEnhancedFeedback.Play(Transform, FeedbackPlayOptions)"/>
        public void Play(Transform _transform, FeedbackPlayOptions _options = FeedbackPlayOptions.PlayAtPosition) {

            ref EnhancedAssetFeedback[] _assetSpan = ref assetFeedbacks;
            int _count = _assetSpan.Length;

            for (int i = 0; i < _count; i++) {
                _assetSpan[i].Play(_transform, _options);
            }

            ref EnhancedSceneFeedback[] _sceneSpan = ref sceneFeedbacks;
            _count = _sceneSpan.Length;

            for (int i = 0; i < _count; i++) {
                _sceneSpan[i].Play(_transform, _options);
            }
        }

        /// <inheritdoc cref="IEnhancedFeedback.Play(Vector3)"/>
        public void Play(Vector3 _position) {

            ref EnhancedAssetFeedback[] _assetSpan = ref assetFeedbacks;
            int _count = _assetSpan.Length;

            for (int i = 0; i < _count; i++) {
                _assetSpan[i].Play(_position);
            }

            ref EnhancedSceneFeedback[] _sceneSpan = ref sceneFeedbacks;
            _count = _sceneSpan.Length;

            for (int i = 0; i < _count; i++) {
                _sceneSpan[i].Play(_position);
            }
        }

        /// <inheritdoc cref="IEnhancedFeedback.Play"/>
        public void Play() {

            ref EnhancedAssetFeedback[] _assetSpan = ref assetFeedbacks;
            int _count = _assetSpan.Length;

            for (int i = 0; i < _count; i++) {
                _assetSpan[i].Play();
            }

            ref EnhancedSceneFeedback[] _sceneSpan = ref sceneFeedbacks;
            _count = _sceneSpan.Length;

            for (int i = 0; i < _count; i++) {
                _sceneSpan[i].Play();
            }
        }

        /// <inheritdoc cref="IEnhancedFeedback.Stop"/>
        public void Stop() {

            ref EnhancedAssetFeedback[] _assetSpan = ref assetFeedbacks;
            int _count = _assetSpan.Length;

            for (int i = 0; i < _count; i++) {
                _assetSpan[i].Stop();
            }

            ref EnhancedSceneFeedback[] _sceneSpan = ref sceneFeedbacks;
            _count = _sceneSpan.Length;

            for (int i = 0; i < _count; i++) {
                _sceneSpan[i].Stop();
            }
        }
        #endregion
    }

    // ===== Core ===== \\

    /// <summary>
    /// <see cref="EnhancedAssetFeedback"/> & <see cref="EnhancedSceneFeedback"/> related play operation data wrapper.
    /// </summary>
    internal struct EnhancedFeedbackPlayData {
        #region Global Members
        public FeedbackPlayOptions Options;
        public Transform Transform;
        public Vector3 Position;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="EnhancedFeedbackPlayData"/>
        public EnhancedFeedbackPlayData(FeedbackPlayOptions _options, Transform _transform, Vector3 _position) {
            Options   = _options;
            Transform = _transform;
            Position  = _position;
        }
        #endregion
    }

    /// <summary>
    /// Base class for any <see cref="EnhancedScriptableObject"/> enhanced feedback.
    /// </summary>
    public abstract class EnhancedAssetFeedback : EnhancedScriptableObject, IEnhancedFeedback {
        #region Global Members
        public const string FilePrefix  = "FBK_";
        public const string MenuPath    = FrameworkUtility.MenuPath + "Feedback/";
        public const int    MenuOrder   = FrameworkUtility.MenuOrder;

        // -----------------------

        [Tooltip("Delay before playing this feedback")]
        [Enhanced, Range(0f, 10f)] public float Delay = 0f;

        [Tooltip("If true, delay will not be affected by time scale")]
		[SerializeField, Enhanced, DisplayName("Real Time")] private bool useRealTime = false;
        #endregion

        #region Enhanced Feedback
        private readonly List<EnhancedFeedbackPlayData> delayedPlayData = new List<EnhancedFeedbackPlayData>();
        private DelayHandler delayHandler = default;
        private Action       onPlayDelay  = null;

        // -------------------------------------------
        // Core
        // -------------------------------------------

        /// <inheritdoc cref="IEnhancedFeedback.Play(Transform, FeedbackPlayOptions)"/>
        [Button(ActivationMode.Play, SuperColor.Green)]
        public void Play(Transform _transform, FeedbackPlayOptions _options = FeedbackPlayOptions.PlayAtPosition) {
            Vector3 _position = (_options == FeedbackPlayOptions.PlayAtPosition) ? _transform.position : Vector3.zero;
            DoPlay(_options, _transform, _position);
        }

        /// <inheritdoc cref="IEnhancedFeedback.Play(Vector3)"/>
        public void Play(Vector3 _position) {
            DoPlay(FeedbackPlayOptions.PlayAtPosition, null, _position);
        }

        /// <inheritdoc cref="IEnhancedFeedback.Play"/>
        public void Play() {
            DoPlay(FeedbackPlayOptions.None, null, Vector3.zero);
        }

        /// <inheritdoc cref="IEnhancedFeedback.Stop"/>
        [Button(ActivationMode.Play, SuperColor.Crimson)]
        public virtual void Stop() {
            if (delayHandler.Cancel()) {
                delayedPlayData.RemoveLast();
            }

            OnStop();
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        protected virtual void DoPlay(FeedbackPlayOptions _options, Transform _transform, Vector3 _position) {

            float _delay = Delay;
            if (_delay != 0f) {

                delayedPlayData.Add(new EnhancedFeedbackPlayData(_options, _transform, _position));

                onPlayDelay ??= DoPlayDelay;
                delayHandler  = Delayer.Call(_delay, onPlayDelay, useRealTime);

            } else {
                OnPlay(_options, _transform, _position);
            }

            // ----- Local Method ----- \\

            void DoPlayDelay() {
                if (!delayedPlayData.SafeFirst(out EnhancedFeedbackPlayData _data)) {
                    return;
                }

                delayedPlayData.RemoveFirst();
                OnPlay(_data.Options, _data.Transform, _data.Position);
            }
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Called when starting to play this feedback.
        /// <br/> Implements this feedback behaviour here.
        /// </summary>
        /// <inheritdoc cref="IEnhancedFeedback.Play(FeedbackPlayOptions, Transform)"/>
        protected abstract void OnPlay(FeedbackPlayOptions _options, Transform _transform, Vector3 _position);

        /// <summary>
        /// Called when stopping to play this feedback.
        /// <br/> Stops this feedback behaviour here.
        /// </summary>
        /// <inheritdoc cref="IEnhancedFeedback.Stop"/>
        protected abstract void OnStop();
        #endregion
    }

    /// <summary>
    /// Base class for any <see cref="EnhancedBehaviour"/> enhanced feedback (using scene reference(s)).
    /// </summary>
    public abstract class EnhancedSceneFeedback : EnhancedBehaviour, IEnhancedFeedback {
        #region Global Members
        [Section("Enhanced Scene Feedback")]

        [Tooltip("Delay before playing this feedback")]
        [Enhanced, Range(0f, 5f)] public float Delay = 0f;

        [Tooltip("If true, delay will not be affected by time scale")]
        [SerializeField, Enhanced, DisplayName("Real Time")] private bool useRealTime = false;
        #endregion

        #region Enhanced Feedback
        private readonly List<EnhancedFeedbackPlayData> delayedPlayData = new List<EnhancedFeedbackPlayData>();
        private DelayHandler delayHandler = default;
        private Action       onPlayDelay  = null;

        // -------------------------------------------
        // Core
        // -------------------------------------------

        /// <inheritdoc cref="IEnhancedFeedback.Play(Transform, FeedbackPlayOptions)"/>
        [Button(ActivationMode.Play, SuperColor.Green)]
        public void Play(Transform _transform, FeedbackPlayOptions _options = FeedbackPlayOptions.PlayAtPosition) {
            Vector3 _position = (_options == FeedbackPlayOptions.PlayAtPosition) ? _transform.position : Vector3.zero;
            DoPlay(_options, _transform, _position);
        }

        /// <inheritdoc cref="IEnhancedFeedback.Play(Vector3)"/>
        public void Play(Vector3 _position) {
            DoPlay(FeedbackPlayOptions.PlayAtPosition, null, _position);
        }

        /// <inheritdoc cref="IEnhancedFeedback.Play"/>
        public void Play() {
            DoPlay(FeedbackPlayOptions.None, null, Vector3.zero);
        }

        /// <inheritdoc cref="IEnhancedFeedback.Stop"/>
        [Button(ActivationMode.Play, SuperColor.Crimson)]
        public virtual void Stop() {
            if (delayHandler.Cancel()) {
                delayedPlayData.RemoveLast();
            }

            OnStopFeedback();
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        protected virtual void DoPlay(FeedbackPlayOptions _options, Transform _transform, Vector3 _position) {

            float _delay = Delay;
            if (_delay != 0f) {

                delayedPlayData.Add(new EnhancedFeedbackPlayData(_options, _transform, _position));

                onPlayDelay ??= DoPlayDelay;
                delayHandler  = Delayer.Call(_delay, onPlayDelay, useRealTime);

            } else {
                OnPlayFeedback(_options, _transform, _position);
            }

            // ----- Local Method ----- \\

            void DoPlayDelay() {
                if (!delayedPlayData.SafeFirst(out EnhancedFeedbackPlayData _data)) {
                    return;
                }

                delayedPlayData.RemoveFirst();
                OnPlayFeedback(_data.Options, _data.Transform, _data.Position);
            }
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Called when starting to play this feedback.
        /// <br/> Implements this feedback behaviour here.
        /// </summary>
        /// <inheritdoc cref="IEnhancedFeedback.Play(Transform, FeedbackPlayOptions)"/>
        protected abstract void OnPlayFeedback(FeedbackPlayOptions _options, Transform _transform, Vector3 _position);

        /// <summary>
        /// Called when stopping to play this feedback.
        /// <br/> Stops this feedback behaviour here.
        /// </summary>
        /// <inheritdoc cref="IEnhancedFeedback.Stop"/>
        protected abstract void OnStopFeedback();
        #endregion
    }

    // ===== Extensions ===== \\

    /// <summary>
    /// <see cref="EnhancedAssetFeedback"/>-related extension methods.
    /// </summary>
    public static class EnhancedFeedbackExtensions {
        #region Asset
        /// <summary>
        /// Safely plays this feedback using a specific transform, if not null.
        /// </summary>
        /// <param name="_feedback">The feedback to play.</param>
        /// <inheritdoc cref="IEnhancedFeedback.Play(Transform, FeedbackPlayOptions)"/>
        public static void PlaySafe(this EnhancedAssetFeedback _feedback, Transform _transform, FeedbackPlayOptions _options = FeedbackPlayOptions.PlayAtPosition) {
            if (_feedback.IsValid()) {
                _feedback.Play(_transform, _options);
            }
        }

        /// <summary>
        /// Safely plays this feedback at a given position, if not null.
        /// </summary>
        /// <param name="_feedback">The feedback to play.</param>
        /// <inheritdoc cref="IEnhancedFeedback.Play(Transform, Vector3)"/>
        public static void PlaySafe(this EnhancedAssetFeedback _feedback, Vector3 _position) {
            if (_feedback.IsValid()) {
                _feedback.Play(_position);
            }
        }

        /// <summary>
        /// Safely plays this feedback, if not null.
        /// </summary>
        /// <param name="_feedback">The feedback to play.</param>
        public static void PlaySafe(this EnhancedAssetFeedback _feedback) {
            if (_feedback.IsValid()) {
                _feedback.Play();
            }
        }

        /// <summary>
        /// Safely stops this feedback, if not null.
        /// </summary>
        /// <param name="_feedback">The feedback to stop.</param>
        public static void StopSafe(this EnhancedAssetFeedback _feedback) {
            if (_feedback.IsValid()) {
                _feedback.Stop();
            }
        }
        #endregion

        #region Scene
        /// <inheritdoc cref="PlaySafe(EnhancedAssetFeedback, Transform, FeedbackPlayOptions)"/>
        public static void PlaySafe(this EnhancedSceneFeedback _feedback, Transform _transform, FeedbackPlayOptions _options = FeedbackPlayOptions.PlayAtPosition) {
            if (_feedback.IsValid()) {
                _feedback.Play(_transform, _options);
            }
        }

        /// <inheritdoc cref="PlaySafe(EnhancedAssetFeedback, Vector3)"/>
        public static void PlaySafe(this EnhancedSceneFeedback _feedback, Vector3 _position) {
            if (_feedback.IsValid()) {
                _feedback.Play(_position);
            }
        }

        /// <inheritdoc cref="PlaySafe(EnhancedAssetFeedback)"/>
        public static void PlaySafe(this EnhancedSceneFeedback _feedback) {
            if (_feedback.IsValid()) {
                _feedback.Play();
            }
        }

        /// <inheritdoc cref="StopSafe(EnhancedAssetFeedback)"/>
        public static void StopSafe(this EnhancedSceneFeedback _feedback) {
            if (_feedback.IsValid()) {
                _feedback.Stop();
            }
        }
        #endregion
    }
}
