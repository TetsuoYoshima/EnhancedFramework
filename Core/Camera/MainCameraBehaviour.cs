// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Attach this <see cref="Component"/> to your main <see cref="Camera"/> to cache and quickly access it.
    /// </summary>
    [ScriptGizmos(false, true)]
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Camera/Main Camera Behaviour"), DisallowMultipleComponent]
    public sealed class MainCameraBehaviour : EnhancedBehaviour {
        #region Global Members
        [SerializeField, HideInInspector] private new Camera camera = null;

        // -----------------------

        /// <summary>
        /// Whether the <see cref="MainCamera"/> reference is currently set or not.
        /// </summary>
        public static bool HasMainCamera { get; private set; } = false;

        /// <summary>
        /// Main <see cref="Camera"/> instance.
        /// </summary>
        public static Camera MainCamera { get; private set; } = null;
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Set.
            if (!HasMainCamera) {
                MainCamera    = camera;
                HasMainCamera = true;
            }
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Unset.
            if (HasMainCamera && (MainCamera == camera)) {
                MainCamera    = null;
                HasMainCamera = false;
            }
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Button
        // -------------------------------------------

        protected override void OnValidate() {
            base.OnValidate();

            // Reference.
            if (!camera) {
                camera = GetComponent<Camera>();
            }
        }
        #endif
        #endregion

        #region Utility
        /// <summary>
        /// Get the current main <see cref="Camera"/>.
        /// </summary>
        /// <param name="_mainCamera">Main <see cref="Camera"/> instance.</param>
        /// <returns>True if a <see cref="Camera"/> instance could be found, false otherwise.</returns>
        public static bool GetMainCamera(out Camera _mainCamera) {
            _mainCamera = MainCamera;
            return HasMainCamera;
        }
        #endregion
    }
}
