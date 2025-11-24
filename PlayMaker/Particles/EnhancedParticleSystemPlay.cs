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
    /// <see cref="FsmStateAction"/> used to play a <see cref="ParticleSystemAsset"/>.
    /// </summary>
    [Tooltip("Plays a Particle System Asset.")]
    [ActionCategory(ActionCategory.Effects)]
    public sealed class EnhancedParticleSystemPlay : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Particle - Options - Transform - Stop
        // -------------------------------------------

        [Tooltip("The Particle System Asset to play.")]
        [RequiredField, ObjectType(typeof(ParticleSystemAsset))]
        public FsmObject Particle = null;

        [Tooltip("Options used to play this particle.")]
        [RequiredField, ObjectType(typeof(FeedbackPlayOptions))]
        public FsmEnum Options = null;

        [Tooltip("Transform where to play this particle.")]
        [HideIf(nameof(HideTransform))]
        public FsmGameObject Transform;

        [Tooltip("If true, stops this particle when exiting this state.")]
        [RequiredField]
        public FsmBool StopOnExit;
        #endregion

        #region Behaviour
        private ParticleHandler handler = default;

        // -----------------------

        public override void Reset() {
            base.Reset();

            StopOnExit = false;
            Transform  = null;
            Particle   = null;
            Options    = FeedbackPlayOptions.None;
        }

        public override void OnEnter() {
            base.OnEnter();

            Play();
            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            Stop();
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void Play() {

            if (Particle.Value is not ParticleSystemAsset _particle) {
                return;
            }

            GameObject _object   = Transform.Value;
            Transform _transform = (_object != null) ? _object.transform : null;

            FeedbackPlayOptions _options = (FeedbackPlayOptions)Options.Value;

            // Play.
            switch (_options) {

                case FeedbackPlayOptions.PlayAtPosition:
                    handler = ParticleSystemManager.Instance.Play(_particle, _transform.position);
                    break;

                case FeedbackPlayOptions.FollowTransform:
                    handler = ParticleSystemManager.Instance.Play(_particle, _transform);
                    break;

                case FeedbackPlayOptions.None:
                default:
                    _particle.Play();
                    break;
            }
        }

        private void Stop() {
            if (!StopOnExit.Value) {
                return;
            }

            // Stop.
            handler.Stop();
        }
        #endregion

        #region Utility
        public bool HideTransform() {
            return (FeedbackPlayOptions)Options.Value == FeedbackPlayOptions.None;
        }
        #endregion
    }
}
