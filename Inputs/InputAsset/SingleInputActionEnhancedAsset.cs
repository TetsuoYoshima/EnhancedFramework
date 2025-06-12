// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if ENABLE_INPUT_SYSTEM
#define NEW_INPUT_SYSTEM
#endif

using EnhancedEditor;
using System.Runtime.CompilerServices;
using UnityEngine;

#if NEW_INPUT_SYSTEM
using UnityEngine.InputSystem;
#else
using InputSystem = UnityEngine.Input;
#endif

[assembly: InternalsVisibleTo("EnhancedFramework.Editor")]
namespace EnhancedFramework.Inputs {
    /// <summary>
    /// <see cref="ScriptableObject"/> asset used to reference an input action.
    /// </summary>
    [CreateAssetMenu(fileName = FilePrefix + "SingleInputAction", menuName = MenuPath + "Single Action", order = MenuOrder)]
    public sealed class SingleInputActionEnhancedAsset : InputActionEnhancedAsset {
        public const string FilePrefix  = "IPS_";

        #region Global Members
        [Tooltip("If true, this input won't be related to any input map or database")]
        [SerializeField] private bool isOrphan = false;

        [Space(10f)]

        [SerializeField, Enhanced, ReadOnly, ShowIf(nameof(isOrphan), ConditionType.False)] private InputMapEnhancedAsset map = null;

        #if NEW_INPUT_SYSTEM
        [SerializeField, Enhanced, ReadOnly(nameof(isOrphan))] internal InputAction input = null;
        #else
        [SerializeField] private string input = "Input";
        #endif

        // -----------------------

        public override bool IsEnabled {
            get {
                #if NEW_INPUT_SYSTEM
                return input.enabled;
                #else
                return true;
                #endif
            }
            set {
                if (value == IsEnabled) {
                    return;
                }

                if (value) {
                    Enable();
                } else {
                    Disable();
                }
            }
        }

        public override bool IsOrphan {
            get { return isOrphan; }
        }

        /// <summary>
        /// The <see cref="InputMapEnhancedAsset"/> associated with this input.
        /// </summary>
        public InputMapEnhancedAsset Map {
            get { return map; }
        }
        #endregion

        #region Initialization
        #if NEW_INPUT_SYSTEM
        /// <summary>
        /// Editor generator called method.
        /// </summary>
        internal void Initialize(InputAction _input) {
            input    = _input;
            IsPaused = false;
        }
        #endif

        protected override void OnInit(InputDatabase _database) {
            #if NEW_INPUT_SYSTEM
            // Retrieve the associated serialized input from the database.
            var _input = _database.ActionDatabase.FindAction(input.id);
            if (_input != null) {
                input = _input;
            }

            input.started   += (InputAction.CallbackContext _context) => CallOnStarted();
            input.canceled  += (InputAction.CallbackContext _context) => CallOnCanceled();
            input.performed += (InputAction.CallbackContext _context) => CallOnPerformed();
            #endif
        }
        #endregion

        #region Input
        public override bool Performed() {

            if (IsPaused) {
                return false;
            }

            #if NEW_INPUT_SYSTEM
            return input.WasPerformedThisFrame();
            #else
            return InputSystem.GetButtonDown(input);
            #endif
        }

        public override bool Pressed() {

            if (IsPaused) {
                return false;
            }

            #if NEW_INPUT_SYSTEM
            return input.WasPressedThisFrame();
            #else
            return InputSystem.GetButtonDown(input);
            #endif
        }

        public override bool Holding() {

            if (IsPaused) {
                return false;
            }

            #if NEW_INPUT_SYSTEM
            InputActionPhase _phase = input.phase;

            switch (_phase) {

                case InputActionPhase.Performed:
                    return true;

                case InputActionPhase.Waiting:
                    return input.ReadValue<float>() != 0f;

                case InputActionPhase.Started:
                case InputActionPhase.Disabled:
                case InputActionPhase.Canceled:
                default:
                    return false;
            }
            #else
            return InputSystem.GetButton(input);
            #endif
        }

        // -----------------------

        public override float GetAxis() {

            if (IsPaused) {
                return 0f;
            }

            #if NEW_INPUT_SYSTEM
            return input.ReadValue<float>();
            #else
            return InputSystem.GetAxis(input);
            #endif
        }

        public override Vector2 GetVector2Axis() {

            if (IsPaused) {
                return Vector2.zero;
            }

            #if NEW_INPUT_SYSTEM
            return input.ReadValue<Vector2>();
            #else
            this.LogError("Cannot read the value of a native input system axis as a Vector2");
            return Vector2.zero;
            #endif
        }

        public override Vector3 GetVector3Axis() {

            if (IsPaused) {
                return Vector3.zero;
            }

            #if NEW_INPUT_SYSTEM
            Vector2 _value = input.ReadValue<Vector2>();
            return new Vector3(_value.x, 0f, _value.y);
            #else
            this.LogError("Cannot read the value of a native input system axis as a Vector3");
            return Vector3.zero;
            #endif
        }
        #endregion

        #region Utility
        public override void Enable() {
            #if NEW_INPUT_SYSTEM
            input.Enable();
            #endif
        }

        public override void Disable() {
            #if NEW_INPUT_SYSTEM
            input.Disable();
            #endif
        }

        // -----------------------

        /// <summary>
        /// Editor generator called method.
        /// </summary>
        internal bool SetMap(InputMapEnhancedAsset _map) {
            if (_map.Contains(this)) {
                map = _map;
                return true;
            }

            return false;
        }
        #endregion
    }
}
