// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using SceneAsset = EnhancedEditor.SceneAsset;
#endif

namespace EnhancedFramework.Core {
    /// <summary>
    /// Scene-bound manager class, used to reference all save data bound objects from this scene
    /// <br/> and save / load their value.
    /// </summary>
    [ScriptGizmos(false, true)]
    [DefaultExecutionOrder(-944)] // Execute before any other scripts.
    [AddComponentMenu(FrameworkUtility.MenuPath + "Save/Scene Save Data Manager"), DisallowMultipleComponent]
    public sealed class SceneSaveDataManager : EnhancedBehaviour, ILoadingProcessor, IEnhancedSceneOperator {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init;

        #region Loading Processor
        public override bool IsLoadingProcessor => true;

        public bool IsProcessing {
            get { return dataGroup.IsProcessing; }
        }
        #endregion

        #region Global Members
        [Section("Scene Save Data Manager")]

        [SerializeField, Enhanced, Block] private SaveDataGroupManager<EnhancedBehaviour> dataGroup = new SaveDataGroupManager<EnhancedBehaviour>();

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Set to true when this object scene is about to be unloaded")]
        [SerializeField, Enhanced, ReadOnly] private bool isBeingUnloaded = false;
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Registration.
            SaveManager.Instance.RegisterSceneManager(this);
        }

        protected override void OnInit() {
            base.OnInit();

            // Load data for all registered objects.
            LoadData(true);
        }

        internal void DataUpdate() {
            dataGroup.DataUpdate();
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Unregistration.
            SaveManager.Instance.UnregisterSceneManager(this);
        }
        #endregion

        #region Registration
        /// <inheritdoc cref="SaveDataGroupManager{T}.RegisterDynamicObject(T, bool)"/>
        public void RegisterDynamicObject(EnhancedBehaviour _object, bool _loadData = true) {
            dataGroup.RegisterDynamicObject(_object, _loadData);
        }

        /// <inheritdoc cref="SaveDataGroupManager{T}.RegisterModifiedDynamicObject(T, bool)"/>
        public void RegisterModifiedDynamicObject(EnhancedBehaviour _object, bool _loadData = true) {
            dataGroup.RegisterModifiedDynamicObject(_object, _loadData);
        }

        /// <inheritdoc cref="SaveDataGroupManager{T}.UnregisterDynamicObject(T, bool)"/>
        public void UnregisterDynamicObject(EnhancedBehaviour _object, bool _saveData = true) {

            // This object associated scene is being unloaded - do not perform any additional operation.
            if (isBeingUnloaded)
                return;

            dataGroup.UnregisterDynamicObject(_object, _saveData);
        }
        #endregion

        #region Data
        // -------------------------------------------
        // Load / Save Data
        // -------------------------------------------

        /// <summary>
        /// Loads all this scene data.
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.Green)]
        public void LoadData(bool _resetCounters = true) {
            dataGroup.LoadData(_resetCounters);
        }

        /// <summary>
        /// Saves all this scene data.
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.HarvestGold)]
        public void SaveData(bool _resetCounters = true) {
            dataGroup.SaveData(_resetCounters);
        }

        /// <summary>
        /// Saves a specific dynamic object instance data.
        /// </summary>
        /// <param name="_object">Dynamic instance to save data.</param>
        public void SaveDynamicData(ISaveable _object) {
            dataGroup.SaveDynamicData(_object);
        }
        #endregion

        #region Utility
        private static List<EnhancedBehaviour> objectBuffer = null;

        // -----------------------

        /// <summary>
        /// Called whenever before starting to unload a <see cref="SceneBundle"/>.
        /// </summary>
        internal void OnPreUnloadBundle(SceneBundle _bundle) {
            // Notice when this object scene is about to be unloaded.
            if (!IsUnloading(ref _bundle.Scenes, gameObject.scene)) {
                return;
            }

            SaveData(true);
            isBeingUnloaded = true;

            // ----- Local Method ----- \\

            static bool IsUnloading(ref SceneAsset[] _scenes, Scene _scene) {

                for (int i = _scenes.Length; i-- > 0;) {
                    if (_scenes[i].Scene == _scene)
                        return true;
                }

                return false;
            }
        }

        // -------------------------------------------
        // Editor
        // -------------------------------------------

        /// <summary>
        /// Called from the editor to serialize all savable static objects in the scene.
        /// </summary>
        [Button(ActivationMode.Editor, SuperColor.HarvestGold)]
        public void GetSceneResources(bool _setDirty) {

            EnhancedBehaviour[] _objects = FindObjectsByType<EnhancedBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            ref List<EnhancedBehaviour> _span = ref objectBuffer;

            // Buffer init.
            if (_span == null) {
                _span = new List<EnhancedBehaviour>();
            } else {
                _span.Clear();
            }

            // Register objects.
            var _scene = gameObject.scene;

            foreach (EnhancedBehaviour _object in _objects) {
                if (_object.DoSaveData && (_object.gameObject.scene == _scene)) {
                    _span.Add(_object);
                }
            }

            // Set data.
            dataGroup.SetStaticObjects(_span);
            dataGroup.SetGUID(ID.AssetHash);

            #if UNITY_EDITOR
            // Editor stuff.
            if (_setDirty && !Application.isPlaying) {
                EditorUtility.SetDirty(this);
            }
            #endif
        }
        #endregion
    }
}
