// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Utility component used to control a <see cref="FlagValueGroup"/> value.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Flag/Flag Controller")]
    public sealed class FlagController : EventTrigger {
        #region Mode
        /// <summary>
        /// Determines when this object flags are set.
        /// </summary>
        public enum SetFlagMode {
            [Tooltip("Don't do anything")]
            None        = 0,

            [Separator(SeparatorPosition.Top)]

            [Tooltip("Set these flags on initialization")]
            OnInit      = 1,

            [Tooltip("Set these flags after loading")]
            OnPlay      = 2,

            [Tooltip("Set these flags when the object is disabled")]
            OnDisabled  = 3,
        }
        #endregion

        public override UpdateRegistration UpdateRegistration {
            get {
                UpdateRegistration _registration = base.UpdateRegistration;

                switch (setFlagMode) {

                    case SetFlagMode.OnInit:
                        _registration |= UpdateRegistration.Init;
                        break;

                    case SetFlagMode.OnPlay:
                        _registration |= UpdateRegistration.Play;
                        break;

                    case SetFlagMode.None:
                    case SetFlagMode.OnDisabled:
                    default:
                        break;
                }

                return _registration;
            }
        }

        #region Global Members
        [Section("Flag Controller"), PropertyOrder(0)]

        [Tooltip("Flags to set or unset value within this controller")]
        [SerializeField] private FlagValueGroup flags = new FlagValueGroup();

        [Space(10f)]

        [Tooltip("Mode used to automatically set these flags value")]
        [SerializeField] private SetFlagMode setFlagMode = SetFlagMode.None;

        // -----------------------

        /// <summary>
        /// The total amount of flag in this object group.
        /// </summary>
        public int Count {
            get { return flags.Count; }
        }

        /// <summary>
        /// <see cref="FlagValueGroup"/> wrapped in this controller.
        /// </summary>
        public FlagValueGroup Flags {
            get { return flags; }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnInit() {
            base.OnInit();
            SetFlags(true);
        }

        protected override void OnPlay() {
            base.OnPlay();
            SetFlags(true);
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Update.
            if (setFlagMode == SetFlagMode.OnDisabled) {
                SetFlags(true);
            }
        }
        #endregion

        #region Management
        /// <inheritdoc cref="FlagGroup.AddFlag(Flag)"/>
        public void AddFlag(Flag _flag) {
            flags.AddFlag(_flag);
        }

        /// <inheritdoc cref="FlagGroup.RemoveFlag(Flag)"/>
        public void RemoveFlag(Flag _flag) {
            flags.RemoveFlag(_flag);
        }

        /// <inheritdoc cref="FlagGroup.RemoveFlagAt(int)"/>
        public void RemoveFlagAt(int _index) {
            flags.RemoveFlagAt(_index);
        }

        /// <inheritdoc cref="FlagGroup.ContainFlag(Flag, out int)"/>
        public bool ContainFlag(Flag _flag, out int _index) {
            return flags.ContainFlag(_flag, out _index);
        }
        #endregion

        #region Trigger
        protected override void PlayTriggerEvent() {
            SetFlags(true);
        }

        protected override void StopTriggerEvent() {
            SetFlags(false);
        }
        #endregion

        #region Utility
        /// <inheritdoc cref="FlagValueGroup.Valid"/>
        [Button(ActivationMode.Always, SuperColor.Green, IsDrawnOnTop = false)]
        public bool GetValue() {
            bool _value = flags.Valid;

            #if DEVELOPMENT
            this.Log($"{name.Bold()} flags valid: {_value.ToString().Bold()}");
            #endif

            return _value;
        }

        /// <inheritdoc cref="FlagValueGroup.SetValues"/>
        public void SetFlags() {
            Flags.SetValues();
        }

        /// <inheritdoc cref="FlagValueGroup.SetValues(bool)"/>
        [Button(ActivationMode.Play, SuperColor.Orange, IsDrawnOnTop = false)]
        public void SetFlags(bool _valid = true) {
            Flags.SetValues(_valid);
        }
        #endregion
    }
}
