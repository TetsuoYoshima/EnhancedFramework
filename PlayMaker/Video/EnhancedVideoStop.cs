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
    /// <see cref="FsmStateAction"/> used to stop a <see cref="EnhancedVideoPlayer"/>.
    /// </summary>
    [Tooltip("Stops an Enhanced Video Player")]
    [ActionCategory("Video")]
    public sealed class EnhancedVideoStop : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable
        // -------------------------------------------

        [Tooltip("The Video to stop.")]
        [RequiredField, ObjectType(typeof(EnhancedVideoPlayer))]
        public FsmObject Video = null;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Video = null;
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
            if (Video.Value is EnhancedVideoPlayer _video) {
                _video.Stop();
            }
        }
        #endregion
    }
}
