// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Utility <see cref="EnhancedBehaviour"/> used to initialize this object activation state.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Utility/Game Object Activator")]
    public sealed class GameObjectActivator : EnhancedBehaviour {
        #region Mode
        /// <summary>
        /// Enum used to define when the object state is being updated.
        /// </summary>
        public enum Mode {
            [Tooltip("Don't toggle this object state")]
            None    = 0,

            [Separator(SeparatorPosition.Top)]

            [Tooltip("Toggles this object state as soon as it is enabled")]
            Enable  = 1,

            [Tooltip("Toggles this object state on Init")]
            Init    = 2,
        }
        #endregion

        public override UpdateRegistration UpdateRegistration {
            get {
                UpdateRegistration _value = base.UpdateRegistration;
                if (mode == Mode.Init) {
                    _value |= UpdateRegistration.Init;
                }

                return _value;
            }
        }

        #region Global Members
        [Section("Game Object Activator")]

        [Tooltip("Whether to activate or deactivate this object")]
        [SerializeField] private bool active = true;

        [Tooltip("If true, checks if a specific Scripting Symbol is defined, and toggles this object accordingly:\n" +
                 "Set to the specified active state if the symbol defined, set it to the inverse state if not")]
        [SerializeField] private bool useScriptingSymbol = false;

        [Space(10f)]

        [Tooltip("Scripting Symbol to check definition")]
        [SerializeField, Enhanced, ShowIf(nameof(useScriptingSymbol))] private string scriptingSymbol = string.Empty;

        [Tooltip("Defines when this object state is updated")]
        [SerializeField] private Mode mode = Mode.Init;

        [Tooltip("The object to toggle activation")]
        [SerializeField, Enhanced, Required, DisplayName("Object")] private GameObject activeObject = null;
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            switch (mode) {

                // Update.
                case Mode.Enable:
                    UpdateState();
                    break;

                case Mode.Init:
                case Mode.None:
                default:
                    break;
            }
        }

        protected override void OnInit() {
            base.OnInit();

            switch (mode) {

                // Update.
                case Mode.Init:
                    UpdateState();
                    break;

                case Mode.Enable:
                case Mode.None:
                default:
                    break;
            }
        }

        // -----------------------

        /// <summary>
        /// Updates this object activation state.
        /// </summary>
        private void UpdateState() {

            bool _active = active;

            // Inverse if required symbol is not defined.
            if (useScriptingSymbol && !GameManager.Instance.IsScriptingSymbolDefined(scriptingSymbol)) {
                _active = !_active;
            }

            activeObject.SetActive(_active);
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        protected override void OnValidate() {
            base.OnValidate();

            if (!activeObject) {
                activeObject = gameObject;
            }
        }
        #endif
        #endregion
    }
}
