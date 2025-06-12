// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedFramework.Core;
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.Video;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.PlayMaker {
    /// <summary>
    /// <see cref="FsmStateAction"/> used to send an event when a <see cref="EnhancedVideoPlayer"/> is being resumed.
    /// </summary>
    [Tooltip("Sends an Event when an Enhanced Video Player is being resumed")]
    [ActionCategory("Video")]
    public sealed class EnhancedVideoResumedEvent : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - Event
        // -------------------------------------------

        [Tooltip("The Video used by the event.")]
        [RequiredField, ObjectType(typeof(EnhancedVideoPlayer))]
        public FsmObject Video = null;

        [Tooltip("Event to send when the Video is being resumed.")]
        public FsmEvent ResumedEvent;
        #endregion

        #region Behaviour
        private Action<VideoPlayer> onResumed = null;

        // -----------------------

        public override void Reset() {
            base.Reset();

            ResumedEvent = null;
            Video        = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (Video.Value is EnhancedVideoPlayer _video) {
                onResumed ??= OnResumed;
                _video.Resumed += onResumed;
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (Video.Value is EnhancedVideoPlayer _video) {
                _video.Resumed -= onResumed;
            }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void OnResumed(VideoPlayer _video) {
            Fsm.Event(ResumedEvent);
        }
        #endregion
    }
}
