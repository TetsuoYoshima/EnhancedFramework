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
    /// <see cref="FsmStateAction"/> used to send an event when a <see cref="PlayableDirector"/> is being stopped.
    /// </summary>
    [Tooltip("Sends an Event when a Playable is being stopped")]
    [ActionCategory("Playable")]
    public sealed class PlayableStoppedEvent : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - Event
        // -------------------------------------------

        [Tooltip("The Playable used by the event.")]
        [RequiredField, ObjectType(typeof(PlayableDirector))]
        public FsmObject Playable = null;

        [Tooltip("Event to send when the Playable is being stopped.")]
        public FsmEvent StoppedEvent;
        #endregion

        #region Behaviour
        private Action<PlayableDirector> onStopped = null;

        // -----------------------

        public override void Reset() {
            base.Reset();

            StoppedEvent = null;
            Playable     = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (Playable.Value is PlayableDirector _playable) {
                onStopped ??= OnStopped;
                _playable.stopped += onStopped;
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (Playable.Value is PlayableDirector _playable) {
                _playable.stopped -= onStopped;
            }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void OnStopped(PlayableDirector _playable) {
            Fsm.Event(StoppedEvent);
        }
        #endregion
    }
}
