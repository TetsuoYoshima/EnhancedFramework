// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System.Diagnostics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedFramework.Core {
    /// <summary>
    /// Base class to derive enhanced <see cref="ScriptableObject"/> from.
    /// <para/> Provides an <see cref="EnhancedObjectID"/> for all its instances.
    /// </summary>
    public abstract class EnhancedScriptableObject : ScriptableObject {
        #region Global Members
        [SerializeField, HideInInspector] private EnhancedObjectID objectID = EnhancedObjectID.Default;

        /// <summary>
        /// The unique identifier of this object.
        /// </summary>
        public EnhancedObjectID ID {
            get { return objectID; }
        }
        #endregion

        #region Operator
        public static implicit operator EnhancedObjectID(EnhancedScriptableObject _scriptable) {
            return _scriptable.ID;
        }

        public override bool Equals(object _object) {
            if (_object is EnhancedScriptableObject _scriptable) {
                return Equals(_scriptable);
            }

            return base.Equals(_object);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
        #endregion

        #region Enhanced Behaviour
        protected virtual void OnEnable() {
            #if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode) {
                return;
            }
            #endif

            // Assign ID for new instantiated objects.
            GetObjectID();
        }

        // -------------------------------------------
        // Editor
        // -------------------------------------------

        [Conditional("UNITY_EDITOR")]
        protected virtual void OnValidate() {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                GetObjectID();
            }
            #endif
        }
        #endregion

        #region Object ID
        /// <summary>
        /// Get this object unique ID.
        /// </summary>
        [ContextMenu("Get Object ID", false, 10)]
        private void GetObjectID() {

            #if UNITY_EDITOR
            // Editor behaviour.
            if (!Application.isPlaying) {

                EnhancedObjectID _objectID = new EnhancedObjectID(this);
                if (objectID.Equals(_objectID)) {
                    return;
                }

                Undo.RecordObject(this, "Assigning ID");

                objectID = new EnhancedObjectID(this);
                EditorUtility.SetDirty(this);

                return;
            }
            #endif

            // Runtime assignement.
            if (!objectID.IsValid) {
                objectID = new EnhancedObjectID(this);
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Compare two <see cref="EnhancedScriptableObject"/> instances.
        /// </summary>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(EnhancedScriptableObject _other) {
            return _other.IsValid() && ID.Equals(_other.ID);
        }
        #endregion
    }
}
