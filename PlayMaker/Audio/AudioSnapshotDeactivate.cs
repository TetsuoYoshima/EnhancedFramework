// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedFramework.Core;
using HutongGames.PlayMaker;
using UnityEngine;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.PlayMaker {
    /// <summary>
    /// <see cref="FsmStateAction"/> used to deactivate an <see cref="AudioSnapshotController"/>.
    /// </summary>
    [Tooltip("Deactivates an Audio Snapshot.")]
    [ActionCategory(ActionCategory.Audio)]
    public sealed class AudioSnapshotDeactivate : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Snapshot - Instant
        // -------------------------------------------

        [Tooltip("Audio Snapshot to deactivate.")]
        [RequiredField, ObjectType(typeof(AudioSnapshotController))]
        public FsmObject Snapshot;

        [Tooltip("If true, instantly deactivates this Snapshot.")]
        public FsmBool Instant;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Snapshot = null;
            Instant  = false;
        }

        public override void OnEnter() {
            base.OnEnter();

            Deactivate();
            Finish();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void Deactivate() {
            if (Snapshot.Value is AudioSnapshotController _snapshot) {
                _snapshot.Deactivate(Instant.Value);
            }
        }
        #endregion
    }
}
