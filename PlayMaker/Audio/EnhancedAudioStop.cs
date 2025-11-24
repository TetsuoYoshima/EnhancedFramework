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
    /// <see cref="FsmStateAction"/> used to stop playing an <see cref="AudioAsset"/>.
    /// </summary>
    [Tooltip("Stops an Audio Asset.")]
    [ActionCategory(ActionCategory.Audio)]
    public sealed class EnhancedAudioStop : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Audio
        // -------------------------------------------

        [Tooltip("The Audio Asset to stop.")]
        [RequiredField, ObjectType(typeof(AudioAsset))]
        public FsmObject Audio = null;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Audio = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            Stop();
            Finish();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void Stop() {
            if (Audio.Value is AudioAsset _audio) {
                _audio.Stop();
            }
        }
        #endregion
    }
}
