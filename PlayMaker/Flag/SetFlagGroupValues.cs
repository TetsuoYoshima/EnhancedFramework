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
    /// <see cref="FsmStateAction"/> used to set a <see cref="FlagGroupBehaviour"/> values.
    /// </summary>
    [Tooltip("Set the values of a Flag Group")]
    [ActionCategory("Flag")]
    public sealed class SetFlagGroupValues : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable
        // -------------------------------------------

        [Tooltip("Flag Group to set values.")]
        [RequiredField, ObjectType(typeof(FlagGroupBehaviour))]
        public FsmObject FlagGroup = null;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            FlagGroup = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            SetGroupValues();
            Finish();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void SetGroupValues() {
            if (FlagGroup.Value is FlagGroupBehaviour _behaviour) {
                _behaviour.SetFlags();
            }
        }
        #endregion
    }
}
