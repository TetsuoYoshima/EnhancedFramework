// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Utility <see cref="EnhancedBehaviour"/> used to manipulate the <see cref="Transform"/> of an object,
    /// using both an editor and a runtime preconfigured position.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Utility/Transform Controller")]
    public sealed class TransformEditorController : EnhancedBehaviour {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init;

        #region Global Members
        [Section("Transform Controller")]

        [Tooltip("Local position of this object in editor")]
        [SerializeField] private Vector3 editorPosition = new Vector3();

        [Tooltip("Local position of this object at runtime")]
        [SerializeField] private Vector3 runtimePosition = new Vector3();
        #endregion

        #region Enhanced Behaviour
        protected override void OnInit() {
            base.OnInit();
            SetRuntimePosition();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Sets this transform at its editor position.
        /// </summary>
        [Button(SuperColor.HarvestGold)]
        public void SetEditorPosition() {
            transform.localPosition = editorPosition;
        }

        /// <summary>
        /// Sets this transform at its runtime position.
        /// </summary>
        [Button(SuperColor.Green)]
        public void SetRuntimePosition() {
            transform.localPosition = runtimePosition;
        }
        #endregion
    }
}
