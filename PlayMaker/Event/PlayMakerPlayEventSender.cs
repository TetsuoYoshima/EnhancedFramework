// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using UnityEngine;

namespace EnhancedFramework.PlayMaker {
    /// <summary>
    /// <see cref="EnhancedBehaviour"/> used to send the global PLAY event to a <see cref="PlayMakerFSM"/> instance.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "PlayMaker/Play Event Sender")]
    public sealed class PlayMakerPlayEventSender : EnhancedBehaviour {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Play;

        #region Global Members
        [Section("Play - Event Sender")]

        [SerializeField, Enhanced, Required] private PlayMakerFSM fsm = null;
        #endregion

        #region Enhanced Behaviour
        private const string PlayEventName = "PLAY";

        // -----------------------

        protected override void OnPlay() {
            base.OnPlay();
            fsm.SendEvent(PlayEventName);
        }
        #endregion
    }
}
