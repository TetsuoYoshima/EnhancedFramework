// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if NEWTONSOFT
#define JSON_SERIALIZATION
#endif

using EnhancedEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

#if JSON_SERIALIZATION
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#endif

namespace EnhancedFramework.Core {
    // ===== Holders ===== \\

    /// <summary>
    /// Game general save data holder.
    /// </summary>
    [Serializable]
    public sealed class GameSaveData {
        #region Global Members
        /// <summary>
        /// All object data groups in the game.
        /// </summary>
        public List<SaveDataGroup> DataGroups = new List<SaveDataGroup>();

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="GameSaveData(GameSaveData)"/>
        public GameSaveData() { }

        /// <inheritdoc cref="GameSaveData"/>
        public GameSaveData(GameSaveData _data) {
            Copy(_data);
        }
        #endregion

        #region Utility
        // -------------------------------------------
        // Group
        // -------------------------------------------

        /// <returns>Data associated with this scene.</returns>
        /// <inheritdoc cref="GetDataGroup(SceneBundle, out SaveDataGroup, bool)"/>
        public SaveDataGroup GetDataGroup(SceneBundle _bundle) {
            return GetDataGroup(_bundle.GUID);
        }

        /// <param name="_bundle"><see cref="SceneBundle"/> to get the associated data.</param>
        /// <inheritdoc cref="GetDataGroup(int, out SaveDataGroup, bool)"/>
        public bool GetDataGroup(SceneBundle _bundle, out SaveDataGroup _data, bool _createIfNotExist = true) {
            return GetDataGroup(_bundle.GUID, out _data, _createIfNotExist);
        }

        /// <returns>Data associated with this guid.</returns>
        /// <inheritdoc cref="GetDataGroup(int, out SaveDataGroup, bool)"/>
        public SaveDataGroup GetDataGroup(int _guid) {
            GetDataGroup(_guid, out SaveDataGroup _data, true);
            return _data;
        }

        /// <summary>
        /// Get the <see cref="SaveDataGroup"/> associated with a given group.
        /// </summary>
        /// <param name="_guid">Unique ID of the group to get the associated data.</param>
        /// <param name="_data">Data associated with this group (null if none).</param>
        /// <param name="_createIfNotExist">If true, automatically creates the associated <see cref="SaveDataGroup"/> if it does not already exist.</param>
        /// <returns>True if the data could be found, false otherwise.</returns>
        public bool GetDataGroup(int _guid, out SaveDataGroup _data, bool _createIfNotExist = true) {
            ref List<SaveDataGroup> _groups = ref DataGroups;

            for (int i = _groups.Count; i-- > 0;) {
                _data = _groups[i];
                if (_data.GUID == _guid)
                    return true;
            }

            if (_createIfNotExist) {
                _data = new SaveDataGroup(_guid);
                _groups.Add(_data);
                return true;
            }

            _data = null;
            return false;
        }

        // -------------------------------------------
        // Other
        // -------------------------------------------

        /// <summary>
        /// Copies all data from a given <see cref="GameSaveData"/>.
        /// </summary>
        public void Copy(GameSaveData _data) {
            ref var _thisData = ref DataGroups;
            ref var _fromData = ref _data.DataGroups;

            int _count = _fromData.Count;
            int _startCount = Mathf.Min(_thisData.Count, _count);

            // For existing data, copy content.
            for (int i = _startCount; i-- > 0;) {
                _thisData[i].Copy(_fromData[i]);
            }

            // Create and fill missing data.
            _thisData.Resize(_count);
            for (int i = _startCount; i < _count; i++) {
                _thisData[i] = _fromData[i].CreateCopy();
            }
        }

        /// <summary>
        /// Clears all data from this object.
        /// </summary>
        public void Clear() {
            DataGroups.Clear();
        }
        #endregion
    }

    /// <summary>
    /// Wrapper for a save data group.
    /// </summary>
    [Serializable]
    public sealed class SaveDataGroup {
        #region Global Members
        /// <summary>
        /// Unique ID of this group.
        /// </summary>
        public int GUID = 0;

        /// <summary>
        /// Data for dynamic (instantiated) objects bound to this group.
        /// </summary>
        public SaveDataList DynamicData = new SaveDataList();

        /// <summary>
        /// Data for static (non instantiated) objects bound to this group.
        /// </summary>
        public SaveDataList StaticData = new SaveDataList();

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="SaveDataGroup(int)"/>
        public SaveDataGroup() { }

        /// <inheritdoc cref="SaveDataGroup"/>
        public SaveDataGroup(int _guid) {
            GUID = _guid;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Creates a new copy from this group.
        /// </summary>
        public SaveDataGroup CreateCopy() {
            SaveDataGroup _copy = new SaveDataGroup();
            _copy.Copy(this);

            return _copy;
        }

        /// <summary>
        /// Copies all data from a given <see cref="SaveDataGroup"/>.
        /// </summary>
        /// <param name="_group">Other group to copy data from.</param>
        public void Copy(SaveDataGroup _group) {
            GUID = _group.GUID;
            DynamicData.Copy(_group.DynamicData);
            StaticData .Copy(_group.StaticData);
        }
        #endregion
    }

    /// <summary>
    /// Wrapper for multiple <see cref="ObjectSaveData"/>.
    /// </summary>
    [Serializable]
    public sealed class SaveDataList {
        #region Global Members
        /// <summary>
        /// This object data content.
        /// </summary>
        public List<ObjectSaveData> Data = new List<ObjectSaveData>();
        #endregion

        #region Utility
        #if JSON_SERIALIZATION
        [JsonIgnore]
        #endif
        [NonSerialized] private int counter = 0; // Static objects are sorted by id - use this to optimize iteration.

        // -----------------------

        /// <summary>
        /// Get the data associated with a static object (faster than <see cref="GetObjectData"/>).
        /// </summary>
        /// <inheritdoc cref="GetObjectData"/>
        public bool GetStaticObjectData(ISaveable _object, out ObjectSaveData _data, bool _createIfNotExist = false) {
            ref List<ObjectSaveData> _span = ref Data;
            int _count = _span.Count;

            ref int _dataCounter = ref counter;
            EnhancedObjectID _id = _object.ID;

            for (int i = _dataCounter; i < _count; i++) {
                _data = _span[i];

                // Static objects are sorted by their object id.
                int _result = _data.CompareObjectID(_id);

                // If checked data id is superior to the given object id,
                // then it is not registered yet and there is no point to check further.
                if (_result > 0)
                    break;

                _dataCounter++;

                // Found.
                if (_result == 0)
                    return true;
            }

            // Create new data for this object.
            if (_createIfNotExist) {

                _data = new ObjectSaveData(_id);
                _span.Insert(_dataCounter++, _data);

                return true;
            }

            _data = null;
            return false;
        }

        /// <summary>
        /// Get the data associated with a given object (slower than <see cref="GetStaticObjectData"/>)
        /// </summary>
        /// <param name="_object">Object to get the associated data.</param>
        /// <param name="_data">Data associated with the given object (null if none).</param>
        /// <returns>True if this object data could be successfully found, false otherwise.</returns>
        public bool GetObjectData(ISaveable _object,  out ObjectSaveData _data, bool _createIfNotExist = false) {
            ref List<ObjectSaveData> _span = ref Data;
            EnhancedObjectID _id = _object.ID;

            for (int i = _span.Count; i-- > 0;) {

                _data = _span[i];
                if (_data.Match(_id)) {
                    return true;
                }
            }

            // Create new data for this object.
            if (_createIfNotExist) {

                _data = new ObjectSaveData(_id);
                _span.Add(_data);

                return true;
            }

            _data = null;
            return false;
        }

        /// <summary>
        /// Resets this object counter used for static data iteration.
        /// </summary>
        public void ResetStaticCounter() {
            counter = 0;
        }

        // -------------------------------------------
        // Copy
        // -------------------------------------------

        /// <summary>
        /// Copies all data from a given <see cref="SaveDataList"/>.
        /// </summary>
        /// <param name="_list">Other list to copy data from.</param>
        public void Copy(SaveDataList _list) {
            ref var _thisData = ref Data;
            ref var _fromData = ref _list.Data;

            int _count = _fromData.Count;
            int _startCount = Mathf.Min(_thisData.Count, _count);

            // For existing data, copy content.
            for (int i = _startCount; i-- > 0;) {
                _thisData[i].Copy(_fromData[i]);
            }

            // Create and fill missing data.
            _thisData.Resize(_count);
            for (int i = _startCount; i < _count; i++) {
                _thisData[i] = _fromData[i].CreateCopy();
            }
        }
        #endregion
    }

    // ===== Data ===== \\

    /// <summary>
    /// <see cref="ObjectSaveData"/> data holder for a specific type collection.
    /// </summary>
    [Serializable]
    public sealed class SaveDataType<T> {
        #region Global Members
        /// <summary>
        /// All serialized data in this wrapper.
        /// </summary>
        #if JSON_SERIALIZATION
        [JsonProperty]
        #endif
        public List<T> Data = new List<T>();

        /// <summary>
        /// Optionnal identifiers that can be used with the data collection.
        /// </summary>
        #if JSON_SERIALIZATION
        [JsonProperty]
        #endif
        public List<string> Identifiers = null;

        #if JSON_SERIALIZATION
        [JsonIgnore]
        #endif
        private int index = 0;
        #endregion

        #region Behaviour
        /// <param name="_identifier">Additional identifier to be associated with this data.</param>
        /// <inheritdoc cref="Serialize(T)"/>
        public void Serialize(T _data, string _identifier) {
            Data.Add(_data);

            ref List<string> _span = ref Identifiers;
            if (_span == null) {
                _span = new List<string>();
            }

            _span.Add(_identifier);
        }

        /// <summary>
        /// Serializes a new data.
        /// <br/> Use <see cref="ResetCounter"/> before starting serialization.
        /// </summary>
        /// <param name="_data">Data to serialize.</param>
        public void Serialize(T _data) {
            ref List<T> _span = ref Data;
            ref int _counter = ref index;

            if (_counter == _span.Count) {
                _span.Add(_data);
            } else {
                _span[_counter] = _data;
            }

            _counter++;
        }

        /// <summary>
        /// Serializes multiple data at once.
        /// </summary>
        /// <param name="_data">All data to serialize.</param>
        public void Serialize(IList<T> _data) {
            ref List<T> _span = ref Data;

            int _count = _data.Count;
            int _maxIndex = Mathf.Min(_span.Count, _count);

            for (int i = 0; i < _maxIndex; i++) {
                _span[i] = _data[i];
            }

            for (int i = _maxIndex; i < _count; i++) {
                _span.Add(_data[i]);
            }
        }

        /// <summary>
        /// Deserializes the data associated with a given identifier.
        /// <br/> Use this only if you serialized all data using <see cref="Serialize(T, string)"/>.
        /// </summary>
        /// <param name="_identifier">Identifier of the data to deserialize.</param>
        /// <param name="_data">Associated deserialized data (null if none).</param>
        /// <returns>True if the associated data could be found, false otherwise.</returns>
        public bool Deserialize(string _identifier, out T _data) {
            ref List<string> _span = ref Identifiers;

            for (int i = _span.Count; i-- > 0;) {
                if (_span[i].Equals(_identifier, StringComparison.Ordinal)) {
                    _data = Data[i];
                    return true;
                }
            }

            _data = default;
            return false;
        }

        /// <summary>
        /// Deserializes the next data.
        /// <br/> Use <see cref="ResetCounter"/> before starting deserialization.
        /// </summary>
        /// <param name="_data">Next deserialized data.</param>
        /// <returns>True if data could be successfully deserialized, false otherwise.</returns>
        public bool Deserialize(out T _data) {
            ref List<T> _span = ref Data;
            ref int _counter  = ref index;

            if (_counter == _span.Count) {
                _data = default;
                return false;
            }

            _data = _span[_counter++];
            return true;
        }

        /// <returns><inheritdoc cref="Deserialize(out T)" path="/param[@name='_data']"/></returns>
        /// <inheritdoc cref="Deserialize(out T)"/>
        public T Deserialize() {
            if (!Deserialize(out T _data)) {
                this.LogErrorMessage("Collection limit reached");
            }

            return _data;
        }

        /// <summary>
        /// Deserializes multiple data at once.
        /// </summary>
        /// <param name="_buffer">Buffer where to store data result.</param>
        /// <param name="_count">Total count of data to deserialize.</param>
        /// <returns>Total amount of deserialized data (might be inferior to given count).</returns>
        public int Deserialize(List<T> _buffer, int _count) {
            ref List<T> _span = ref Data;
            return Deserialize_Internal(ref _span, _buffer, Mathf.Min(_count, _span.Count));
        }

        /// <summary>
        /// Deserializes all data at once.
        /// </summary>
        /// <param name="_buffer">Buffer where to store data result.</param>
        /// <returns>Total amount of deserialized data.</returns>
        public int Deserialize(List<T> _buffer) {
            ref List<T> _span = ref Data;
            return Deserialize_Internal(ref _span, _buffer, _span.Count);
        }

        // -----------------------

        private int Deserialize_Internal(ref List<T> _span, List<T> _buffer, int _count) {
            int _startSize = Mathf.Min(_buffer.Count, _count);

            for (int i = 0; i < _startSize; i++) {
                _buffer[i] = _span[i];
            }

            for (int i = _startSize; i < _count; i++) {
                _buffer.Add(_span[i]);
            }

            return _count;
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Resets the counter used to efficiently serialize and deserialize data.
        /// </summary>
        /// <returns>True if there is any data saved in this wrapper, false otherwsie.</returns>
        public bool ResetCounter() {
            if (Data.Count == 0)
                return false;

            index = 0;
            return true;
        }

        /// <summary>
        /// Set the number of total elements wrapped in this data.
        /// <br/> Use this before starting serialization.
        /// </summary>
        /// <param name="_size">New size of this wrapper.</param>
        public void Resize(int _size) {
            Data.Resize(_size);
            Identifiers?.Resize(_size);
        }

        /// <summary>
        /// Copies all content from another <see cref="SaveDataType{T}"/>.
        /// </summary>
        /// <param name="_other">Other object to copy data from.</param>
        public void Copy(SaveDataType<T> _other) {
            if (_other is null) {
                Clear();
                return;
            }

            ref List<string> _span = ref Identifiers;
            if (_span != null) {
                _span.ReplaceBy(_other.Identifiers);
            } else if (_other.Identifiers != null) {
                _span = new List<string>(_other.Identifiers);
            }

            Data.ReplaceBy(_other.Data);
        }

        /// <summary>
        /// Clears this data content.
        /// </summary>
        internal void Clear() {
            Identifiers?.Clear();
            Data.Clear();
        }
        #endregion
    }

    /// <summary>
    /// <see cref="SaveManager"/> data holder used to serialize and deserialize values for a single object.
    /// </summary>
    [Serializable]
    public sealed class ObjectSaveData {
        #region Global Members
        /// <summary>
        /// Identifier of the object associated with this data.
        /// </summary>
        #if JSON_SERIALIZATION
        [JsonProperty]
        #endif
        public EnhancedObjectID ID = EnhancedObjectID.Default;

        // Struct.
        public SaveDataType<Vector3>  Vector3_data    = null;

        // Value.
        public SaveDataType<string>   String_data     = null;
        public SaveDataType<float>    Float_data      = null;
        public SaveDataType<bool>     Bool_data       = null;
        public SaveDataType<int>      Int_data        = null;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="ObjectSaveData(EnhancedObjectID)"/>
        internal ObjectSaveData() { }

        /// <inheritdoc cref="ObjectSaveData(EnhancedObjectID)"/>
        public ObjectSaveData(ISaveable _object) : this(_object.ID) { }

        /// <inheritdoc cref="ObjectSaveData"/>
        public ObjectSaveData(EnhancedObjectID _id) {
            ID.Copy(_id);
        }
        #endregion

        #region Comparison
        /// <summary>
        /// Get if this object does match a given <see cref="ISaveable"/> instance.
        /// </summary>
        /// <param name="_object">Object to check.</param>
        /// <returns>True if this object matches the given <see cref="ISaveable"/> instance, false otherwise.</returns>
        public bool Match(ISaveable _object) {
            return Match(_object.ID);
        }

        /// <summary>
        /// Get if this object does match a given id.
        /// </summary>
        /// <param name="_id">Id to check.</param>
        /// <returns>True if this object matches the given id, false otherwise.</returns>
        public bool Match(EnhancedObjectID _id) {
            return _id.Equals(ID);
        }

        /// <summary>
        /// Compares this object <see cref="EnhancedObjectID.ObjectID"/> with a given <see cref="ISaveable"/> instance.
        /// </summary>
        /// <param name="_object">Object to compare.</param>
        /// <inheritdoc cref="CompareObjectID(EnhancedObjectID)"/>
        public int CompareObjectID(ISaveable _object) {
            return CompareObjectID(_object.ID);
        }

        /// <summary>
        /// Compares this object <see cref="EnhancedObjectID.ObjectID"/> with a given id.
        /// </summary>
        /// <param name="_id">Id to compare.</param>
        /// <returns>-1 if this object id is inferior to the other, 1 if superior, 0 if they are equal.</returns>
        public int CompareObjectID(EnhancedObjectID _id) {
            return ID.ObjectID.CompareTo(_id.ObjectID);
        }
        #endregion

        #region Getter
        /// <summary>
        /// Get a <see cref="Vector3"/> wrapper to write and read data.
        /// </summary>
        /// <inheritdoc cref="GetData"/>
        public bool GetVector3Data(bool _create, bool _resetCounter, out SaveDataType<Vector3> _data) {
            return GetData(_create, _resetCounter, ref Vector3_data, out _data);
        }

        /// <summary>
        /// Get a <see cref="string"/> wrapper to write and read data.
        /// </summary>
        /// <inheritdoc cref="GetData"/>
        public bool GetStringData(bool _create, bool _resetCounter, out SaveDataType<string> _data) {
            return GetData(_create, _resetCounter, ref String_data, out _data);
        }

        /// <summary>
        /// Get a <see cref="float"/> wrapper to write and read data.
        /// </summary>
        /// <inheritdoc cref="GetData"/>
        public bool GetFloatData(bool _create, bool _resetCounter, out SaveDataType<float> _data) {
            return GetData(_create, _resetCounter, ref Float_data, out _data);
        }

        /// <summary>
        /// Get a <see cref="int"/> wrapper to write and read data.
        /// </summary>
        /// <inheritdoc cref="GetData"/>
        public bool GetIntData(bool _create, bool _resetCounter, out SaveDataType<int> _data) {
            return GetData(_create, _resetCounter, ref Int_data, out _data);
        }

        /// <summary>
        /// Get a <see cref="bool"/> wrapper to write and read data.
        /// </summary>
        /// <inheritdoc cref="GetData"/>
        public bool GetBoolData(bool _create, bool _resetCounter, out SaveDataType<bool> _data) {
            return GetData(_create, _resetCounter, ref Bool_data, out _data);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        /// <param name="_create">If true, automatically create a wrapper for this data if none exist.</param>
        /// <param name="_resetCounter">If true, automatically resets the wrapper read and write counter.</param>
        /// <param name="_data">Wrapper that can be used to write and read data.</param>
        /// <returns>True if the data wrapper can and should be used, false otherwise.</returns>
        private bool GetData<T>(bool _create, bool _resetCounter, ref SaveDataType<T> _refData, out SaveDataType<T> _data) {

            // Null data management.
            if (_refData == null) {
                if (!_create) {
                    _data = null;
                    return false;
                }

                _refData = new SaveDataType<T>();
            }

            // Get if data should be used.
            _data = _refData;
            return !_resetCounter || _refData.ResetCounter() || _create;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Setup this data for a specific <see cref="EnhancedObjectID"/>.
        /// </summary>
        /// <param name="_id"><see cref="EnhancedObjectID"/> to setup.</param>
        internal void Setup(EnhancedObjectID _id) {
            ID.Copy(_id);
            Clear();
        }

        /// <summary>
        /// Clear this data content.
        /// </summary>
        internal void Clear() {
            Vector3_data?.Clear();
            String_data ?.Clear();
            Float_data  ?.Clear();
            Bool_data   ?.Clear();
            Int_data    ?.Clear();
        }

        // -------------------------------------------
        // Copy
        // -------------------------------------------

        /// <summary>
        /// Creates a new copy from this data.
        /// </summary>
        public ObjectSaveData CreateCopy() {
            ObjectSaveData _copy = new ObjectSaveData();
            _copy.Copy(this);

            return _copy;
        }

        /// <summary>
        /// Copies all data from a given <see cref="ObjectSaveData"/>.
        /// </summary>
        /// <param name="_data">Other object to copy data from.</param>
        public void Copy(ObjectSaveData _data) {
            ID.Copy(_data.ID);

            CopyData(ref Vector3_data, ref _data.Vector3_data);
            CopyData(ref String_data , ref _data.String_data );
            CopyData(ref Float_data  , ref _data.Float_data  );
            CopyData(ref Bool_data   , ref _data.Bool_data   );
            CopyData(ref Int_data    , ref _data.Int_data    );

            // ----- Local Method ----- \\

            static void CopyData<T>(ref SaveDataType<T> _to, ref SaveDataType<T> _from) {
                if (_to is null) {
                    if (_from is null)
                        return;

                    _to = new SaveDataType<T>();
                }

                _to.Copy(_from);
            }
        }
        #endregion
    }
}
