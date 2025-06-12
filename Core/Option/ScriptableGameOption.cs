// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[assembly: InternalsVisibleTo("EnhancedFramework.UI")]
namespace EnhancedFramework.Core.Option {
    /// <summary>
    /// <see cref="BaseGameOption"/>-related <see cref="ScriptableObject"/> wrapper.
    /// </summary>
    [CreateAssetMenu(fileName = "OPT_GameOption", menuName = FrameworkUtility.MenuPath + "Game Option", order = FrameworkUtility.MenuOrder)]
    public sealed class ScriptableGameOption : EnhancedScriptableObject {
        #region Global Members
        [Section("Game Option")]

        [Tooltip("This option type")]
        [SerializeField] private SerializedType<BaseGameOption> optionType = new SerializedType<BaseGameOption>(SerializedTypeConstraint.None, typeof(DefaultGameOption));

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Default option values")]
        [SerializeReference, Enhanced, Block] internal BaseGameOption defaultOption = new DefaultGameOption();

        // -----------------------

        public int AvailableOptionCount {
            get { return defaultOption.AvailableOptionCount; }
        }
        #endregion

        #region Scriptable Object
        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        private void Awake() {
            RefreshValues();
        }

        protected override void OnValidate() {
            base.OnValidate();
            RefreshValues();
        }

        // -----------------------

        private void RefreshValues() {
            if (Application.isPlaying) {
                return;
            }

            // Option type update.
            if (optionType.Type != defaultOption.GetType()) {

                BaseGameOption _option = Activator.CreateInstance(optionType) as BaseGameOption;
                defaultOption = EnhancedUtility.CopyObjectContent(defaultOption, _option) as BaseGameOption;

                EditorUtility.SetDirty(this);
            }
        }
        #endif
        #endregion

        #region Option
        [NonSerialized] private BaseGameOption runtimeOption = null;

        /// <summary>
        /// This object wrapped <see cref="BaseGameOption"/>.
        /// </summary>
        public BaseGameOption Option {
            get { return runtimeOption; }
        }

        // -----------------------

        /// <summary>
        /// Initializes this option.
        /// </summary>
        /// <param name="_settings"><see cref="OptionSettings"/> instance.</param>
        internal void Initialize(OptionSettings _settings) {

            // Initializes this option value.
            runtimeOption = _settings.GetOption(defaultOption.GUID, defaultOption.Name, this);
            runtimeOption.Initialize(defaultOption);
        }

        /// <summary>
        /// Creates this option default value.
        /// </summary>
        /// <returns>This option default value.</returns>
        internal BaseGameOption CreateOption() {
            BaseGameOption _option = Activator.CreateInstance(optionType)    as BaseGameOption;
            return EnhancedUtility.CopyObjectContent(defaultOption, _option) as BaseGameOption;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Regenerates this option guid.
        /// </summary>
        [Button(SuperColor.Crimson, IsDrawnOnTop = false)]
        public void GenerateGUID() {

            #if UNITY_EDITOR
            Undo.RecordObject(this, "Generate GUID");
            #endif

            defaultOption.guid = EnhancedUtility.GenerateGUID();

            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
        #endregion
    }
}
