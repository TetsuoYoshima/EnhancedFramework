// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using EnhancedEditor.Editor;
using EnhancedFramework.Core;
using System;
using UnityEditor;
using UnityEngine;

namespace EnhancedFramework.Editor {
    /// <summary>
    /// <see cref="EnhancedObjectID"/>-related <see cref="EnhancedSettings"/> class.
    /// </summary>
    [Serializable]
    public sealed class ObjectIdEnhancedSettings : EnhancedSettings {
        #region Global Members
        public bool ConsoleLogs = false;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="ObjectIdEnhancedSettings"/>
        public ObjectIdEnhancedSettings(int _guid) : base(_guid) { }

        #if UNITY_EDITOR
        [InitializeOnLoad]
        private sealed class EditorInitialize {
            static EditorInitialize() {
                UpdateValue();
            }
        }
        #endif
        #endregion

        #region Settings
        private static readonly GUIContent consoleLogGUI = new GUIContent("Object ID Logs", "Logs a message to the console whenever an object id is changed");

        private static ObjectIdEnhancedSettings settings = null;
        private static readonly int settingsGUID = "EnhancedEditorObjectIdSettings".GetHashCode();

        // -----------------------

        /// <inheritdoc cref="ObjectIdEnhancedSettings"/>
        public static ObjectIdEnhancedSettings Settings {
            get {
                EnhancedEditorUserSettings _userSettings = EnhancedEditorUserSettings.Instance;

                if ((settings == null) && !_userSettings.GetSetting(settingsGUID, out settings, out _)) {
                    settings = new ObjectIdEnhancedSettings(settingsGUID);
                    _userSettings.AddSetting(settings);
                }

                return settings;
            }
        }

        // -------------------------------------------
        // Drawer
        // -------------------------------------------

        [EnhancedEditorUserSettings(Order = 26)]
        private static void DrawSettings() {
            var _settings = Settings;
            bool _toggle = EditorGUILayout.Toggle(consoleLogGUI, _settings.ConsoleLogs);

            if (_toggle != _settings.ConsoleLogs) {

                _settings.ConsoleLogs = _toggle;
                GUI.changed = true;

                UpdateValue();
            }
        }

        private static void UpdateValue() {
            EnhancedObjectID.ConsoleLogs = Settings.ConsoleLogs;
        }
        #endregion
    }
}
