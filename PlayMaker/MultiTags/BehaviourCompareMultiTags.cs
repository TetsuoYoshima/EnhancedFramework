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
    /// <see cref="FsmStateAction"/> used to compare the tags of two <see cref="MultiTagsBehaviour"/>.
    /// </summary>
    [Tooltip("Compare the tags of two Multi Tags Behaviours.")]
    [ActionCategory(ActionCategory.Logic)]
    public sealed class BehaviourCompareMultiTags : FsmStateAction {
        #region Global Members
        // --------------------------------------------------------
        // Variable - Compare - True - False - Store - Every Frame
        // --------------------------------------------------------

        [Tooltip("The Multi Tags to compare.")]
        [RequiredField, ObjectType(typeof(MultiTagsBehaviour))]
        public FsmObject Variable = null;

        [Tooltip("Tags to check for.")]
        [RequiredField, ObjectType(typeof(MultiTagsBehaviour))]
        public FsmObject Value = null;

        [Tooltip("Event to send if the Variable has all Tags.")]
        public FsmEvent TrueEvent;

        [Tooltip("Event to send if the Variable does not have all Tags.")]
        public FsmEvent FalseEvent;

        [Tooltip("Store the result in a Bool variable.")]
        [UIHint(UIHint.Variable)]
        public FsmBool StoreResult;

        [Tooltip("Repeat every frame.")]
        public bool EveryFrame;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            StoreResult = null;
            EveryFrame  = false;
            FalseEvent  = null;
            TrueEvent   = null;
            Variable    = null;
            Value       = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            CompareTags();

            if (!EveryFrame) {
                Finish();
            }
        }

        public override void OnUpdate() {
            base.OnUpdate();

            CompareTags();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void CompareTags() {
            bool _hasTags = false;

            if ((Variable.Value is MultiTagsBehaviour _variable) && (Value.Value is MultiTagsBehaviour _value)) {
                _hasTags = _variable.Tags.ContainsAll(_value.Tags);
            }

            StoreResult.Value = _hasTags;
            Fsm.Event(_hasTags ? TrueEvent : FalseEvent);
        }
        #endregion
    }
}
