// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using UnityEngine;

namespace EnhancedFramework.PlayMaker {
    /// <summary>
    /// <see cref="EnhancedBehaviour"/> used to send a global event
    /// to a <see cref="PlayMakerFSM"/> instance on trigger callback.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "PlayMaker/Trigger Event Sender")]
    public sealed class PlayMakerTriggerEventSender : EnhancedTrigger {
        #region Global Members
        [Section("Trigger Event Sender")]

        [SerializeField, Enhanced, Required] private PlayMakerFSM fsm = null;

        [Space(10f)]

        [Tooltip("Event to call on trigger enter")]
        [SerializeField] private string enterEventName = string.Empty;

        [Tooltip("Event to call on trigger exit")]
        [SerializeField] private string exitEventName = string.Empty;
        #endregion

        #region Enhanced Behaviour
        protected override void OnEnterTrigger(ITriggerActor _actor, EnhancedBehaviour _behaviour) {
            base.OnEnterTrigger(_actor, _behaviour);

            if (!string.IsNullOrEmpty(enterEventName)) {
                fsm.SendEvent(enterEventName);
            }
        }

        protected override void OnExitTrigger(ITriggerActor _actor, EnhancedBehaviour _behaviour) {
            base.OnExitTrigger(_actor, _behaviour);

            if (!string.IsNullOrEmpty(exitEventName)) {
                fsm.SendEvent(exitEventName);
            }
        }
        #endregion
    }
}
