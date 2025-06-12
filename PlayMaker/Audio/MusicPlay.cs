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
    /// <see cref="FsmStateAction"/> used to activate an <see cref="AudioMusicController"/>.
    /// </summary>
    [Tooltip("Plays a Music.")]
    [ActionCategory(ActionCategory.Audio)]
    public sealed class MusicPlay : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Music - Instant - Stop
        // -------------------------------------------

        [Tooltip("Music to play.")]
        [RequiredField, ObjectType(typeof(AudioMusicController))]
        public FsmObject Music;

        [Tooltip("If true, instantly play this Music.")]
        public FsmBool Instant;

        [Tooltip("Stops this Music when exiting the state.")]
        public FsmBool StopOnExit;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            StopOnExit = false;
            Instant    = false;
            Music      = null;
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
            if (Music.Value is AudioMusicController _music) {
                _music.Activate(Instant.Value);
            }
        }

        private void Deactivate() {
            if (StopOnExit.Value && (Music.Value is AudioMusicController _music)) {
                _music.Deactivate(Instant.Value);
            }
        }
        #endregion
    }
}
