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
    /// <see cref="FsmStateAction"/> used to activate an <see cref="AudioAssetController"/>.
    /// </summary>
    [Tooltip("Activates an Audio Controller.")]
    [ActionCategory(ActionCategory.Audio)]
    public sealed class AudioControllerActivate : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Audio - Instant - Reset
        // -------------------------------------------

        [Tooltip("Audio Controller to activate.")]
        [RequiredField, ObjectType(typeof(AudioAssetController))]
        public FsmObject Audio;

        [Tooltip("If true, instantly activates this Controller.")]
        public FsmBool Instant;

        [Tooltip("Deactivates this Controller when exiting the state.")]
        public FsmBool ResetOnExit;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            ResetOnExit = false;
            Instant     = false;
            Audio       = null;
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
            if (Audio.Value is AudioAssetController _audio) {
                _audio.Activate(Instant.Value);
            }
        }

        private void Deactivate() {
            if (ResetOnExit.Value && (Audio.Value is AudioAssetController _audio)) {
                _audio.Deactivate(Instant.Value);
            }
        }
        #endregion
    }
}
