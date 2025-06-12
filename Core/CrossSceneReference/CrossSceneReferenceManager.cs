// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Singleton manager referencing all active <see cref="CrossSceneObject"/> in the loaded scene(s),
    /// <br/> and used to load the associated <see cref="CrossSceneReference{T}"/>.
    /// </summary>
    [ScriptGizmos(false, true)]
    [DefaultExecutionOrder(-900)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Cross Scene/Cross Scene Reference Manager"), DisallowMultipleComponent]
    public sealed class CrossSceneReferenceManager : EnhancedSingleton<CrossSceneReferenceManager> {
        #region Global Members
        [Section("Cross Scene Reference Manager")]

        [SerializeField, Enhanced, ReadOnly] private List<CrossSceneObject> references = new List<CrossSceneObject>();
        #endregion

        #region Registration
        /// <summary>
        /// Registers a new <see cref="CrossSceneObject"/>.
        /// </summary>
        /// <param name="_object">The new object reference to register.</param>
        internal void Register(CrossSceneObject _object) {
            references.Add(_object);
        }

        /// <summary>
        /// Unregisters a specific <see cref="CrossSceneObject"/>.
        /// </summary>
        /// <param name="_object">The object reference to unregister.</param>
        internal void Unregister(CrossSceneObject _object) {
            references.RemoveSwap(_object);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Retrieves a specific <see cref="CrossSceneObject"/> from its associated id.
        /// </summary>
        /// <param name="_id">ID of the object reference to find.</param>
        /// <param name="_reference">The <see cref="CrossSceneObject"/> associated with this id.</param>
        /// <returns>True if the associated <see cref="CrossSceneObject"/> could be found, false otherwise.</returns>
        public bool GetReference(EnhancedObjectID _id, out CrossSceneObject _reference) {

            ref List<CrossSceneObject> _span = ref references;

            for (int i = _span.Count; i-- > 0;) {

                _reference = _span[i];
                if (_reference.ID.Equals(_id)) {
                    return true;
                }
            }

            this.LogErrorMessage($"CrossScene reference not found ({references.Count})");

            _reference = null;
            return false;
        }
        #endregion
    }
}
