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
    /// <see cref="FsmStateAction"/> used to set a <see cref="FlagBehaviour"/> value.
    /// </summary>
    [Tooltip("Set the value of a Flag")]
    [ActionCategory("Flag")]
    public sealed class SetFlag : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - Value
        // -------------------------------------------

        [Tooltip("Flag value to set.")]
        [RequiredField, ObjectType(typeof(FlagBehaviour))]
        public FsmObject FlagVariable = null;

        [Tooltip("Value to set the Flag to.")]
        public FsmBool FlagValue = null;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            FlagVariable = null;
            FlagValue    = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            SetFlagValue();
            Finish();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void SetFlagValue() {
            if (FlagVariable.Value is FlagBehaviour _behaviour) {
                _behaviour.SetValue(FlagValue.Value);
            }
        }
        #endregion
    }
}
