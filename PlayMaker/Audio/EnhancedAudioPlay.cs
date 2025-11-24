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
    /// <see cref="FsmStateAction"/> used to play an <see cref="AudioAsset"/>.
    /// </summary>
    [Tooltip("Plays an Audio Asset.")]
    [ActionCategory(ActionCategory.Audio)]
    public sealed class EnhancedAudioPlay : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Audio - Settings - Options - Transform - Stop
        // -------------------------------------------

        [Tooltip("The Audio Asset to play.")]
        [RequiredField, ObjectType(typeof(AudioAsset))]
        public FsmObject Audio = null;

        [Tooltip("Override Settings used to play this audio.")]
        [ObjectType(typeof(AudioAssetSettings))]
        [HideIf(nameof(HideSettings))]
        public FsmObject Settings = null;

        [Tooltip("Whether to override the default Audio Settings or not.")]
        [RequiredField]
        public FsmBool OverrideSettings = null;

        [Tooltip("Options used to play these feedbacks.")]
        [RequiredField, ObjectType(typeof(FeedbackPlayOptions))]
        public FsmEnum Options = null;

        [Tooltip("Transform where to play this audio.")]
        [HideIf(nameof(HideTransform))]
        public FsmGameObject Transform;

        [Tooltip("If true, stops this audio when exiting this state.")]
        [RequiredField]
        public FsmBool StopOnExit;
        #endregion

        #region Behaviour
        private AudioHandler handler = default;

        // -----------------------

        public override void Reset() {
            base.Reset();

            OverrideSettings = false;
            StopOnExit       = false;
            Transform        = null;
            Settings         = null;
            Options          = FeedbackPlayOptions.None;
            Audio            = null;
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

            if (Audio.Value is not AudioAsset _audio) {
                return;
            }

            GameObject _object   = Transform.Value;
            Transform _transform = (_object != null) ? _object.transform : null;

            AudioAssetSettings _settings = OverrideSettings.Value ? (Settings.Value as AudioAssetSettings) : _audio.Settings;
            FeedbackPlayOptions _options = (FeedbackPlayOptions)Options.Value;

            // Play.
            switch (_options) {

                case FeedbackPlayOptions.PlayAtPosition:
                    handler = AudioManager.Instance.Play(_audio, _settings, _transform.position);
                    break;

                case FeedbackPlayOptions.FollowTransform:
                    handler = AudioManager.Instance.Play(_audio, _settings, _transform);
                    break;

                case FeedbackPlayOptions.None:
                default:
                    handler = AudioManager.Instance.Play(_audio, _settings);
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
        public bool HideSettings() {
            return !OverrideSettings.Value;
        }

        public bool HideTransform() {
            return (FeedbackPlayOptions)Options.Value == FeedbackPlayOptions.None;
        }
        #endregion
    }
}
