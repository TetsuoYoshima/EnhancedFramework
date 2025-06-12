// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using HutongGames.PlayMaker;
using UnityEngine;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.PlayMaker {
    /// <summary>
    /// <see cref="FsmStateAction"/> used to get a <see cref="EnhancedEditor.SuperColor"/> value and store it in a variable.
    /// </summary>
    [Tooltip("Get the value of a SuperColor and store it in a Color Variable.")]
    [ActionCategory(ActionCategory.Color)]
    public sealed class GetSuperColor : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - Value - Every Frame
        // -------------------------------------------

        [Tooltip("The Color Variable to set.")]
        [RequiredField, UIHint(UIHint.Variable)]
        public FsmColor ColorVariable = null;

        [Tooltip("The SuperColor to set the variable to.")]
        [ObjectType(typeof(SuperColor))]
        public FsmEnum SuperColor = null;

        [Tooltip("Repeat every frame.")]
        public bool EveryFrame = false;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            ColorVariable = null;
            SuperColor    = null;
            EveryFrame    = false;
        }

        public override void OnEnter() {
            base.OnEnter();

            DoSetColorValue();

            if (!EveryFrame) {
                Finish();
            }
        }

        public override void OnUpdate() {
            base.OnUpdate();

            DoSetColorValue();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void DoSetColorValue() {
            ColorVariable.Value = ((SuperColor)SuperColor.Value).Get();
        }
        #endregion
    }
}
