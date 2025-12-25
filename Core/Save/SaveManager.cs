// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if NEWTONSOFT
#define JSON_SERIALIZATION
#endif

using EnhancedEditor;
using System.Collections.Generic;
using UnityEngine;

#if JSON_SERIALIZATION
using Newtonsoft.Json;
using System;
#endif

namespace EnhancedFramework.Core {
    /// <summary>
    /// Application runtime serialization class.
    /// <br/> Used to serialize and deserialize data for any object in the game.
    /// </summary>
    [ScriptGizmos(false, true)]
    [DefaultExecutionOrder(-945)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Save/Save Manager"), DisallowMultipleComponent]
    public sealed class SaveManager : EnhancedSingleton<SaveManager>, IStableUpdate, ILoadingProcessor {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Stable;

        #region State
        /// <summary>
        /// All <see cref="SaveManager"/> possible states.
        /// </summary>
        public enum State {
            Inactive = 0,

            Saving  = 1,
            Loading = 2,
        }
        #endregion

        #region Loading Processor
        public override bool IsLoadingProcessor => true;

        public bool IsProcessing {
            get { return globalDataGroup.IsProcessing || (state != State.Inactive); }
        }
        #endregion

        #region Global Members
        [Section("Save Manager")]

        [Tooltip("Default scene manager used when no other is available")]
        [SerializeField, Enhanced, Required] private SceneSaveDataManager defaultSceneManager = null;

        [Tooltip("All currently loaded and active scene-bound save data manager")]
        [SerializeField, Enhanced, ReadOnly] private List<SceneSaveDataManager> activeSceneManagers = new List<SceneSaveDataManager>();

        [Space(10f)]

        [SerializeField] private SaveDataGroupManager<ISaveable> globalDataGroup = new SaveDataGroupManager<ISaveable>();

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Current state of this manager")]
        [SerializeField] private State state = State.Inactive;

        [Tooltip("Active while a pending loading is waiting for an operation to be complete")]
        [SerializeField] private bool isWaitingForLoading = false;

        [Space(5f)]

        [Tooltip("All current game save data")]
        [SerializeField] private GameSaveData gameSaveData = new GameSaveData();

        // -----------------------

        [NonSerialized] private readonly GameSaveData lastSaveData = new GameSaveData();

        // -----------------------

        /// <summary>
        /// Current game save data.
        /// </summary>
        public GameSaveData GameSaveData {
            get { return gameSaveData; }
        }

        /// <summary>
        /// Last saved game data.
        /// </summary>
        public GameSaveData LastSaveData {
            get { return lastSaveData; }
        }

        /// <summary>
        /// Current state of this object.
        /// </summary>
        public State Status {
            get { return state; }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Registration.
            EnhancedSceneManager.OnPreUnloadBundle += OnPreUnloadBundle;
        }

        void IStableUpdate.Update() {

            // Global.
            globalDataGroup.DataUpdate();

            // Scenes.
            ref List<SceneSaveDataManager> _managers = ref activeSceneManagers;
            for (int i = _managers.Count; i-- > 0;) {
                _managers[i].DataUpdate();
            }

            // State.
            switch (state) {

                case State.Saving:
                    if (IsProcessing(ref _managers))
                        break;

                    OnSaveComplete();
                    break;

                case State.Loading:
                    if (IsProcessing(ref _managers))
                        break;

                    OnLoadComplete();
                    break;

                case State.Inactive:
                default:
                    break;
            }

            // ----- Local Method ----- \\

            bool IsProcessing(ref List<SceneSaveDataManager> _managers) {
                if (globalDataGroup.IsProcessing)
                    return true;

                for (int i = _managers.Count; i-- > 0;) {
                    if (_managers[i].IsProcessing)
                        return true;
                }

                return false;
            }
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Unregistration.
            EnhancedSceneManager.OnPreUnloadBundle -= OnPreUnloadBundle;
        }

        // -------------------------------------------
        // Callback(s)
        // -------------------------------------------

        private void OnPreUnloadBundle(SceneBundle _bundle) {
            ref List<SceneSaveDataManager> _managers = ref activeSceneManagers;
            for (int i = _managers.Count; i-- > 0;) {
                _managers[i].OnPreUnloadBundle(_bundle);
            }
        }
        #endregion

        #region Registration
        // -------------------------------------------
        // Global Objects
        // -------------------------------------------

        /// <summary>
        /// Set all static global data for save.
        /// </summary>
        /// <param name="_data">All static global data.</param>
        internal void SetStaticGlobalObjects(SaveSettings.SavableDataGroup[] _data) {
            ref var _dataGroup = ref globalDataGroup;
            int _count = _data.Length;

            for (int i = 0; i < _count; i++) {
                _dataGroup.AddStaticObjects(_data[i].Data);
            }

            _dataGroup.SortStaticObjects();
        }

        /// <summary>
        /// Registers a new dynamic <see cref="ISaveable"/> for save.
        /// </summary>
        /// <param name="_object"><see cref="ISaveable"/> instance to register.</param>
        public void RegisterDynamicGlobalObject(ISaveable _object, bool _loadData = true) {
            globalDataGroup.RegisterDynamicObject(_object, _loadData);
        }

        /// <summary>
        /// Registers multiple dynamic <see cref="ISaveable"/> for save.
        /// </summary>
        /// <param name="_objects">All <see cref="ISaveable"/> instances to register.</param>
        public void RegisterDynamicGlobalObjects(IList<ISaveable> _objects, bool _loadData = true) {
            globalDataGroup.RegisterDynamicObjects(_objects, _loadData);
        }

        /// <summary>
        /// Unregisters a dynamic <see cref="ISaveable"/> from save.
        /// </summary>
        /// <param name="_object"><see cref="ISaveable"/> instance to unregister.</param>
        public void UnregisterDynamicGlobalObject(ISaveable _object, bool _saveData = true) {
            globalDataGroup.UnregisterDynamicObject(_object, _saveData);
        }

        // -------------------------------------------
        // Dynamic Scene Objects
        // -------------------------------------------

        /// <summary>
        /// Registers a new scene-bound dynamic object instance.
        /// </summary>
        /// <returns>True if the object could be successfully registered, false otherwise.</returns>
        /// <inheritdoc cref="SceneSaveDataManager.RegisterDynamicObject"/>
        public void RegisterDynamicSceneObject(EnhancedBehaviour _behaviour, bool _loadData) {
            GetSceneManager(_behaviour).RegisterDynamicObject(_behaviour, _loadData);
        }

        /// <summary>
        /// Registers a modified scene-bound dynamic object instance.
        /// </summary>
        /// <inheritdoc cref="RegisterDynamicSceneObject"/>
        public void RegisterModifiedDynamicSceneObject(EnhancedBehaviour _behaviour, bool _loadData) {
            GetSceneManager(_behaviour).RegisterModifiedDynamicObject(_behaviour, _loadData);
        }

        /// <summary>
        /// Unregisters a new scene-bound dynamic object instance.
        /// </summary>
        /// <returns>True if the object could be successfully unregistered, false otherwise.</returns>
        /// <inheritdoc cref="SceneSaveDataManager.UnregisterDynamicObject"/>
        public void UnregisterDynamicSceneObject(EnhancedBehaviour _behaviour, bool _saveData) {
            GetSceneManager(_behaviour).UnregisterDynamicObject(_behaviour, _saveData);
        }

        // -------------------------------------------
        // Scene Manager
        // -------------------------------------------

        /// <summary>
        /// Registers a new active <see cref="SceneSaveDataManager"/> instance.
        /// </summary>
        /// <param name="_sceneManager"><see cref="SceneSaveDataManager"/> instance to register.</param>
        public void RegisterSceneManager(SceneSaveDataManager _sceneManager) {
            activeSceneManagers.Add(_sceneManager);
        }

        /// <summary>
        /// Unregisters a <see cref="SceneSaveDataManager"/> instance.
        /// </summary>
        /// <param name="_sceneManager"><see cref="SceneSaveDataManager"/> instance to unregister.</param>
        public void UnregisterSceneManager(SceneSaveDataManager _sceneManager) {
            activeSceneManagers.Remove(_sceneManager);
        }
        #endregion

        #region Core
        private static readonly GameSaveData quickBufferData = new GameSaveData();

        private Action<GameSaveData> onLoadingComplete = null;
        private Action<GameSaveData> onSavingComplete  = null;

        private GameSaveData pendingLoadData = null;

        // -------------------------------------------
        // Save & Load
        // -------------------------------------------

        /// <summary>
        /// Saves the current game data.
        /// </summary>
        /// <param name="_onComplete">Called after the saving operation is complete, with saved data as parameter.</param>
        /// <returns>True if successfully entered in saving state, false otherwise.</returns>
        public bool SaveData(Action<GameSaveData> _onComplete = null) {
            // Cannot save while loading is in process.
            if (state == State.Loading)
                return false;

            onSavingComplete = _onComplete;
            SetState(State.Saving);

            SaveGlobalData(true);
            SaveSceneData (true);
            return true;
        }

        /// <summary>
        /// Loads a given <see cref="Core.GameSaveData"/>.
        /// </summary>
        /// <param name="_data"><see cref="Core.GameSaveData"/> to load.</param>
        /// <param name="_onComplete">Called after the loading operation is complete, with loaded data as parameter.</param>
        /// <returns>True if successfully entered in loading state, false otherwise.</returns>
        public bool LoadData(GameSaveData _data, Action<GameSaveData> _onComplete = null) {
            // A loading is already in process.
            if (state == State.Loading)
                return false;

            onLoadingComplete = _onComplete;

            if (state == State.Saving) {
                isWaitingForLoading = true;
                pendingLoadData = _data;
            } else {
                LoadData_Internal(_data);
            }

            return true;
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        /// <summary>
        /// Called whenever a saving operation is completed.
        /// </summary>
        private void OnSaveComplete() {
            lastSaveData.Copy(gameSaveData);

            onSavingComplete?.Invoke(gameSaveData); // Use callback to perform additional operation - write data as file, etc.

            if (isWaitingForLoading) {
                LoadData_Internal(pendingLoadData);
            } else {
                SetState(State.Inactive);
            }
        }

        /// <summary>
        /// Called whenever a loading operation is completed.
        /// </summary>
        private void OnLoadComplete() {
            onLoadingComplete?.Invoke(gameSaveData);
            SetState(State.Inactive);
        }

        /// <summary>
        /// Force loading a given data.
        /// </summary>
        private void LoadData_Internal(GameSaveData _data) {
            // Replace current data.
            gameSaveData.Copy(_data);

            // Start loading.
            SetState(State.Loading);
            LoadAllData();
        }

        // -------------------------------------------
        // Editor
        // -------------------------------------------

        /// <summary>
        /// Editor button used to test the saving functionality.
        /// </summary>
        /// <param name="_fileName">Optionnal name of the file to save data as - use the quick buffer data if empty.</param>
        [Button(ActivationMode.Play, SuperColor.HarvestGold), DisplayName("Save")]
        #pragma warning disable
        private void EditorSaveGame(string _fileName = "", bool _openDirectory = false) {
            this.LogMessage("Starts saving...");
            SaveData(OnComplete);

            // ----- Local Method ----- \\

            void OnComplete(GameSaveData _data) {
                if (string.IsNullOrEmpty(_fileName)) {
                    quickBufferData.Copy(_data);
                } else {
                    WriteSaveData(_data, _fileName);
                }

                this.LogMessage("Save completed");

                if (_openDirectory) {
                    SaveSettings.I.OpenDirectory();
                }
            }
        }

        /// <summary>
        /// Editor button used to test the loading functionality.
        /// </summary>
        /// <param name="_fileName">Optionnal name of the file to read data from - use the quick buffer data if empty.</param>
        [Button(ActivationMode.Play, SuperColor.Crimson), DisplayName("Load")]
        #pragma warning disable
        private void EditorLoadGame(string _fileName = "") {
            GameSaveData _data;

            if (string.IsNullOrEmpty(_fileName)) {
                _data = quickBufferData;
            } else if (!ReadSaveDataFromFile(_fileName, out _data))
                return;

            this.LogMessage("Starts loading...");
            LoadData(_data, OnComplete);

            // ----- Local Method ----- \\

            void OnComplete(GameSaveData _data) {
                this.LogMessage("Save loaded");
            }
        }
        #endregion

        #region Data
        // -------------------------------------------
        // General
        // -------------------------------------------

        /// <summary>
        /// Loads all game data.
        /// </summary>
        public void LoadAllData(bool _resetCounters = true) {
            LoadGlobalData(_resetCounters);
            LoadSceneData(_resetCounters);
        }

        /// <summary>
        /// Saves all game data.
        /// </summary>
        public void SaveAllData(bool _resetCounters = true) {
            SaveGlobalData(_resetCounters);
            SaveSceneData(_resetCounters);
        }

        // -------------------------------------------
        // Global
        // -------------------------------------------

        /// <summary>
        /// Loads all global data.
        /// </summary>
        public void LoadGlobalData(bool _resetCounters = true) {
            globalDataGroup.LoadData(_resetCounters);
        }

        /// <summary>
        /// Saves all global data.
        /// </summary>
        public void SaveGlobalData(bool _resetCounters = true) {
            globalDataGroup.SaveData(_resetCounters);
        }

        /// <summary>
        /// Saves a specific dynamic object instance data.
        /// </summary>
        /// <param name="_object">Dynamic instance to save data.</param>
        public void SaveDynamicData(ISaveable _object) {
            globalDataGroup.SaveDynamicData(_object);
        }

        // -------------------------------------------
        // Scene
        // -------------------------------------------

        /// <summary>
        /// Loads all scene-related game data.
        /// </summary>
        public void LoadSceneData(bool _resetCounters = true) {
            ref List<SceneSaveDataManager> _managers = ref activeSceneManagers;
            for (int i = _managers.Count; i-- > 0;) {
                _managers[i].LoadData(_resetCounters);
            }
        }

        /// <summary>
        /// Saves all scene-related game data.
        /// </summary>
        public void SaveSceneData(bool _resetCounters = true) {
            ref List<SceneSaveDataManager> _managers = ref activeSceneManagers;
            for (int i = _managers.Count; i-- > 0;) {
                _managers[i].SaveData(_resetCounters);
            }
        }
        #endregion

        #region File
        // -------------------------------------------
        // Write
        // -------------------------------------------

        /// <summary>
        /// Writes a given <see cref="Core.GameSaveData"/> as a file on disk.
        /// </summary>
        /// <param name="_data"><see cref="Core.GameSaveData"/> to save and write as a file on disk.</param>
        /// <inheritdoc cref="WriteSaveData(string, string)"/>
        public bool WriteSaveData(GameSaveData _data, string _fileName) {
            #if JSON_SERIALIZATION
            string _jsonData;
            try {
                _jsonData = JsonConvert.SerializeObject(_data);
            } catch (Exception e) {
                this.LogErrorMessage("An error occured - game data could not be properly serialized. See exception log for additional information");
                this.LogException(e);

                return false;
            }

            return WriteSaveData(_jsonData, _fileName);
            #else
            this.LogErrorMessage("Please import NEWTONSOFT package to serialize and write data");
            return false;
            #endif
        }

        /// <summary>
        /// Writes a given data as a file on disk.
        /// </summary>
        /// <param name="_jsonData"><see cref="Core.GameSaveData"/> serialized as json to save and write as a file on disk.</param>
        /// <param name="_fileName">Name of the save file to write.</param>
        /// <returns>True if the file could be successfully saved, false otherwise.</returns>
        public bool WriteSaveData(string _jsonData, string _fileName) {
            return SaveSettings.I.WriteFile(_jsonData, _fileName);
        }

        // -------------------------------------------
        // Read
        // -------------------------------------------

        /// <summary>
        /// Reads a given file name from disk and create a new <see cref="Core.GameSaveData"/> from its content.
        /// </summary>
        /// <param name="_fileName">Name of the file to read.</param>
        /// <param name="_data">Newly created data from the given file.</param>
        /// <returns>True if the file could be successfully read and converted, false otheriwse.</returns>
        public bool ReadSaveDataFromFile(string _fileName, out GameSaveData _data) {
            if (!SaveSettings.I.ReadFile(_fileName, out string _jsonData)) {
                this.LogErrorMessage($"An error occured - game data could not be read from file \"{_fileName}\"");

                _data = null;
                return false;
            }

            return ReadSaveData(_jsonData, out _data);
        }

        /// <summary>
        /// Reads a given json data and create a new <see cref="Core.GameSaveData"/> from its content.
        /// </summary>
        /// <param name="_jsonData">Json data to read.</param>
        /// <param name="_data">Newly created data from the given json.</param>
        /// <returns>True if the json could be successfully converted, false otheriwse.</returns>
        public bool ReadSaveData(string _jsonData, out GameSaveData _data) {
            #if JSON_SERIALIZATION
            try {
                _data = JsonConvert.DeserializeObject<GameSaveData>(_jsonData);
                return true;
            } catch (Exception e) {
                this.LogErrorMessage("An error occured - game data could not be properly deserialized. See exception log for additional information");
                this.LogException(e);

                _data = null;
                return false;
            }
            #else
            this.LogErrorMessage("Please import NEWTONSOFT package to serialize and read data");

            _data = null;
            return false;
            #endif
        }

        /// <summary>
        /// Reads a given file name from disk and populate a given <see cref="Core.GameSaveData"/> from its content.
        /// </summary>
        /// <param name="_fileName">Name of the file to read.</param>
        /// <param name="_data">Existing data to populate from file content.</param>
        /// <returns>True if the file could be successfully read and the data populated, false otheriwse.</returns>
        public bool ReadSaveDataFromFile(string _fileName, GameSaveData _data) {
            if (!SaveSettings.I.ReadFile(_fileName, out string _jsonData)) {
                this.LogErrorMessage($"An error occured - game data could not be read from file \"{_fileName}\"");
                return false;
            }

            return ReadSaveData(_jsonData, _data);
        }

        /// <summary>
        /// Reads a given json data and populate a given <see cref="Core.GameSaveData"/> from its content.
        /// </summary>
        /// <param name="_jsonData">Json data to read.</param>
        /// <param name="_data">Existing data to populate from json content.</param>
        /// <returns>True if the json could be successfully read and the data populated, false otheriwse.</returns>
        public bool ReadSaveData(string _jsonData, GameSaveData _data) {
            #if JSON_SERIALIZATION
            try {
                JsonConvert.PopulateObject(_jsonData, _data);
                return true;
            } catch (Exception e) {
                this.LogErrorMessage("An error occured - game data could not be properly deserialized. See exception log for additional information");
                this.LogException(e);

                return false;
            }
            #else
            this.LogErrorMessage("Please import NEWTONSOFT package to serialize and read data");

            _data = null;
            return false;
            #endif
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <inheritdoc cref="SaveSettings.GetAllFiles"/>
        public string[] GetAllFiles() {
            return SaveSettings.I.GetAllFiles();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get the <see cref="SceneSaveDataManager"/> for the scene associated with a given <see cref="GameObject"/> instance.
        /// </summary>
        /// <param name="_sceneInstance">Instance to get the associated scene-related <see cref="SceneSaveDataManager"/>.</param>
        /// <returns><see cref="SceneSaveDataManager"/> associated with this instance.</returns>
        public SceneSaveDataManager GetSceneManager(EnhancedBehaviour _sceneInstance) {
            int _buildIndex = _sceneInstance.gameObject.scene.buildIndex;
            ref List<SceneSaveDataManager> _managers = ref activeSceneManagers;

            for (int i = _managers.Count; i-- > 0;) {
                SceneSaveDataManager _manager = _managers[i];

                if (_manager.gameObject.scene.buildIndex == _buildIndex) {
                    return _manager;
                }
            }

            return defaultSceneManager;
        }

        /// <summary>
        /// Set this object current state.
        /// </summary>
        private void SetState(State _state) {
            state = _state;
        }
        #endregion
    }
}
