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
    /// <see cref="FsmStateAction"/> used to set the tags of a <see cref="UnityEngine.GameObject"/>, using the Mutli-Tags system.
    /// </summary>
    [Tooltip("Set the tags of a Game Object from a a Multi Tags Behaviour Variable.")]
    [ActionCategory(ActionCategory.GameObject)]
    public sealed class SetMultiTags : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - Value
        // -------------------------------------------

        [Tooltip("The Game Object to set the tags.")]
        [RequiredField]
        public FsmOwnerDefault GameObject = null;

        [Tooltip("Multi Tags to assign to the Game Object.")]
        [RequiredField, ObjectType(typeof(MultiTagsBehaviour))]
        public FsmObject MultiTags = null;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            GameObject = null;
            MultiTags  = null;            
        }

        public override void OnEnter() {
            base.OnEnter();

            SetTags();
            Finish();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void SetTags() {
            GameObject _gameObject = Fsm.GetOwnerDefaultTarget(GameObject);
            
            if (_gameObject.IsValid() && (MultiTags.Value is MultiTagsBehaviour _behaviour)) {
                _gameObject.SetTags(_behaviour.Tags);
            }
        }
        #endregion
    }
}
