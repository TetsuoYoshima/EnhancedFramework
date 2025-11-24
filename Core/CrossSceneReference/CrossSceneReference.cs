// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

using Object     = UnityEngine.Object;
using SceneAsset = EnhancedEditor.SceneAsset;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="CrossSceneObject"/> reference wrapper.
    /// </summary>
    /// <typeparam name="T">The referencing object <see cref="Component"/> type to load.</typeparam>
    [Serializable]
    public sealed class CrossSceneReference<T> where T : Component {
        #region Global Members
        /// <summary>
        /// The id of the referenced <see cref="CrossSceneObject"/>.
        /// </summary>
        public EnhancedObjectID ReferenceID = EnhancedObjectID.Default;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="CrossSceneReference{T}"/>
        public CrossSceneReference() { }

        /// <param name="_reference">The referenced object instance (must not be null).</param>
        /// <inheritdoc cref="CrossSceneReference{T}"/>
        public CrossSceneReference(CrossSceneObject _reference) : this(_reference.ID) { }

        /// <param name="_id">Id of the object to reference.</param>
        /// <inheritdoc cref="CrossSceneReference{T}"/>
        public CrossSceneReference(EnhancedObjectID _id) {
            ReferenceID = _id;
        }
        #endregion

        #region Operator
        public static implicit operator T(CrossSceneReference<T> _reference) {
            return _reference.GetReference();
        }

        public static implicit operator CrossSceneReference<T>(T _component) {
            return new CrossSceneReference<T>(_component.gameObject.AddComponentIfNone<CrossSceneObject>());
        }
        #endregion

        #region Utility
        private static readonly SceneAsset sceneAssetBuffer = new SceneAsset();

        // -----------------------

        /// <summary>
        /// <inheritdoc cref="GetReference(out T)" path="/summary"/>
        /// </summary>
        /// <returns><inheritdoc cref="GetReference(out T)" path="/param[@name='_reference']"/></returns>
        /// <exception cref="MissingCrossCeneReferenceException"></exception>
        public T GetReference() {
            if (GetReference(out T _reference)) {
                return _reference;
            }

            throw new MissingCrossCeneReferenceException(ReferenceID.ToString());
        }

        /// <summary>
        /// Get this object reference <see cref="Component"/> instance.
        /// </summary>
        /// <param name="_reference">This class referencing <see cref="Component"/>.</param>
        /// <returns>True if the associated reference <see cref="Component"/> could be found, false otherwise.</returns>
        public bool GetReference(out T _reference) {
            if (ReferenceID.IsValid && GetReference(out CrossSceneObject _ref) && _ref.TryGetComponent(out _reference)) {
                return true;
            }

            _reference = null;
            return false;

            // ----- Local Method ----- \\

            bool GetReference(out CrossSceneObject _reference) {
                #if UNITY_EDITOR
                if (!Application.isPlaying) {
                    CrossSceneObject[] _objects = Object.FindObjectsByType<CrossSceneObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                    foreach (CrossSceneObject _object in _objects) {
                        if (_object.ID == ReferenceID) {
                            _reference = _object;
                            return true;
                        }
                    }

                    _reference = null;
                    return false;
                }
                #endif

                return CrossSceneReferenceManager.Instance.GetReference(ReferenceID, out _reference);
            }
        }

        /// <summary>
        /// Get the <see cref="SceneAsset"/> associated with this object.
        /// </summary>
        public bool GetScene(out SceneAsset _scene) {
            return ReferenceID.GetScene(out _scene);
        }

        /// <summary>
        /// Get the <see cref="SceneAsset"/> associated with this object, filling a buffer <see cref="SceneAsset"/>.
        /// </summary>
        public bool GetScene(SceneAsset _scene) {
            return ReferenceID.GetScene(_scene);
        }

        /// <summary>
        /// Get the <see cref="SceneBundle"/> associated with this cross scene reference.
        /// </summary>
        /// <param name="_bundle"><see cref="SceneBundle"/> of this object reference.</param>
        /// <returns>True if the associated bundle could be successfully found, false otherwise.</returns>
        public bool GetBundle(out SceneBundle _bundle) {

            SceneAsset _sceneBuffer = sceneAssetBuffer;

            if (!GetScene(_sceneBuffer)) {
                _bundle = null;
                return false;
            }
            
            return BuildSceneDatabase.Database.GetSceneBundle(_sceneBuffer, out _bundle);
        }

        /// <inheritdoc cref="IsLoaded(out SceneBundle, out T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLoaded(out SceneBundle _bundle) {
            return IsLoaded(out _bundle, out _);
        }

        /// <summary>
        /// Get if this cross scene reference source scene is currently loaded.
        /// </summary>
        /// <param name="_bundle"><see cref="SceneBundle"/> of this object reference.</param>
        /// <param name="_reference">This class referencing <see cref="Component"/>.</param>
        /// <returns>True if this object associated scene is loaded, false otherwise.</returns>
        public bool IsLoaded(out SceneBundle _bundle, out T _reference) {

            if (!GetBundle(out _bundle) || !_bundle.IsLoaded) {
                _reference = null;
                return false;
            }

            return GetReference(out _reference);
        }
        #endregion
    }

    /// <summary>
    /// Exception raised when a <see cref="CrossSceneReference{T}"/> could not be retrieved.
    /// </summary>
    public sealed class MissingCrossCeneReferenceException : Exception {
        #region Global Members
        public const string MessageFormat = "Could not retrieve the cross scene reference with GUID \'{0}\'";

        // -----------------------

        /// <inheritdoc cref="MissingCrossCeneReferenceException(string, Exception)"/>
        public MissingCrossCeneReferenceException() : base(string.Format(MessageFormat, "[Uknown]")) { }

        /// <inheritdoc cref="MissingCrossCeneReferenceException(string, Exception)"/>
        public MissingCrossCeneReferenceException(string _guid) : base(string.Format(MessageFormat, _guid)) { }

        /// <param name="_guid">Guid of the missing cross scene object.</param>
        /// <inheritdoc cref="MissingCrossCeneReferenceException"/>
        public MissingCrossCeneReferenceException(string _guid, Exception _innerException) : base(string.Format(MessageFormat, _guid), _innerException) { }
        #endregion
    }
}
