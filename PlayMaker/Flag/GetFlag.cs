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
    /// <see cref="FsmStateAction"/> used to get a <see cref="FlagBehaviour"/> value.
    /// </summary>
    [Tooltip("Get the value of a Flag")]
    [ActionCategory("Flag")]
    public sealed class GetFlag : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - True - False - Store
        // -------------------------------------------

        [Tooltip("Flag value to get.")]
        [RequiredField, ObjectType(typeof(FlagBehaviour))]
        public FsmObject FlagVariable = null;

        [Tooltip("Event to send if the Flag equals True.")]
        public FsmEvent TrueEvent;

        [Tooltip("Event to send if the Flag equals False.")]
        public FsmEvent FalseEvent;

        [Tooltip("Store the Flag value in a bool Variable.")]
        [UIHint(UIHint.Variable)]
        public FsmBool StoreValue = null;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            FlagVariable = null;
            FalseEvent   = null;
            StoreValue   = null;
            TrueEvent    = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            GetFlagValue();
            Finish();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void GetFlagValue() {
            bool _flag = false;

            if (FlagVariable.Value is FlagBehaviour _behaviour) {
                _flag = _behaviour.GetValue();
            }

            StoreValue.Value = _flag;
            Fsm.Event(_flag ? TrueEvent : FalseEvent);
        }
        #endregion
    }
}
