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
    /// <see cref="FsmStateAction"/> used to send an event when a <see cref="EnhancedVideoPlayer"/> starts being played.
    /// </summary>
    [Tooltip("Sends an Event when an Enhanced Video Player starts being played")]
    [ActionCategory("Video")]
    public sealed class EnhancedVideoStartedEvent : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - Event
        // -------------------------------------------

        [Tooltip("The Video used by the event.")]
        [RequiredField, ObjectType(typeof(EnhancedVideoPlayer))]
        public FsmObject Playable = null;

        [Tooltip("Event to send when the Video starts being played.")]
        public FsmEvent StartedEvent;
        #endregion

        #region Behaviour
        private Action<VideoPlayer> onStarted = null;

        // -----------------------

        public override void Reset() {
            base.Reset();

            StartedEvent = null;
            Playable     = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (Playable.Value is EnhancedVideoPlayer _video) {
                onStarted ??= OnStarted;
                _video.Started += onStarted;
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (Playable.Value is EnhancedVideoPlayer _video) {
                _video.Started -= onStarted;
            }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void OnStarted(VideoPlayer _video) {
            Fsm.Event(StartedEvent);
        }
        #endregion
    }
}
