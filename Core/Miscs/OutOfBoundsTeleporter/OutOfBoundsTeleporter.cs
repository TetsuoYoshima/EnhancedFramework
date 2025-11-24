// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Security trigger used to teleport any entering object to a specific <see cref="Transform"/> world position.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Utility/Out of Bounds Teleporter"), SelectionBase, DisallowMultipleComponent]
    public sealed class OutOfBoundsTeleporter : EnhancedBehaviour {
        #region Enhanced Behaviour
        [Section("Teleporter")]

        [Tooltip("Destination position and rotation of this teleporter")]
        [SerializeField, Enhanced, Required] private Transform destination = null;

        [Tooltip("Layers of all detected objects")]
        [SerializeField] private LayerMask detectionMask = new LayerMask();
        #endregion

        #region Trigger
        private void OnTriggerEnter(Collider _collider) {

            // Don't inherit from the ITrigger interface or EnhancedTrigger component
            // to avoid catching every target object without requiring to setup any script on it.
            if (_collider.isTrigger || !detectionMask.Contains(_collider.gameObject.layer)) {
                return;
            }

            // Teleport.
            Transform _destination = destination;
            _collider.transform.SetPositionAndRotation(_destination.position, _destination.rotation);

            if (_collider.TryGetComponent(out Rigidbody _rigidbody)) {
                _rigidbody.position = _destination.position;
                _rigidbody.rotation = _destination.rotation;
            }
        }
        #endregion
    }
}
