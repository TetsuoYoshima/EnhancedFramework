// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
// 
// Notes:
//
// ================================================================================== //

using EnhancedFramework.Core;
using HutongGames.PlayMaker;
using UnityEngine;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.PlayMaker {
    /// <summary>
    /// <see cref="FsmStateAction"/> used to get if all values in a <see cref="FlagGroupBehaviour"/> are valid.
    /// </summary>
    [Tooltip("Get if all values of a Flag Group are valid")]
    [ActionCategory("Flag")]
    public sealed class GetFlagGroupIsValid : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - True - False - Store
        // -------------------------------------------

        [Tooltip("Flag Group to check validity.")]
        [RequiredField, ObjectType(typeof(FlagGroupBehaviour))]
        public FsmObject FlagGroup = null;

        [Tooltip("Event to send if the Flag Group is valid.")]
        public FsmEvent TrueEvent;

        [Tooltip("Event to send if the Flag Group is not valid.")]
        public FsmEvent FalseEvent;

        [Tooltip("Store the validity of the Flag Group in a bool Variable.")]
        [UIHint(UIHint.Variable)]
        public FsmBool StoreValue = null;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            FalseEvent = null;
            StoreValue = null;
            TrueEvent  = null;
            FlagGroup  = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            GetValidity();
            Finish();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void GetValidity() {
            bool _isValid = false;

            if (FlagGroup.Value is FlagGroupBehaviour _behaviour) {
                _isValid = _behaviour.GetValue();
            }

            StoreValue.Value = _isValid;
            Fsm.Event(_isValid ? TrueEvent : FalseEvent);
        }
        #endregion
    }
}
