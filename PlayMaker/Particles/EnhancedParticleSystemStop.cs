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
    /// <see cref="FsmStateAction"/> used to stop playing an <see cref="ParticleSystemAsset"/>.
    /// </summary>
    [Tooltip("Stops a Particle System Asset.")]
    [ActionCategory(ActionCategory.Effects)]
    public sealed class EnhancedParticleSystemStop : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Particle
        // -------------------------------------------

        [Tooltip("The Particle System Asset to stop.")]
        [RequiredField, ObjectType(typeof(ParticleSystemAsset))]
        public FsmObject Particle = null;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Particle = null;
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
            if (Particle.Value is ParticleSystemAsset _particle) {
                _particle.Stop();
            }
        }
        #endregion
    }
}
