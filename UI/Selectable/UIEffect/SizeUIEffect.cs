// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if TEXT_MESH_PRO_PACKAGE
using EnhancedEditor;
using TMPro;
using UnityEngine;

namespace EnhancedFramework.UI {
    /// <summary>
    /// Set the size of the button text.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(MenuPath + "Size [UI Effect]"), DisallowMultipleComponent]
    public sealed class SizeUIEffect : EnhancedSelectableEffect {
        #region Global Members
        [Section("Size [UI Effect]")]

        [SerializeField, Enhanced, Required] private TextMeshProUGUI text = null;

        [Space(5f)]

        [SerializeField] private EnumValues<SelectableState, float> size = new EnumValues<SelectableState, float>(1f);
        #endregion

        #region Behaviour
        public override void OnSelectionState(EnhancedSelectable _selectable, SelectableState _state, bool _instant) {
            text.fontSize = size[_state];
        }
        #endregion
    }
}
#endif
