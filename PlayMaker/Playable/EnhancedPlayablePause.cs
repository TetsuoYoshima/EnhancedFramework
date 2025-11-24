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
    /// <see cref="FsmStateAction"/> used to pause a <see cref="EnhancedPlayablePlayer"/>.
    /// </summary>
    [Tooltip("Pauses an Enhanced Playable Player")]
    [ActionCategory("Playable")]
    public sealed class EnhancedPlayablePause : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable
        // -------------------------------------------

        [Tooltip("The Playable to pause.")]
        [RequiredField, ObjectType(typeof(EnhancedPlayablePlayer))]
        public FsmObject Playable = null;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Playable = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            Pause();
            Finish();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void Pause() {
            if (Playable.Value is EnhancedPlayablePlayer _playable) {
                _playable.Pause();
            }
        }
        #endregion
    }
}
