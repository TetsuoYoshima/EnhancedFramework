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
    /// Fades a <see cref="FadingGroup"/>.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(MenuPath + "Fading Group [UI Effect]"), DisallowMultipleComponent]
    public sealed class FadingGroupUIEffect : EnhancedSelectableEffect {
        #region Global Members
        [Section("Fading Group [UI Effect]")]

        [SerializeField, Enhanced, Required] private FadingObjectBehaviour group = null;

        [Space(5f)]

        [SerializeField] private EnumValues<SelectableState, FadingMode> fade = new EnumValues<SelectableState, FadingMode>(FadingMode.None);
        #endregion

        #region Behaviour
        public override void OnSelectionState(EnhancedSelectable _selectable, SelectableState _state, bool _instant) {
            group.Fade(fade[_state]);
        }
        #endregion
    }
}
