// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedFramework.Core;
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.Playables;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.PlayMaker {
    /// <summary>
    /// <see cref="FsmStateAction"/> used to play a <see cref="EnhancedPlayablePlayer"/>.
    /// </summary>
    [Tooltip("Plays an Enhanced Playable Player")]
    [ActionCategory("Playable")]
    public sealed class EnhancedPlayablePlay : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - Paused - Stopped
        // -------------------------------------------

        [Tooltip("The Playable to play.")]
        [RequiredField, ObjectType(typeof(EnhancedPlayablePlayer))]
        public FsmObject Playable = null;

        [Tooltip("Event to send when the Playable is paused.")]
        public FsmEvent PausedEvent;

        [Tooltip("Event to send when the Playable is stopped.")]
        public FsmEvent StoppedEvent;
        #endregion

        #region Behaviour
        private Action<PlayableDirector> onStopped = null;
        private Action<PlayableDirector> onPaused  = null;

        // -----------------------

        public override void Reset() {
            base.Reset();

            StoppedEvent = null;
            PausedEvent  = null;
            Playable     = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (Playable.Value is EnhancedPlayablePlayer _playable) {
                var _director = _playable.GetPlayableDirector();

                if (onStopped == null) {
                    onStopped = OnStopped;
                    onPaused  = OnPaused;
                }

                _director.stopped += onStopped;
                _director.paused  += onPaused;

                _playable.Play();
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (Playable.Value is EnhancedPlayablePlayer _playable) {
                var _director = _playable.GetPlayableDirector();

                _director.stopped -= onStopped;
                _director.paused  -= onPaused;
            }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void OnPaused(PlayableDirector _playable) {
            Fsm.Event(PausedEvent);
        }

        private void OnStopped(PlayableDirector _playable) {
            Fsm.Event(StoppedEvent);
        }
        #endregion
    }
}
