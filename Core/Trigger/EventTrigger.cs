// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="EventTrigger"/> activation mode.
    /// </summary>
    public enum EventTriggerActivation {
        [Tooltip("Disable this trigger")]
        None    = 0,

        [Separator(SeparatorPosition.Top)]

        [Tooltip("Play this event on trigger enter")]
        OnEnter = 1,

        [Tooltip("Play this event on trigger exit")]
        OnExit  = 2,

        [Tooltip("Play this event while on trigger")]
        WhileOnTrigger = 3,
    }

    /// <summary>
    /// Trigger used to play an event on actor interaction.
    /// </summary>
    public abstract class EventTrigger : EnhancedTrigger {
        #region Global Members
        [PropertyOrder(1)]

        [Tooltip("Determines this event trigger activation mode")]
        [SerializeField] protected EventTriggerActivation triggerActivation = EventTriggerActivation.None;
        #endregion

        #region Trigger
        /// <summary>
        /// Plays this trigger event.
        /// </summary>
        protected abstract void PlayTriggerEvent();

        /// <summary>
        /// Stops this trigger event.
        /// </summary>
        protected abstract void StopTriggerEvent();

        // -------------------------------------------
        // Trigger
        // -------------------------------------------

        protected override void OnEnterTrigger(ITriggerActor _actor, EnhancedBehaviour _behaviour) {
            base.OnEnterTrigger(_actor, _behaviour);

            // If there is more than one actor, event is probably already playing.
            if (TriggerActorCount != 1) {
                return;
            }

            switch (triggerActivation) {

                // Play.
                case EventTriggerActivation.OnEnter:
                case EventTriggerActivation.WhileOnTrigger:
                    PlayTriggerEvent();
                    break;

                case EventTriggerActivation.None:
                case EventTriggerActivation.OnExit:
                default:
                    break;
            }
        }

        protected override void OnExitTrigger(ITriggerActor _actor, EnhancedBehaviour _behaviour) {
            base.OnExitTrigger(_actor, _behaviour);

            // If there is still an actor, event should not be stopped.
            if (TriggerActorCount != 0) {
                return;
            }

            switch (triggerActivation) {

                // Play.
                case EventTriggerActivation.OnExit:
                    PlayTriggerEvent();
                    break;

                // Stop.
                case EventTriggerActivation.WhileOnTrigger:
                    StopTriggerEvent();
                    break;

                case EventTriggerActivation.None:
                case EventTriggerActivation.OnEnter:
                default:
                    break;
            }
        }
        #endregion
    }
}
