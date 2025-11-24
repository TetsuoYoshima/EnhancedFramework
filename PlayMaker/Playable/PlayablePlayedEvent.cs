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
    /// <see cref="FsmStateAction"/> used to send an event when a <see cref="PlayableDirector"/> is being played.
    /// </summary>
    [Tooltip("Sends an Event when a Playable is being played (started or resumed)")]
    [ActionCategory("Playable")]
    public sealed class PlayablePlayedEvent : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - Event
        // -------------------------------------------

        [Tooltip("The Playable used by the event.")]
        [RequiredField, ObjectType(typeof(PlayableDirector))]
        public FsmObject Playable = null;

        [Tooltip("Event to send when the Playable is being played (started or resumed).")]
        public FsmEvent PlayedEvent;
        #endregion

        #region Behaviour
        private Action<PlayableDirector> onPlayed = null;

        // -----------------------

        public override void Reset() {
            base.Reset();

            PlayedEvent = null;
            Playable    = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (Playable.Value is PlayableDirector _playable) {
                onPlayed ??= OnPlayed;
                _playable.played += onPlayed;
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (Playable.Value is PlayableDirector _playable) {
                _playable.played -= onPlayed;
            }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void OnPlayed(PlayableDirector _playable) {
            Fsm.Event(PlayedEvent);
        }
        #endregion
    }
}
