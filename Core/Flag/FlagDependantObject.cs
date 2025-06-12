// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Utility component used to activate/deactivate an object on flag values.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Flag/Flag Dependant Object")]
    public sealed class FlagDependantObject : EnhancedBehaviour, IFlagCallback {
        #region State
        /// <summary>
        /// Activation mode of this object.
        /// </summary>
        public enum ActivationMode {
            [Tooltip("Don't do anything")]
            None        = 0,

            [Separator(SeparatorPosition.Top)]

            [Tooltip("Activates this object if required flags are valid")]
            Activate    = 1,

            [Tooltip("Deactivates this object if required flags are valid")]
            Deactivate  = 2,
        }

        /// <summary>
        /// Determines when this object state is updated.
        /// </summary>
        public enum UpdateMode {
            [Tooltip("State is never updated")]
            None        = 0,

            [Separator(SeparatorPosition.Top)]

            [Tooltip("Update this object state on Init")]
            OnInit      = 1,

            [Tooltip("Update this object state on Play (after loading)")]
            OnPlay      = 2,

            [Tooltip("Update this object state whenever a flag value is changed")]
            OnUpdate    = 3,
        }
        #endregion

        public override UpdateRegistration UpdateRegistration {
            get {
                UpdateRegistration _registration = base.UpdateRegistration;

                switch (updateMode) {

                    case UpdateMode.OnInit:
                        _registration |= UpdateRegistration.Init;
                        break;

                    case UpdateMode.OnPlay:
                        _registration |= UpdateRegistration.Play;
                        break;

                    case UpdateMode.None:
                    case UpdateMode.OnUpdate:
                    default:
                        break;
                }

                return _registration;
            }
        }

        #region Global Members
        [Section("Flag Dependant Object")]

        [Tooltip("Objects to manage activation on flag condition")]
        [SerializeField, Enhanced, Required] private GameObject[] managedObjects = new GameObject[0];

        [Tooltip("Objects to inverse activation on flag condition")]
        [SerializeField, Enhanced, Required] private GameObject[] inverseObjects = new GameObject[0];

        [Space(10f)]

        [Tooltip("Required flags condition")]
        [SerializeField] private FlagValueGroup requiredFlags = new FlagValueGroup();

        [Tooltip("Determines this object activation mode when this flag condtion is valid")]
        [SerializeField] private ActivationMode onFlagsState = ActivationMode.None;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Determines when this object state is updated")]
        [SerializeField] private UpdateMode updateMode = UpdateMode.OnInit;
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Registration.
            if (updateMode == UpdateMode.OnUpdate) {
                FlagUtility.RegisterFlagCallback(this);
                UpdateState();
            }
        }

        protected override void OnInit() {
            base.OnInit();
            UpdateState();
        }

        protected override void OnPlay() {
            base.OnPlay();
            UpdateState();
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Unregistration.
            if (updateMode == UpdateMode.OnUpdate) {
                FlagUtility.UnregisterFlagCallback(this);
            }
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        protected override void OnValidate() {
            base.OnValidate();

            // Reference.
            if (managedObjects.Length == 0) {
                ArrayUtility.Add(ref managedObjects, gameObject);
            }
        }
        #endif
        #endregion

        #region Behaviour
        /// <summary>
        /// Updates this object activation based on flags value.
        /// </summary>
        public void UpdateState() {

            bool _isValid = requiredFlags.Valid;
            bool _active;

            switch (onFlagsState) {

                case ActivationMode.Activate:
                    _active = _isValid;
                    break;

                case ActivationMode.Deactivate:
                    _active = !_isValid;
                    break;

                case ActivationMode.None:
                default:
                    return;
            }

            // Update state.
            ref GameObject[] _span = ref managedObjects;
            for (int i = _span.Length; i-- > 0;) {
                _span[i].SetActive(_active);
            }

            _span = ref inverseObjects;
            for (int i = _span.Length; i-- > 0;) {
                _span[i].SetActive(!_active);
            }            
        }

        // -------------------------------------------
        // Callback
        // -------------------------------------------

        void IFlagCallback.OnFlagChanged(Flag _flag, bool _value) {
            UpdateState();
        }
        #endregion
    }
}
