// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Base non generic class inherited by <see cref="SaveDataGroupManager{T}"/>.
    /// </summary>
    [Serializable]
    public abstract class SaveDataGroupManager {
        #region State
        /// <summary>
        /// All <see cref="SaveDataGroupManager"/>-related possible states.
        /// </summary>
        public enum State {
            Inactive = 0,

            Loading  = 1,
            Saving   = 2,
        }
        #endregion
    }

    /// <summary>
    /// Utility class used to save and load data for a specific <see cref="SaveDataGroup"/>.
    /// </summary>
    /// <typeparam name="T">This group related data object type.</typeparam>
    [Serializable]
    public sealed class SaveDataGroupManager<T> : SaveDataGroupManager where T : ISaveable {
        #region Global Members
        [Tooltip("All currently registered dynamic (instantiated) objects")]
        [SerializeField] private List<T> dynamicObjects = new List<T>();

        [Tooltip("All static (non instantiated) objects")]
        [SerializeField] private List<T> staticObjects  = new List<T>(); // Sorted by id.

        [Space(5f)]

        [Tooltip("Unique identifier of this group data")]
        [SerializeField, Enhanced, ReadOnly] private int guid = 0;

        [Space(5f)]

        [Tooltip("Counter used for loading and saving dynamic object data")]
        [SerializeField, Enhanced, ReadOnly] private int dynamicObjectCounter = 0;
        [Tooltip("Counter used for loading and saving static object data")]
        [SerializeField, Enhanced, ReadOnly] private int staticObjectCounter  = 0;

        [Space(5f)]

        [Tooltip("Last effective status of this object")]
        [SerializeField, Enhanced, ReadOnly] private State lastStatus = State.Inactive;
        [Tooltip("Current status of this object")]
        [SerializeField, Enhanced, ReadOnly] private State status     = State.Inactive;

        // -----------------------

        [NonSerialized] private SaveDataGroup data = null;

        // -----------------------

        /// <summary>
        /// Indicates if this object is currently processing any operation.
        /// </summary>
        public bool IsProcessing {
            get { return status != State.Inactive; }
        }

        /// <summary>
        /// Current status of this object.
        /// </summary>
        public State Status {
            get { return status; }
        }
        #endregion

        #region Core
        private const long FrameMaxLoadDuration = 100L; // In milliseconds, so about 0,1 second.
        private const long FrameMaxSaveDuration = 100L; // In milliseconds, so about 0,1 second.
        private static Stopwatch timeWatcher    = new Stopwatch();

        // -----------------------

        internal void DataUpdate() {
            // Saving...
            if (status == State.Saving) {

                ref Stopwatch _watcher = ref timeWatcher;
                _watcher.Restart();

                // Fixed objects first, then dynamic - for potential dependencies.
                if (!SaveObjectsData(ref staticObjects , ref staticObjectCounter , ref data.StaticData , ref _watcher, true  ) &&
                    !SaveObjectsData(ref dynamicObjects, ref dynamicObjectCounter, ref data.DynamicData, ref _watcher, false)) {

                    // Loading is over.
                    SetStatus(State.Inactive);
                }

                return;
            }

            // Loading...
            if (status == State.Loading) {

                ref Stopwatch _watcher = ref timeWatcher;
                _watcher.Restart();

                // Fixed objects first, then dynamic - for potential dependencies.
                if (!LoadObjectsData(ref staticObjects , ref staticObjectCounter , ref data.StaticData , ref _watcher, true  ) &&
                    !LoadObjectsData(ref dynamicObjects, ref dynamicObjectCounter, ref data.DynamicData, ref _watcher, false)) {

                    // Loading is over.
                    SetStatus(State.Inactive);
                }

                return;
            }

            // ----- Local Methods ----- \\

            static bool SaveObjectsData(ref List<T> _objects, ref int _counter, ref SaveDataList _saveData, ref Stopwatch _watcher, bool _isStatic) {
                int _count = _objects.Count;
                while (_counter < _count) {

                    // Save.
                    ISaveable _instance = _objects[_counter];
                    ObjectSaveData _data;

                    if (_isStatic ? _saveData.GetStaticObjectData(_instance, out _data, true) : _saveData.GetObjectData(_instance, out _data, true)) {
                        _instance.Serialize(_data);
                    }

                    _counter++;

                    // Stop for this frame.
                    if (_watcher.ElapsedMilliseconds > FrameMaxSaveDuration) {
                        return true;
                    }
                }

                return false;
            }

            static bool LoadObjectsData(ref List<T> _objects, ref int _counter, ref SaveDataList _saveData, ref Stopwatch _watcher, bool _isStatic) {
                int _count = _objects.Count;
                while (_counter < _count) {

                    // Load.
                    ISaveable _instance = _objects[_counter];
                    ObjectSaveData _data;

                    if (_isStatic ? _saveData.GetStaticObjectData(_instance, out _data) : _saveData.GetObjectData(_instance, out _data)) {
                        _instance.Deserialize(_data);
                    }

                    _counter++;

                    // Stop for this frame.
                    if (_watcher.ElapsedMilliseconds > FrameMaxLoadDuration) {
                        return true;
                    }
                }

                return false;
            }
        }
        #endregion

        #region Registration
        // -------------------------------------------
        // Static
        // -------------------------------------------

        /// <summary>
        /// Adds static objects to this group.
        /// </summary>
        public void AddStaticObjects(IList<T> _objects) {
            staticObjects.AddRange(_objects);
        }

        /// <summary>
        /// Set this group static objects.
        /// </summary>
        public void SetStaticObjects(IList<T> _objects) {
            staticObjects.ReplaceBy(_objects);
            SortStaticObjects();
        }

        /// <summary>
        /// Sorts all static objects by their id.
        /// </summary>
        public void SortStaticObjects() {
            ref List<T> _span = ref staticObjects;

            // Sort to ensure objects are always checked in the same order.
            if (_span.Count != 0) {
                SortSavableObjects(_span);
            }
        }

        // -------------------------------------------
        // Dynamic
        // -------------------------------------------

        /// <summary>
        /// Registers a new dynamic object instance.
        /// </summary>
        /// <param name="_object">Dynamic instance to register.</param>
        /// <param name="_loadData">If true, starts loading all objects data.</param>
        public void RegisterDynamicObject(T _object, bool _loadData = true) {
            dynamicObjects.Add(_object);

            if (_loadData) {
                LoadData(false);
            }
        }

        /// <summary>
        /// Registers multiple new dynamic object instances.
        /// </summary>
        /// <param name="_objects">All dynamic instances to register.</param>
        /// <param name="_loadData">If true, starts loading all objects data.</param>
        public void RegisterDynamicObjects(IList<T> _objects, bool _loadData = true) {
            dynamicObjects.AddRange(_objects);

            if (_loadData) {
                LoadData(false);
            }
        }

        /// <summary>
        /// Registers a modified dynamic object instance.
        /// </summary>
        /// <inheritdoc cref="RegisterDynamicObject"/>
        public void RegisterModifiedDynamicObject(T _object, bool _loadData = true) {
            int _index = dynamicObjects.IndexOf(_object);
            if (_index == -1) {
                dynamicObjects.Add(_object);

            } else if (dynamicObjectCounter > _index) {

                // Shift elements to properly load this object data.
                dynamicObjects.Move(_index, dynamicObjects.Count - 1);
                dynamicObjectCounter--;
            }

            if (_loadData) {
                LoadData(false);
            }
        }

        /// <summary>
        /// Unregisters a given dynamic object instance.
        /// </summary>
        /// <param name="_object">Dynamic instance to unregister.</param>
        /// <param name="_saveData">If true, saves this specific object data.</param>
        public void UnregisterDynamicObject(T _object, bool _saveData = true) {
            int _index = dynamicObjects.IndexOf(_object);
            if (_index != -1) {

                // Already loaded - decrease counter.
                if (_index < dynamicObjectCounter) {
                    dynamicObjectCounter--;
                }

                dynamicObjects.Remove(_object);
            }

            if (_saveData) {
                SaveDynamicData(_object);
            }
        }
        #endregion

        #region Data
        // -------------------------------------------
        // Load / Save Data
        // -------------------------------------------

        /// <summary>
        /// Loads all this group data.
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.Green)]
        public void LoadData(bool _resetCounters = true) {
            // Data could not be found.
            if (!GetData(out SaveDataGroup _sceneData, false)) {
                return;
            }

            ResetCounterIfNeeded(_sceneData, State.Saving, _resetCounters);

            lastStatus = State.Loading;
            SetStatus(State.Loading);
        }

        /// <summary>
        /// Saves all this group data.
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.HarvestGold)]
        public void SaveData(bool _resetCounters = true) {
            if (!GetData(out SaveDataGroup _sceneData, true)) {
                this.LogErrorMessage("Scene Data could not be found");
                return;
            }

            ResetCounterIfNeeded(_sceneData, State.Loading, _resetCounters);

            lastStatus = State.Saving;
            SetStatus(State.Saving);
        }

        /// <summary>
        /// Saves a specific dynamic object instance data.
        /// </summary>
        /// <param name="_object">Dynamic instance to save data.</param>
        public void SaveDynamicData(ISaveable _object) {
            if (!GetData(out SaveDataGroup _sceneData, true)) {
                this.LogErrorMessage("Scene Data could not be found");
                return;
            }

            if (_sceneData.DynamicData.GetObjectData(_object, out ObjectSaveData _data)) {
                _object.Serialize(_data);
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get this group associated <see cref="SaveDataGroup"/>.
        /// </summary>
        private bool GetData(out SaveDataGroup _groupData, bool _autoCreate) {
            _groupData = data;
            if (_groupData != null)
                return true;

            if (SaveManager.Instance.GameSaveData.GetDataGroup(guid, out _groupData, _autoCreate)) {
                data = _groupData;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resets data-related counters if required.
        /// </summary>
        private void ResetCounterIfNeeded(SaveDataGroup _sceneData, State _resetIfState, bool _forceReset) {
            if (_forceReset || (lastStatus == _resetIfState)) {

                dynamicObjectCounter = 0;
                staticObjectCounter = 0;

                _sceneData.StaticData.ResetStaticCounter();
            }
        }
        #endregion

        #region Utility
        private static readonly Comparison<T> savableObjectComparison = CompareSavableObjects;

        // -----------------------

        /// <summary>
        /// Set this group unique guid.
        /// </summary>
        /// <param name="_guid">New guid of this group.</param>
        public void SetGUID(int _guid) {
            guid = _guid;
        }

        /// <summary>
        /// Set this object current status.
        /// </summary>
        private void SetStatus(State _status) {
            status = _status;
        }

        // -------------------------------------------
        // Sorting
        // -------------------------------------------

        internal static void SortSavableObjects(List<T> _collection) {
            _collection.Sort(savableObjectComparison);
        }

        private static int CompareSavableObjects(T a, T b) {
            return a.ID.ObjectID.CompareTo(b.ID.ObjectID);
        }
        #endregion
    }
}
