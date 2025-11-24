// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedFramework.Core;
using HutongGames.PlayMaker;
using UnityEngine;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.PlayMaker {
    /// <summary>
    /// <see cref="FsmStateAction"/> used to play a collection of <see cref="IEnhancedFeedback"/>.
    /// </summary>
    [Tooltip("Plays a collection of Enhanced Feedback.")]
    [ActionCategory("Feedback")]
    public sealed class FeedbackPlay : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Feedbacks - Options - Transform - Stop
        // -------------------------------------------

        [Tooltip("Asset Feedbacks to play.")]
        [ArrayEditor(typeof(EnhancedAssetFeedback), "Asset Feedback")]
        [RequiredField]
        public FsmArray AssetFeedbacks = null;

        [Tooltip("Scene Feedbacks to play.")]
        [ArrayEditor(typeof(EnhancedSceneFeedback), "Scene Feedback")]
        [RequiredField]
        public FsmArray SceneFeedbacks = null;

        [Tooltip("Options used to play these feedbacks.")]
        [RequiredField, ObjectType(typeof(FeedbackPlayOptions))]
        public FsmEnum Options = null;

        [Tooltip("Transform used to play these feedbacks (position and rotation).")]
        [HideIf(nameof(HideTransform))]
        public FsmGameObject Transform;

        [Tooltip("If true, stops these feedbacks when exiting this state.")]
        [RequiredField]
        public FsmBool StopOnExit;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            AssetFeedbacks = null;
            SceneFeedbacks = null;
            StopOnExit     = false;
            Transform      = null;
            Options        = FeedbackPlayOptions.None;
        }

        public override void OnEnter() {
            base.OnEnter();

            Play();
            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            Stop();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void Play() {
            GameObject _object   = Transform.Value;
            Transform _transform = (_object != null) ? _object.transform : null;

            FeedbackPlayOptions _options = (FeedbackPlayOptions)Options.Value;

            // Assets.
            ref FsmArray _span = ref AssetFeedbacks;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                if (_span.Get(i) is EnhancedAssetFeedback _feedback) {
                    _feedback.PlaySafe(_transform, _options);
                }
            }

            // Scene.
            _span = ref SceneFeedbacks;
            _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                if (_span.Get(i) is EnhancedSceneFeedback _feedback) {
                    _feedback.PlaySafe(_transform, _options);
                }
            }
        }

        private void Stop() {
            if (!StopOnExit.Value) {
                return;
            }

            // Assets.
            ref FsmArray _span = ref AssetFeedbacks;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                if (_span.Get(i) is EnhancedAssetFeedback _feedback) {
                    _feedback.StopSafe();
                }
            }

            // Scene.
            _span = ref SceneFeedbacks;
            _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                if (_span.Get(i) is EnhancedSceneFeedback _feedback) {
                    _feedback.StopSafe();
                }
            }
        }
        #endregion

        #region Utility
        public bool HideTransform() {
            return (FeedbackPlayOptions)Options.Value == FeedbackPlayOptions.None;
        }
        #endregion
    }
}
