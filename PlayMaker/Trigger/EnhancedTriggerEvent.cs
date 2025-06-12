// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using HutongGames.PlayMaker;
using UnityEngine;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.PlayMaker {
    /// <summary>
    /// <see cref="FsmStateAction"/> used to send an event when an object enters or exits a <see cref="LevelTrigger"/>.
    /// </summary>
    [Tooltip("Sends an event when an object enters / exits a Trigger.")]
    [ActionCategory(ActionCategory.Physics)]
    public sealed class EnhancedTriggerEvent : FsmStateAction, ITrigger {
        #region Global Members
        // -------------------------------------------
        // Trigger - Events - Object
        // -------------------------------------------

        [Tooltip("Object to detect trigger events on.")]
        [RequiredField, CheckForComponent(typeof(LevelTrigger))]
        public FsmOwnerDefault Trigger;

        [Tooltip("Event to send when an object enters the Trigger.")]
        public FsmEvent OnEnterEvent;

        [Tooltip("Event to send when an object exits the Trigger.")]
        public FsmEvent OnExitEvent;

        [Tooltip("The Object that entered / exited the Trigger.")]
        [ObjectType(typeof(EnhancedBehaviour))]
        [UIHint(UIHint.Variable)]
        public FsmObject StoreObject;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            OnEnterEvent = null;
            OnExitEvent  = null;
            StoreObject  = null;
            Trigger      = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (GetTrigger(out LevelTrigger _trigger)) {
                _trigger.RegisterCallback(this);
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (GetTrigger(out LevelTrigger _trigger)) {
                _trigger.UnregisterCallback(this);
            }
        }

        // -------------------------------------------
        // Trigger
        // -------------------------------------------

        public void OnEnterTrigger(ITriggerActor _actor) {

            StoreObject.Value = _actor.Behaviour;
            Fsm.Event(OnEnterEvent);
        }

        public void OnExitTrigger(ITriggerActor _actor) {

            StoreObject.Value = _actor.Behaviour;
            Fsm.Event(OnExitEvent);
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        private bool GetTrigger(out LevelTrigger _trigger) {
            GameObject _gameObject = Fsm.GetOwnerDefaultTarget(Trigger);

            if (_gameObject.IsValid()) {
                return _gameObject.TryGetComponent(out _trigger);
            }

            _trigger = null;
            return false;
        }
        #endregion
    }
}
