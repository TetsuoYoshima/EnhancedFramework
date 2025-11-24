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
    /// <see cref="FsmStateAction"/> used to send an event when a <see cref="EnhancedVideoPlayer"/> is being stopped.
    /// </summary>
    [Tooltip("Sends an Event when an Enhanced Video Player is being stopped")]
    [ActionCategory("Video")]
    public sealed class EnhancedVideoStoppedEvent : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - Event
        // -------------------------------------------

        [Tooltip("The Video used by the event.")]
        [RequiredField, ObjectType(typeof(EnhancedVideoPlayer))]
        public FsmObject Video = null;

        [Tooltip("Event to send when the Video is being stopped.")]
        public FsmEvent StoppedEvent;
        #endregion

        #region Behaviour
        private Action<VideoPlayer> onStopped = null;

        // -----------------------

        public override void Reset() {
            base.Reset();

            StoppedEvent = null;
            Video        = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (Video.Value is EnhancedVideoPlayer _video) {
                onStopped ??= OnStopped;
                _video.Stopped += onStopped;
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (Video.Value is EnhancedVideoPlayer _video) {
                _video.Stopped -= onStopped;
            }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void OnStopped(VideoPlayer _video) {
            Fsm.Event(StoppedEvent);
        }
        #endregion
    }
}
