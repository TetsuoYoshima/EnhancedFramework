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
    /// <see cref="FsmStateAction"/> used to pause a <see cref="EnhancedVideoPlayer"/>.
    /// </summary>
    [Tooltip("Pauses an Enhanced Video Player")]
    [ActionCategory("Video")]
    public sealed class EnhancedVideoPause : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable
        // -------------------------------------------

        [Tooltip("The Video to pause.")]
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

            Pause();
            Finish();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void Pause() {
            if (Video.Value is EnhancedVideoPlayer _video) {
                _video.Pause();
            }
        }
        #endregion
    }
}
