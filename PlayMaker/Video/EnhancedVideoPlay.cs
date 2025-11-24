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
    /// <see cref="FsmStateAction"/> used to play a <see cref="EnhancedVideoPlayer"/>.
    /// </summary>
    [Tooltip("Plays an Enhanced Video Player")]
    [ActionCategory("Video")]
    public sealed class EnhancedVideoPlay : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - Paused - Resumed - Stopped
        // -------------------------------------------

        [Tooltip("The Playable to play.")]
        [RequiredField, ObjectType(typeof(EnhancedVideoPlayer))]
        public FsmObject Video = null;

        [Tooltip("Event to send when the Playable is paused.")]
        public FsmEvent PausedEvent;

        [Tooltip("Event to send when the Playable is resumed.")]
        public FsmEvent ResumedEvent;

        [Tooltip("Event to send when the Playable has stopped.")]
        public FsmEvent StoppedEvent;
        #endregion

        #region Behaviour
        private Action<VideoPlayer> onStopped = null;
        private Action<VideoPlayer> onResumed = null;
        private Action<VideoPlayer> onPaused  = null;

        // -----------------------

        public override void Reset() {
            base.Reset();

            StoppedEvent = null;
            ResumedEvent = null;
            PausedEvent  = null;
            Video        = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (Video.Value is EnhancedVideoPlayer _video) {

                if (onStopped == null) {
                    onStopped = OnStopped;
                    onResumed = OnResumed;
                    onPaused  = OnPaused;
                }

                _video.Stopped += onStopped;
                _video.Resumed += onResumed;
                _video.Paused  += onPaused;

                _video.Play();
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (Video.Value is EnhancedVideoPlayer _video) {
                _video.Stopped -= onStopped;
                _video.Resumed -= onResumed;
                _video.Paused  -= onPaused;
            }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void OnPaused(VideoPlayer _video) {
            Fsm.Event(PausedEvent);
        }

        private void OnResumed(VideoPlayer _video) {
            Fsm.Event(ResumedEvent);
        }

        private void OnStopped(VideoPlayer _video) {
            Fsm.Event(StoppedEvent);
        }
        #endregion
    }
}
