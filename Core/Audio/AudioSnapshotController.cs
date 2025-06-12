// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="AudioSnapshotAsset"/>-related <see cref="EnhancedBehaviour"/> controller.
    /// </summary>
    [AddComponentMenu(FrameworkUtility.MenuPath + "Audio/Snapshot Controller"), DisallowMultipleComponent]
    public sealed class AudioSnapshotController : AudioWeightControllerBehaviour {
        #region Global Members
        [Section("Audio Snapshot Controller"), PropertyOrder(0)]

        [Tooltip("Snapshot wrapped in this object")]
        [SerializeField, Enhanced, Required] private AudioSnapshotAsset snaphsot = null;

        [Space(5f)]

        [Tooltip("If true, overrides the default priority of this snaphsot")]
        [SerializeField] private bool overridePriority = false;

        [Tooltip("Priority of this snapshot. Only the snapshot with the highest priority is active")]
        [SerializeField, Enhanced, ShowIf(nameof(overridePriority)), Range(0f, 99f)] private int priority = 0;

        [Space(5f)]

        [Tooltip("If true, replaces all snapshot with the same priority")]
        [SerializeField] private bool replaceSnapshot = false;

        [Tooltip("If true, preserves this snapshot after exiting this trigger")]
        [SerializeField] private bool preserveOnExit = false;

        // -----------------------

        /// <summary>
        /// Priority of this snapshot. Only the snapshot with the highest priority is active.
        /// </summary>
        public int Priority {
            get { return overridePriority ? priority : snaphsot.Priority; }
        }
        #endregion

        #region Behaviour
        protected override void OnDeactivation() {
            base.OnDeactivation();

            if (!preserveOnExit) {
                AudioManager.Instance.PopSnapshot(snaphsot, false);
            }
        }

        // -----------------------

        protected override void SetWeight(float _weight) {
            base.SetWeight(_weight);

            if (replaceSnapshot) {
                AudioManager.Instance.ReplaceSnapshot(snaphsot, weight, Priority, false);
            } else {
                AudioManager.Instance.PushSnapshot(snaphsot, weight, Priority, false);
            }
        }
        #endregion
    }
}
