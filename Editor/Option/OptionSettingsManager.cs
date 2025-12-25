// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor.Editor;
using EnhancedFramework.Core.Option;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace EnhancedFramework.Editor {
    /// <summary>
    /// Editor class manipulating and updating the data contained in the <see cref="OptionSettings"/>.
    /// </summary>
    [InitializeOnLoad]
    public sealed class OptionSettingsManager : IPreprocessBuildWithReport {
        #region Global Members
        private static readonly AutoManagedResource<OptionSettings> resource = new AutoManagedResource<OptionSettings>("OptionSettings", false);

        int IOrderedCallback.callbackOrder => 999;

        /// <summary>
        /// Settings containing informations about all game options.
        /// </summary>
        public static OptionSettings Settings => resource.GetResource();

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        static OptionSettingsManager() {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        #endregion

        #region Management
        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport _report) {

            // Called just before a build is started.
            UpdateDatabase();
            AssetDatabase.SaveAssets();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _state) {
            if (_state == PlayModeStateChange.EnteredPlayMode) {
                UpdateDatabase();
            }
        }

        private static void UpdateDatabase() {

            // Register all scriptable game options from the project.
            Settings.scriptableOptions = EnhancedEditorUtility.LoadAssets<ScriptableGameOption>();
            EditorUtility.SetDirty(Settings);
        }
        #endregion
    }
}
