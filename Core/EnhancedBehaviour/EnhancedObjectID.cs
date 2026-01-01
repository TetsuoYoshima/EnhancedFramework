// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
//  Based on Unity editor struct GlobalObjectId:
//      https://docs.unity3d.com/ScriptReference/GlobalObjectId.html
//
// ================================================================================== //

#if NEWTONSOFT
#define JSON_SERIALIZATION
#endif

using EnhancedEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

#if JSON_SERIALIZATION
using Newtonsoft.Json;
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using SceneAsset = EnhancedEditor.SceneAsset;
#endif

namespace EnhancedFramework.Core {
    /// <summary>
    /// Enhanced type used as a persistent id for an object.
    /// </summary>
    [Serializable]
    public sealed class EnhancedObjectID {
        #region Wrappers
        [Serializable]
        private class DynamicInstanceData {
            [SerializeField] private List<SceneInstanceData> sceneData = new List<SceneInstanceData>();

            public int Register(Component _object, ulong _prefabID) {
                int _sceneID = BuildSceneDatabase.GetSceneIdentifier(_object.gameObject.scene);

                ref List<SceneInstanceData> _span = ref sceneData;

                for (int i = _span.Count; i-- > 0;) {
                    if (_span[i].Register(_sceneID, _prefabID, out int _instanceID))
                        return _instanceID;
                }

                _span.Add(new SceneInstanceData(_sceneID, _prefabID));
                return 1;
            }

            public void Unregister(Component _object, ulong _prefabID, int _instanceID) {
                int _sceneID = BuildSceneDatabase.GetSceneIdentifier(_object.gameObject.scene);

                ref List<SceneInstanceData> _span = ref sceneData;

                for (int i = _span.Count; i-- > 0;) {
                    if (_span[i].Unregister(_sceneID, _prefabID, _instanceID))
                        return;
                }
            }

            public void OnUnloadSceneBundle(SceneBundle _sceneBundle) {
                ref List<SceneInstanceData> _span = ref sceneData;
                ref var _scenes = ref _sceneBundle.Scenes;

                int _spanCount = _span.Count;

                for (int i = _scenes.Length; i-- > 0;) {
                    int _sceneID = _scenes[i].GetIdentifier();

                    for (int j = _spanCount; j-- > 0;) {
                        if (_span[j].OnUnloadScene(_sceneID))
                            break;
                    }
                }
            }
        }

        [Serializable]
        private class SceneInstanceData {
            [SerializeField] private List<PrefabInstanceData> instanceData = new List<PrefabInstanceData>();
            [SerializeField] private int sceneID = 0;

            public SceneInstanceData(int _sceneID, ulong _prefabID) {
                sceneID = _sceneID;
                instanceData.Add(new PrefabInstanceData(_prefabID));
            }

            public bool Register(int _sceneID, ulong _prefabID, out int _instanceID) {
                if (_sceneID != sceneID) {
                    _instanceID = 0;
                    return false;
                }

                ref List<PrefabInstanceData> _span = ref instanceData;

                for (int i = _span.Count; i-- > 0;) {
                    if (_span[i].Register(_prefabID, out _instanceID))
                        return true;
                }

                _span.Add(new PrefabInstanceData(_prefabID));
                _instanceID = 1;

                return true;
            }

            public bool Unregister(int _sceneID, ulong _prefabID, int _instanceID) {
                if (_sceneID != sceneID)
                    return false;

                ref List<PrefabInstanceData> _span = ref instanceData;

                for (int i = _span.Count; i-- > 0;) {
                    if (_span[i].Unregister(_prefabID, _instanceID))
                        return true;
                }

                return false;
            }

            public bool OnUnloadScene(int _sceneID) {
                if (_sceneID != sceneID)
                    return false;

                ref List<PrefabInstanceData> _span = ref instanceData;

                for (int i = _span.Count; i-- > 0;) {
                    _span[i].UpdatePendings();
                }

                return true;
            }
        }

        [Serializable]
        private class PrefabInstanceData {
            [SerializeField] private List<int> pendingIDs = new List<int>();
            [SerializeField] private ulong prefabID = 0;

            [SerializeField] private int instanceCounter = 0;
            [SerializeField] private int pendingCounter  = 0;
            [SerializeField] private int pendingCount    = 0;

            public PrefabInstanceData(ulong _prefabID) {
                prefabID = _prefabID;
                pendingIDs.Add(++instanceCounter);
            }

            public bool Register(ulong _prefabID, out int _instanceID) {
                if (_prefabID != prefabID) {
                    _instanceID = 0;
                    return false;
                }

                if (pendingCounter != 0) {
                    _instanceID = pendingIDs[pendingCount - (pendingCounter--)];
                    return true;
                }

                _instanceID = ++instanceCounter;
                pendingIDs.Add(_instanceID);
                return true;
            }

            public bool Unregister(ulong _prefabID, int _instanceID) {
                if (_prefabID != prefabID)
                    return false;

                pendingIDs.Remove(_instanceID);
                return true;
            }

            public void UpdatePendings() {
                int _count = pendingIDs.Count;
                pendingCounter = _count;
                pendingCount   = _count;
            }
        }
        #endregion

        /// <summary>
        /// Identifier for an <see cref="EnhancedObjectID"/> associated object type.
        /// </summary>
        public enum Type {
            // Unity.
            Null            = 0,
            ImportedAsset   = 1,
            SceneObject     = 2,
            SourceAsset     = 3,

            [Separator(SeparatorPosition.Top)]

            [Tooltip("Dynamically created object - most probably instantiated prefab instance")]
            DynamicObject   = 11,

            [Tooltip("Unknown object type")]
            Unknown         = 99,
        }

        #region Global Members
        [SerializeField] private string assetGUID   = string.Empty;
        [SerializeField] private ulong objectID     = 0L;
        [SerializeField] private ulong prefabID     = 0L;
        [SerializeField] private int instanceID     = 0;
        [SerializeField] private int assetHash      = 0;
        [SerializeField] private Type type          = Type.Null;

        // -----------------------

        /// <summary>
        /// Get this id associated asset (or scene) guid.
        /// </summary>
        #if JSON_SERIALIZATION
        [JsonIgnore]
        #endif
        public string AssetGUID {
            get { return assetGUID; }
        }

        /// <summary>
        /// Get this id associated asset (or scene) hashed guid.
        /// </summary>
        #if JSON_SERIALIZATION
        [JsonIgnore]
        #endif
        public int AssetHash {
            get { return assetHash; }
        }

        /// <summary>
        /// This id associated object instance id.
        /// </summary>
        #if JSON_SERIALIZATION
        [JsonIgnore]
        #endif
        public ulong ObjectID {
            get { return objectID; }
        }

        /// <summary>
        /// This id associated prefab id.
        /// </summary>
        #if JSON_SERIALIZATION
        [JsonIgnore]
        #endif
        public ulong PrefabID {
            get { return prefabID; }
        }

        /// <summary>
        /// This id associated dynamic instance id.
        /// </summary>
        #if JSON_SERIALIZATION
        [JsonIgnore]
        #endif
        public int InstanceID {
            get { return instanceID; }
        }

        /// <summary>
        /// Indicates if this id is valid.
        /// </summary>
        #if JSON_SERIALIZATION
        [JsonIgnore]
        #endif
        public bool IsValid {
            get { return (type != Type.Null) && (objectID != 0L); }
        }

        /// <summary>
        /// Is this object a static (non-instanciated) scene-bound object?
        /// </summary>
        #if JSON_SERIALIZATION
        [JsonIgnore]
        #endif
        public bool IsStaticSceneObject {
            get { return type == Type.SceneObject; }
        }

        /// <summary>
        /// Is this object a dynamic (runtime instanciated) scene-bound object?
        /// </summary>
        #if JSON_SERIALIZATION
        [JsonIgnore]
        #endif
        public bool IsDynamicSceneObject {
            get { return type == Type.DynamicObject; }
        }

        /// <summary>
        /// Is this object an initialized scene-bound object?
        /// </summary>
        #if JSON_SERIALIZATION
        [JsonIgnore]
        #endif
        public bool IsSceneObject {
            get {
                switch (type) {
                    case Type.SceneObject:
                    case Type.DynamicObject:
                        return true;

                    case Type.Null:
                    case Type.ImportedAsset:
                    case Type.SourceAsset:
                    case Type.Unknown:
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// This object type.
        /// </summary>
        #if JSON_SERIALIZATION
        [JsonIgnore]
        #endif
        public Type ObjectType {
            get { return type; }
        }

        // -----------------------

        /// <summary>
        /// Creates a new default empty <see cref="EnhancedObjectID"/>.
        /// </summary>
        public static EnhancedObjectID Default {
            get {
                return new EnhancedObjectID(Type.Null, 0L);
            }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <param name="_object"><see cref="Object"/> to create an id for.</param>
        /// <inheritdoc cref="EnhancedObjectID(Type, ulong)"/>
        public EnhancedObjectID(Object _object) {
            LoadObjectID(_object);
        }

        /// <inheritdoc cref="EnhancedObjectID(Type, ulong)"/>
        public EnhancedObjectID() : this(Type.DynamicObject, GenerateID()) { }

        /// <inheritdoc cref="EnhancedObjectID"/>
        private EnhancedObjectID(Type _type, ulong _objectID) {
            assetGUID  = string.Empty;
            assetHash  = 0;
            prefabID   = 0L;
            instanceID = 0;

            objectID = _objectID;
            type = _type;
        }
        #endregion

        #region Operator
        public static bool operator ==(EnhancedObjectID a, EnhancedObjectID b) {
            if (a is not null) {
                return a.Equals(b);
            }

            return b is null;
        }

        public static bool operator !=(EnhancedObjectID a, EnhancedObjectID b) {
            return !(a == b);
        }

        public override bool Equals(object _object) {
            if (_object is EnhancedObjectID _id) {
                return Equals(_id);
            }

            return base.Equals(_object);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override string ToString() {
            return $"{(int)type}-{assetGUID}-{objectID}.{prefabID}.{instanceID}";
        }
        #endregion

        #region Core
        internal static bool DynamicObjectStableID = false;
        internal static bool ConsoleLogs = false;

        private static readonly DynamicInstanceData dynamicData = new DynamicInstanceData();
        private static readonly EnhancedObjectID bufferID       = Default;

        // -----------------------

        /// <summary>
        /// Get the id for a given <see cref="ScriptableObject"/> instance.
        /// </summary>
        /// <param name="_object"><see cref="ScriptableObject"/> instance to load the associated id for.</param>
        /// <returns>True if a new id was successfully assigned, false otherwise.</returns>
        public bool GetID(ScriptableObject _object) {
            #if UNITY_EDITOR
            // Editor.
            if (!Application.isPlaying) {

                // Load from object.
                if (!LoadObjectID(_object, out EnhancedObjectID _id))
                    return false;

                // Ignore if invalid.
                if (!_id.IsValid || Equals(_id))
                    return false;

                #if UNITY_EDITOR
                if (ConsoleLogs) {
                    _object.LogMessage(_object.name + " - Update Scriptable ID =>      " + this + "     |||     " + _id);
                }
                #endif

                // Assign new value.
                Undo.RecordObject(_object, "Assigning ID");

                Copy(_id);
                EditorUtility.SetDirty(_object);

                return true;
            }
            #endif

            // Runtime assignement.
            if (!IsValid) {

                assetGUID  = string.Empty;
                assetHash  = 0;
                prefabID   = 0L;
                instanceID = 0;

                objectID = GenerateID();
                type = Type.DynamicObject;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the id for a given <see cref="Component"/> instance.
        /// </summary>
        /// <param name="_object"><see cref="Component"/> instance to load the associated id for.</param>
        /// <returns>True if a new id was successfully assigned, false otherwise.</returns>
        public bool GetID(Component _object) {
            #if UNITY_EDITOR
            // Editor.
            if (!Application.isPlaying) {

                // Load from object.
                if (!LoadObjectID(_object, out EnhancedObjectID _id))
                    return false;

                // Prefab objects may, when editing in prefab mode, be considered as "scene object" - which they are not.
                if (!_object.gameObject.scene.IsValid() || (StageUtility.GetCurrentStage() is PrefabStage)) {

                    if ((_id.type != Type.ImportedAsset) || string.IsNullOrEmpty(_id.assetGUID))
                        return false;
                }

                // Ignore if invalid.
                if (!_id.IsValid || EqualsSlow(_id))
                    return false;

                #if UNITY_EDITOR
                if (ConsoleLogs) {
                    _object.LogMessage(_object.name + " - update id => " + this + "   |||   " + _id);
                }
                #endif

                // Assign new value.
                Undo.RecordObject(_object, "Assigning ID");
                PrefabUtility.RecordPrefabInstancePropertyModifications(_object);

                Copy(_id);
                EditorUtility.SetDirty(_object);

                return true;
            }
            #endif

            // Runtime assignement.
            if (!IsValid) {

                assetGUID  = string.Empty;
                assetHash  = 0;
                prefabID   = 0L;
                instanceID = 0;

                objectID = GenerateID();
                type = Type.DynamicObject;

                return true;
            }

            // New asset (prefab) instance - increment associated instance id.
            switch (type) {

                case Type.ImportedAsset:
                case Type.SourceAsset:

                    type = Type.DynamicObject;
                    instanceID = DynamicObjectStableID ? dynamicData.Register(_object, objectID) : Guid.NewGuid().GetHashCode();

                    return true;

                case Type.Null:
                case Type.SceneObject:
                case Type.DynamicObject:
                case Type.Unknown:
                default:
                    break;
            }

            return false;
        }

        // -------------------------------------------
        // Callback(s)
        // -------------------------------------------

        /// <summary>
        /// Called whenever a <see cref="SceneBundle"/> is being unloaded.
        /// </summary>
        /// <param name="_sceneBundle"><see cref="SceneBundle"/> being unloaded.</param>
        internal static void OnUnloadSceneBundle(SceneBundle _sceneBundle) {
            dynamicData.OnUnloadSceneBundle(_sceneBundle);
        }

        /// <summary>
        /// Called whenever a given <see cref="Component"/> instance is being dynamically disabled (outside of being unloaded).
        /// </summary>
        /// <param name="_object"><see cref="Component"/> instance being disabled.</param>
        public void OnDisabled(Component _object) {
            if (!DynamicObjectStableID || !IsDynamicSceneObject)
                return;

            dynamicData.Unregister(_object, prefabID, instanceID);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        private static bool LoadObjectID(Object _object, out EnhancedObjectID _id) {
            _id = bufferID;
            return _id.LoadObjectID(_object);
        }

        private bool LoadObjectID(Object _object) {
            #if UNITY_EDITOR
            // Editor - get raw id.
            if (!Application.isPlaying) {
                GlobalObjectId _globalID = GlobalObjectId.GetGlobalObjectIdSlow(_object);
                string _guid = _globalID.assetGUID.ToString();

                assetGUID  = _guid;
                assetHash  = _guid.GetStableHashCode();
                objectID   = _globalID.targetObjectId;
                prefabID   = _globalID.targetPrefabId;
                instanceID = 0;

                type = (Type)_globalID.identifierType;
                return true;
            }
            #endif

            return false;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Copies the values of another <see cref="EnhancedObjectID"/>.
        /// </summary>
        /// <param name="_id"><see cref="EnhancedObjectID"/> to copy values.</param>
        public void Copy(EnhancedObjectID _id) {
            assetGUID  = _id.assetGUID;
            assetHash  = _id.assetHash;
            objectID   = _id.objectID;
            prefabID   = _id.prefabID;
            instanceID = _id.instanceID;
            type       = _id.type;
        }
        
        /// <summary>
        /// Get a simplified version of this object id.
        /// </summary>
        /// <returns>Object id as first, instance id as second.</returns>
        public Pair<ulong, int> GetSimplifiedID() {
            return new Pair<ulong, int>(objectID, instanceID);
        }

        /// <summary>
        /// Set this object instance id.
        /// </summary>
        /// <param name="_instanceID">New instance id to assign to this object.</param>
        public void SetInstanceID(int _instanceID) {
            instanceID = _instanceID;
        }

        /// <summary>
        /// Compares this id with another one.
        /// </summary>
        /// <param name="_id">ID to compare with this one.</param>
        /// <returns>True if both ids are equal, false otherwise.</returns>
        public bool Equals(EnhancedObjectID _id) {
            return (_id is not null) &&
                   (type      == _id.type) &&
                   objectID  .Equals(_id.objectID)   &&
                   prefabID  .Equals(_id.prefabID)   &&
                   instanceID.Equals(_id.instanceID) &&
                   assetHash .Equals(_id.assetHash);
        }

        /// <summary>
        /// Compares this id with another one, checking <see cref="string"/> content.
        /// </summary>
        /// <inheritdoc cref="Equals"/>
        public bool EqualsSlow(EnhancedObjectID _id) {
            return Equals(_id) && assetGUID.Equals(_id.assetGUID, StringComparison.Ordinal);
        }

        /// <summary>
        /// Compares this id with a simplified id.
        /// </summary>
        /// <param name="_objectID">Object id to compare with this one.</param>
        /// <param name="_instanceID">Instance id to compare with this one.</param>
        /// <returns>True if ids are equal, false otherwise.</returns>
        public bool EqualsSimplified(ulong _objectID, int _instanceID) {
            return objectID.Equals(_objectID) && _instanceID.Equals(_instanceID);
        }

        /// <summary>
        /// Get the <see cref="SceneAsset"/> associated with this id.
        /// </summary>
        /// <param name="_scene"><see cref="SceneAsset"/> associated with this id (null if none).</param>
        /// <returns>True if a scene could be found, false otherwise.</returns>
        public bool GetScene(out SceneAsset _scene) {
            ref string _guid = ref assetGUID;

            if (string.IsNullOrEmpty(_guid)) {
                _scene = null;
                return false;
            }

            _scene = new SceneAsset(_guid);
            return _scene.IsValid;
        }

        /// <param name="_scene">Buffer used to store this id associated scene <see cref="SceneAsset"/>.</param>
        /// <inheritdoc cref="GetScene(out SceneAsset)"/>
        public bool GetScene(SceneAsset _scene) {
            ref string _guid = ref assetGUID;

            if (string.IsNullOrEmpty(_guid)) {
                return false;
            }

            _scene.SetGUID(_guid);
            return _scene.IsValid;
        }

        /// <summary>
        /// Tries to parse a given <see cref="string"/> into an <see cref="EnhancedObjectID"/> instance.
        /// </summary>
        /// <param name="_id"><see cref="string"/> id representation to parse.</param>
        /// <param name="_objectID">Parsed id instance.</param>
        /// <returns>True if the parse operation could be successfully performed, false otherwise.</returns>
        public static bool TryParse(string _id, out EnhancedObjectID _objectID) {
            string[] _components = _id.Split('-');
            if ((_components.Length == 3) && int.TryParse(_components[0], out int _type)) {

                string[] _ids = _components[2].Split('.');
                if ((_ids.Length == 3) && ulong.TryParse(_ids[0], out ulong _objID) && ulong.TryParse(_ids[1], out ulong _prefabID) && int.TryParse(_ids[2], out int _instanceID)) {

                    string _guid = _components[1];

                    _objectID = new EnhancedObjectID() {
                        assetGUID  = _guid,
                        assetHash  = _guid.GetStableHashCode(),
                        objectID   = _objID,
                        prefabID   = _prefabID,
                        instanceID = _instanceID,
                        type       = (Type)_type,
                    };

                    return true;
                }
            }

            _objectID = null;
            return false;
        }

        /// <summary>
        /// Generates a new <see cref="ulong"/> id.
        /// </summary>
        /// <returns>New generated <see cref="ulong"/> id.</returns>
        public static ulong GenerateID() {
            unchecked {
                return (ulong)Guid.NewGuid().GetHashCode();
            }
        }
        #endregion
    }
}
