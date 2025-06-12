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
    /// <see cref="FsmStateAction"/> used to play a <see cref="VisualEffectAsset"/>.
    /// </summary>
    [Tooltip("Plays a Visuel Effect Asset.")]
    [ActionCategory(ActionCategory.Effects)]
    public sealed class EnhancedVisuelEffectPlay : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Visuel Effect - Options - Transform - Stop
        // -------------------------------------------

        [Tooltip("The Visuel Effect Asset to play.")]
        [RequiredField, ObjectType(typeof(VisualEffectAsset))]
        public FsmObject Effect = null;

        [Tooltip("Options used to play these effect.")]
        [RequiredField, ObjectType(typeof(FeedbackPlayOptions))]
        public FsmEnum Options = null;

        [Tooltip("Transform where to play this effect.")]
        [HideIf(nameof(HideTransform))]
        public FsmGameObject Transform;

        [Tooltip("If true, stops this effect when exiting this state.")]
        [RequiredField]
        public FsmBool StopOnExit;
        #endregion

        #region Behaviour
        private VisualEffectHandler handler = default;

        // -----------------------

        public override void Reset() {
            base.Reset();

            StopOnExit = false;
            Transform  = null;
            Options    = FeedbackPlayOptions.None;
            Effect     = null;
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

            if (Effect.Value is not VisualEffectAsset _effect) {
                return;
            }

            GameObject _object   = Transform.Value;
            Transform _transform = (_object != null) ? _object.transform : null;

            FeedbackPlayOptions _options = (FeedbackPlayOptions)Options.Value;

            // Play.
            switch (_options) {

                case FeedbackPlayOptions.PlayAtPosition:
                    handler = VisualEffectManager.Instance.Play(_effect, _transform.position);
                    break;

                case FeedbackPlayOptions.FollowTransform:
                    handler = VisualEffectManager.Instance.Play(_effect, _transform);
                    break;

                case FeedbackPlayOptions.None:
                default:
                    _effect.Play();
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
