// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace EnhancedFramework.Inputs {
    /// <summary>
    /// Multiple <see cref="SingleInputActionEnhancedAsset"/>-related asset,
    /// <br/> being performed when all specified inputs are being performed together.
    /// </summary>
    [CreateAssetMenu(fileName = FilePrefix + "MultiInputAction", menuName = MenuPath + "Multi Action", order = MenuOrder)]
    public sealed class MultipleInputActionEnhancedAsset : InputActionEnhancedAsset {
        public const string FilePrefix  = "IPX_";

        #region Global Members
        [Space(10f)]

        [SerializeField] private SingleInputActionEnhancedAsset[] inputs = new SingleInputActionEnhancedAsset[0];

        // -----------------------

        public override bool IsEnabled {
            get {

                ref SingleInputActionEnhancedAsset[] _inputs = ref inputs;
                for (int i = _inputs.Length; i-- > 0;) {
                    if (!_inputs[i].IsEnabled)
                        return false;
                }

                return true;
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
        #endregion

        #region Initialization
        protected override void OnInit(InputDatabase _database) {
            // Event setup.
            ref SingleInputActionEnhancedAsset[] _inputs = ref inputs;
            for (int i = _inputs.Length; i-- > 0;) {
                SingleInputActionEnhancedAsset _input = _inputs[i];

                _input.OnStarted   += (InputActionEnhancedAsset _) => CallOnStarted();
                _input.OnCanceled  += (InputActionEnhancedAsset _) => CallOnCanceled();
                _input.OnPerformed += OnInputPerformed;
            }

            // ----- Local Methods ----- \\

            void OnInputPerformed(InputActionEnhancedAsset _) {
                if (Performed()) {
                    CallOnPerformed();
                }
            }
        }
        #endregion

        #region Input
        public override bool Performed() {
            bool _isPerformed = false;

            ref SingleInputActionEnhancedAsset[] _inputs = ref inputs;
            for (int i = _inputs.Length; i-- > 0;) {
                SingleInputActionEnhancedAsset _input = _inputs[i];

                if (_input.Performed()) {
                    _isPerformed = true;
                    continue;
                }

                if (!_input.Holding()) {
                    return false;
                }
            }

            return _isPerformed;
        }

        public override bool Pressed() {
            bool _isPressed = false;

            ref SingleInputActionEnhancedAsset[] _inputs = ref inputs;
            for (int i = _inputs.Length; i-- > 0;) {
                SingleInputActionEnhancedAsset _input = _inputs[i];

                if (_input.Pressed()) {
                    _isPressed = true;
                    continue;
                }

                if (!_input.Performed() && !_input.Holding()) {
                    return false;
                }
            }

            return _isPressed;
        }

        public override bool Holding() {
            ref SingleInputActionEnhancedAsset[] _inputs = ref inputs;
            for (int i = _inputs.Length; i-- > 0;) {
                if (!_inputs[i].Holding())
                    return false;
            }

            return true;
        }

        // -----------------------

        public override float GetAxis() {
            this.LogError($"{GetType().Name} value cannot be read as an axis");
            return 0f;
        }

        public override Vector2 GetVector2Axis() {
            this.LogError($"{GetType().Name} value cannot be read as a Vector2");
            return Vector2.zero;
        }

        public override Vector3 GetVector3Axis() {
            this.LogError($"{GetType().Name} value cannot be read as a Vector3");
            return Vector3.zero;
        }
        #endregion

        #region Utility
        public override void Enable() {
            ref SingleInputActionEnhancedAsset[] _inputs = ref inputs;
            for (int i = _inputs.Length; i-- > 0;) {
                _inputs[i].Enable();
            }
        }

        public override void Disable() {
            ref SingleInputActionEnhancedAsset[] _inputs = ref inputs;
            for (int i = _inputs.Length; i-- > 0;) {
                _inputs[i].Disable();
            }
        }
        #endregion
    }
}
