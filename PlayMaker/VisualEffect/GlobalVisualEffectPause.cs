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
    /// <see cref="FsmStateAction"/> used to pause or resume the visual effect global state.
    /// </summary>
    [Tooltip("Pauses/Resumes the VisualEffect global state.")]
    [ActionCategory(ActionCategory.Effects)]
    public sealed class GlobalVisualEffectPause : FsmStateAction {
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
            VisualEffectManager _manager = VisualEffectManager.Instance;

            wasPaused = _manager.IsPaused;
            _manager.Pause(Pause.Value);
        }

        private void Revert() {
            if (ResetOnExit.Value) {
                VisualEffectManager.Instance.Pause(wasPaused);
            }
        }
        #endregion
    }
}
