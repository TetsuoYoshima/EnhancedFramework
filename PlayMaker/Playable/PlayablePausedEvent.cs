// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.Playables;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.PlayMaker {
    /// <summary>
    /// <see cref="FsmStateAction"/> used to send an event when a <see cref="PlayableDirector"/> is being paused.
    /// </summary>
    [Tooltip("Sends an Event when a Playable is being paused")]
    [ActionCategory("Playable")]
    public sealed class PlayablePausedEvent : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - Event
        // -------------------------------------------

        [Tooltip("The Playable used by the event.")]
        [RequiredField, ObjectType(typeof(PlayableDirector))]
        public FsmObject Playable = null;

        [Tooltip("Event to send when the Playable is being paused.")]
        public FsmEvent PausedEvent;
        #endregion

        #region Behaviour
        private Action<PlayableDirector> onPaused = null;

        // -----------------------

        public override void Reset() {
            base.Reset();

            PausedEvent = null;
            Playable    = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (Playable.Value is PlayableDirector _playable) {
                onPaused ??= OnPaused;
                _playable.paused += onPaused;
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (Playable.Value is PlayableDirector _playable) {
                _playable.paused -= onPaused;
            }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void OnPaused(PlayableDirector _playable) {
            Fsm.Event(PausedEvent);
        }
        #endregion
    }
}
