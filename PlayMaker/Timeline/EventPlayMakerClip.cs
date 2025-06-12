// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Timeline;
using System;
using System.ComponentModel;
using UnityEngine.Playables;

using DisplayName = System.ComponentModel.DisplayNameAttribute;

namespace EnhancedFramework.PlayMaker.Timeline {
    /// <summary>
    /// Sends an event in a <see cref="PlayMakerFSM"/> on play.
    /// </summary>
    [DisplayName("PlayMaker/Send Event")]
    public sealed class EventPlayMakerClip : PlayMakerPlayableAsset<EventPlayMakerBehaviour> {
        #region Utility
        public override string ClipDefaultName {
            get { return "PlayMaker Event"; }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="EventPlayMakerClip"/>-related <see cref="PlayableBehaviour"/>.
    /// </summary>
    [Serializable]
    public sealed class EventPlayMakerBehaviour : EnhancedPlayableBehaviour<PlayMakerFSM> {
        #region Global Members
        public string EventName = "Timeline Event";

        // -----------------------

        protected override bool CanExecuteInEditMode {
            get { return false; }
        }
        #endregion

        #region Behaviour
        protected override void OnPlay(Playable _playable, FrameData _info) {
            base.OnPlay(_playable, _info);

            PlayMakerFSM _fsm = bindingObject;
            if (_fsm != null) {
                _fsm.SendEvent(EventName);
            }
        }
        #endregion
    }
}
