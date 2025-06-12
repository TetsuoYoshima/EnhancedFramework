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
    /// <see cref="FsmStateAction"/> used to stop playing an <see cref="VisualEffectAsset"/>.
    /// </summary>
    [Tooltip("Stops a Visuel Effect Asset.")]
    [ActionCategory(ActionCategory.Effects)]
    public sealed class EnhancedVisuelEffectStop : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Visuel Effect
        // -------------------------------------------

        [Tooltip("The Visuel Effect Asset to stop.")]
        [RequiredField, ObjectType(typeof(VisualEffectAsset))]
        public FsmObject Effect = null;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Effect = null;
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
            if (Effect.Value is VisualEffectAsset _effect) {
                _effect.Stop();
            }
        }
        #endregion
    }
}
