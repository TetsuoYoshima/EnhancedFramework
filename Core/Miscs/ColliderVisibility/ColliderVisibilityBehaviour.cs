// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Utility component used to manage a collider visibility.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Utility/Collider Visibility Behaviour"), SelectionBase, DisallowMultipleComponent]
    public sealed class ColliderVisibilityBehaviour : EnhancedBehaviour {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init;

        #region Enhanced Behaviour
        protected override void OnInit() {
            base.OnInit();
            Disable();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Enables this collider visibility and display its renderer.
        /// </summary>
        public void Enable() {
            Toggle(true);
        }

        /// <summary>
        /// Disables this collider visibility hide display its renderer.
        /// </summary>
        public void Disable() {
            Toggle(false);
        }

        // -----------------------

        /// <summary>
        /// Set this collider visibility.
        /// </summary>
        /// <param name="_enabled">True to display it, false to hide it.</param>
        public void Toggle(bool _enabled) {

            if (TryGetComponent(out MeshRenderer _renderer)) {
                _renderer.enabled = _enabled;
            }

            enabled = _enabled;
        }
        #endregion
    }
}
