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
    /// <see cref="FsmStateAction"/> used to activate an <see cref="AudioAmbientController"/>.
    /// </summary>
    [Tooltip("Activates an Audio Ambient.")]
    [ActionCategory(ActionCategory.Audio)]
    public sealed class AudioAmbientActivate : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Ambient - Instant - Reset
        // -------------------------------------------

        [Tooltip("Audio Ambient to activate.")]
        [RequiredField, ObjectType(typeof(AudioAmbientController))]
        public FsmObject Ambient;

        [Tooltip("If true, instantly activates this Ambient.")]
        public FsmBool Instant;

        [Tooltip("Deactivates this Ambient when exiting the state.")]
        public FsmBool ResetOnExit;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            ResetOnExit = false;
            Ambient     = null;
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
            if (Ambient.Value is AudioAmbientController _ambient) {
                _ambient.Activate(Instant.Value);
            }
        }

        private void Deactivate() {
            if (ResetOnExit.Value && (Ambient.Value is AudioAmbientController _ambient)) {
                _ambient.Deactivate(Instant.Value);
            }
        }
        #endregion
    }
}
