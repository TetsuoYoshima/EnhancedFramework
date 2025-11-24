// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using ArrayUtility = EnhancedEditor.ArrayUtility;
using Object       = UnityEngine.Object;
#endif

namespace EnhancedFramework.Core.Settings {
    #if UNITY_EDITOR
    /// <summary>
    /// Implements the interface on another class to avoid Unity creating a new instance of the console window.
    /// </summary>
    [InitializeOnLoad]
    internal sealed class GameSettingsProcessor : IPreprocessBuildWithReport {
        #region Global Members
        int IOrderedCallback.callbackOrder {
            get { return 99; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        static GameSettingsProcessor() {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        #endregion

        #region Callbacks
        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport _report) {
            BuildTargetGroup _target = _report.summary.platformGroup;
            RegisterScriptingSymbols(_target);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _playMode) {

            switch (_playMode) {

                // Ignore.
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                case PlayModeStateChange.ExitingPlayMode:
                default:
                    return;

                case PlayModeStateChange.ExitingEditMode:
                    break;
            }

            BuildTargetGroup _target = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            RegisterScriptingSymbols(_target);
        }

        /// <summary>
        /// Registers all active defined scripting symbols for a group in the <see cref="GameSettings"/>.
        /// </summary>
        /// <param name="_target">Group to register the associated symbols.</param>
        private static void RegisterScriptingSymbols(BuildTargetGroup _target) {

            string[] _guids = AssetDatabase.FindAssets($"t:{typeof(GameSettings).Name}");

            // No settings.
            if (_guids.Length == 0) {
                return;
            }

            GameSettings _settings = AssetDatabase.LoadAssetAtPath<GameSettings>(AssetDatabase.GUIDToAssetPath(_guids[0]));
            PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(_target), out string[] _defines);

            _settings.scriptingSymbols = _defines;
            EditorUtility.SetDirty(_settings);
        }
        #endregion
    }
    #endif

    // ===== Settings ===== \\

    /// <summary>
    /// Global game settings, referencing each and every <see cref="BaseSettings{T}"/> shared across the game.
    /// </summary>
    [CreateAssetMenu(fileName = "GGS_GameSettings", menuName = MenuPath + "GameSettings", order = MenuOrder - 50)]
    public sealed class GameSettings : BaseSettings<GameSettings> {
        #region Global Members
        [Section("Game Settings")]

        [SerializeField] private ScriptableSettings[] settings = new ScriptableSettings[0];

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("All scripting symbols currently defined in the game")]
        [SerializeField, Enhanced, ReadOnly] internal string[] scriptingSymbols = new string[0];
        #endregion

        #region Initialization
        internal protected override void Init() {
            base.Init();

            // Settings and databases assignement.
            ref ScriptableSettings[] _span = ref settings;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                ScriptableSettings _setting = _span[i];

                #if DEVELOPMENT
                if (_setting == null) {
                    this.LogErrorMessage($"Null setting detected at index {i}");
                    continue;
                }
                #endif

                _setting.Init();
            }
        }
        #endregion

        #region Scripting Symbols
        /// <summary>
        /// Get all defined scripting symbols.
        /// </summary>
        /// <returns>An array of all defined scripting symbols.</returns>
        public string[] GetScriptingSymbols() {
            return scriptingSymbols;
        }

        /// <summary>
        /// Get if a specific scripting symbol is defined.
        /// </summary>
        /// <param name="_symbol">Scripting symbol to check definition.</param>
        /// <returns>True if the given symbol is defined, false otherwise.</returns>
        public bool IsScriptingSymbolDefined(string _symbol) {
            return ArrayUtility.Contains(scriptingSymbols, _symbol);
        }
        #endregion

        #region Editor Tool
        /// <summary>
        /// Editor utility, retrieving all inputs assets from the project.
        /// </summary>
        [Button(ActivationMode.Editor, SuperColor.Green, IsDrawnOnTop = false)]
        private void Setup() {
            #if UNITY_EDITOR
            // Settings.
            var _types = TypeCache.GetTypesDerivedFrom<ScriptableSettings>();
            ArrayUtility.RemoveNulls(ref settings);

            foreach (Type _type in _types) {

                if (_type.IsAbstract || _type.IsGenericType || (_type == GetType())) {
                    continue;
                }

                if (Array.Find(settings, s => s.GetType().IsSameOrSubclass(_type))) {
                    continue;
                }

                Object _asset = LoadObject(_type);
                if (_asset == null) {

                    // Create setting.
                    string _prefix = _type.IsSubclassOfGeneric(typeof(BaseDatabase<>)) ? "DT_" : "GS_";
                    string _path   = Path.Combine(GetProjectFolder(), $"{_prefix}{_type.Name}.asset");

                    _path  = AssetDatabase.GenerateUniqueAssetPath(_path);
                    _asset = CreateInstance(_type);

                    AssetDatabase.CreateAsset(_asset, _path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                ArrayUtility.Add(ref settings, _asset as ScriptableSettings);
            }

            Array.Sort(settings, (a, b) => a.name.CompareTo(b.name));
            EditorUtility.SetDirty(this);

            // ----- Local Methods ----- \\

            static Object LoadObject(Type _type) {
                if (AssetDatabase.FindAssets($"t:{_type.Name}").SafeFirst(out string _guid)) {
                    return AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_guid), _type);
                }

                return null;
            }

            static string GetProjectFolder() {

                foreach (Object _object in Selection.GetFiltered<Object>(SelectionMode.Assets)) {
                    string _path = AssetDatabase.GetAssetPath(_object);

                    if (string.IsNullOrEmpty(_path)) {
                        continue;
                    }

                    if (Directory.Exists(_path)) {
                        return _path;
                    }
                    
                    if (File.Exists(_path)) {
                        return Path.GetDirectoryName(_path);
                    }
                }

                return UnityEditorInternal.InternalEditorUtility.GetAssetsFolder();
            }
            #endif
        }
        #endregion
    }
}
