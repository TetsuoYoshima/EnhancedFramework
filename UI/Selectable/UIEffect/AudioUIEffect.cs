// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using UnityEngine;

namespace EnhancedFramework.UI {
    /// <summary>
    /// Plays an <see cref="AudioAsset"/>.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(MenuPath + "Audio [UI Effect]")]
    public sealed class AudioUIEffect : EnhancedSelectableEffect {
        #region Global Members
        [Section("Audio [UI Effect]")]

        [Tooltip("If true, don't play any sound when this button is the first selected")]
        [SerializeField] private bool ignoreFirstSelection = true;

        [Space(5f)]

        [SerializeField] private new EnumValues<SelectableState, AudioAsset> audio = new EnumValues<SelectableState, AudioAsset>();
        #endregion

        #region Behaviour
        private SelectableState lastState = SelectableState.Normal;

        // -----------------------

        public override void OnSelectionState(EnhancedSelectable _selectable, SelectableState _state, bool _instant) {

            // Ignore.
            if (lastState == _state) {
                return;
            }

            SelectableState _last = lastState;
            lastState = _state;

            if ((_state == SelectableState.Selected) && (_last == SelectableState.Pressed)) {
                return;
            }

            // Invalid.
            if (!audio.GetValue(_state, out AudioAsset _audio) || !_audio.IsValid()) {
                return;
            }

            // First selection.
            if ((_state == SelectableState.Selected) && ignoreFirstSelection && !SelectionUtility.IsSelection) {
                return;
            }

            // Play.
            _audio.PlayAudio();
        }
        #endregion
    }
}
