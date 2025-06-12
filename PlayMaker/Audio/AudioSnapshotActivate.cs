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
    /// <see cref="FsmStateAction"/> used to activate an <see cref="AudioSnapshotController"/>.
    /// </summary>
    [Tooltip("Activates an Audio Snapshot.")]
    [ActionCategory(ActionCategory.Audio)]
    public sealed class AudioSnapshotActivate : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Snapshot - Instant - Reset
        // -------------------------------------------

        [Tooltip("Audio Snapshot to activate.")]
        [RequiredField, ObjectType(typeof(AudioSnapshotController))]
        public FsmObject Snapshot;

        [Tooltip("If true, instantly activates this Snapshot.")]
        public FsmBool Instant;

        [Tooltip("Deactivates this Snapshot when exiting the state.")]
        public FsmBool ResetOnExit;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            ResetOnExit = false;
            Snapshot    = null;
            Instant     = false;
        }

        public override void OnEnter() {
            base.OnEnter();

            Activate();
            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            Deactivate();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void Activate() {
            if (Snapshot.Value is AudioSnapshotController _snapshot) {
                _snapshot.Activate(Instant.Value);
            }
        }

        private void Deactivate() {
            if (ResetOnExit.Value && (Snapshot.Value is AudioSnapshotController _snapshot)) {
                _snapshot.Deactivate(Instant.Value);
            }
        }
        #endregion
    }
}
