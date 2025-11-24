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
    /// <see cref="EnhancedBehaviour"/> used to send the global INIT event to a <see cref="PlayMakerFSM"/> instance.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "PlayMaker/Init Event Sender")]
    public sealed class PlayMakerInitEventSender : EnhancedBehaviour {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init;

        #region Global Members
        [Section("Init - Event Sender")]

        [SerializeField, Enhanced, Required] private PlayMakerFSM fsm = null;
        #endregion

        #region Enhanced Behaviour
        private const string InitEventName = "INIT";

        // -----------------------

        protected override void OnInit() {
            base.OnInit();
            fsm.SendEvent(InitEventName);
        }
        #endregion
    }
}
