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
    /// <see cref="FsmStateAction"/> used to stop a collection of <see cref="IEnhancedFeedback"/>.
    /// </summary>
    [Tooltip("Stops a collection of Enhanced Feedback.")]
    [ActionCategory("Feedback")]
    public sealed class FeedbackStop : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Feedbacks
        // -------------------------------------------

        [Tooltip("Asset Feedbacks to stop.")]
        [ArrayEditor(typeof(EnhancedAssetFeedback), "Asset Feedback")]
        public FsmArray AssetFeedbacks = null;

        [Tooltip("Scene Feedbacks to stop.")]
        [ArrayEditor(typeof(EnhancedSceneFeedback), "Scene Feedback")]
        public FsmArray SceneFeedbacks = null;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            AssetFeedbacks = null;
            SceneFeedbacks = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            Stop();
            Finish();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void Stop() {
            // Assets.
            ref FsmArray _span = ref AssetFeedbacks;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                if (_span.Get(i) is EnhancedAssetFeedback _feedback) {
                    _feedback.StopSafe();
                }
            }

            // Scene.
            _span  = ref SceneFeedbacks;
            _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                if (_span.Get(i) is EnhancedSceneFeedback _feedback) {
                    _feedback.StopSafe();
                }
            }
        }
        #endregion
    }
}
