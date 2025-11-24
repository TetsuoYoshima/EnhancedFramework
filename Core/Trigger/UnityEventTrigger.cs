// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;
using UnityEngine.Events;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Plays a <see cref="UnityEvent"/> on trigger enter and / or exit.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Trigger/Unity Event Trigger"), DisallowMultipleComponent]
    public sealed class UnityEventTrigger : EnhancedTrigger {
        #region Global Members
        [Section("Unity Event Trigger"), PropertyOrder(0)]

        [Tooltip("Event to invoke on trigger enter")]
        [SerializeField, Enhanced, Required] private UnityEvent onEnterEvent = new UnityEvent();

        [Tooltip("Event to invoke on trigger exit")]
        [SerializeField, Enhanced, Required] private UnityEvent onExitEvent = new UnityEvent();
        #endregion

        #region Interact
        protected override void OnEnterTrigger(ITriggerActor _actor, EnhancedBehaviour _behaviour) {
            base.OnEnterTrigger(_actor, _behaviour);

            onEnterEvent.Invoke();
        }

        protected override void OnExitTrigger(ITriggerActor _actor, EnhancedBehaviour _behaviour) {
            base.OnExitTrigger(_actor, _behaviour);

            onExitEvent.Invoke();
        }
        #endregion
    }
}
