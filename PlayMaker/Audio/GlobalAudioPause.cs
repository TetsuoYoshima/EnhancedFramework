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
    /// <see cref="FsmStateAction"/> used to pause or resume the audio global state.
    /// </summary>
    [Tooltip("Pauses/Resumes the Audio global state.")]
    [ActionCategory(ActionCategory.Audio)]
    public sealed class GlobalAudioPause : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Pause - Reset
        // -------------------------------------------

        [Tooltip("Set to True to pause, False to resume.")]
        [RequiredField]
        public FsmBool Pause;

        [Tooltip("Reset the initial pause state when exiting the state.")]
        public FsmBool ResetOnExit;
        #endregion

        #region Behaviour
        private bool wasPaused = false;

        // -----------------------

        public override void Reset() {
            base.Reset();

            ResetOnExit = false;
            Pause       = true;
        }

        public override void OnEnter() {
            base.OnEnter();

            DoPause();
            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            Revert();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void DoPause() {
            wasPaused = AudioManager.IsPaused;
            AudioManager.Instance.Pause(Pause.Value);
        }

        private void Revert() {
            if (ResetOnExit.Value) {
                AudioManager.Instance.Pause(wasPaused);
            }
        }
        #endregion
    }
}
