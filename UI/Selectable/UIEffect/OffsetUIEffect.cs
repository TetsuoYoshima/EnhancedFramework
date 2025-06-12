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
    /// Set the offset of the button text.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(MenuPath + "Offset [UI Effect]"), DisallowMultipleComponent]
    public sealed class OffsetUIEffect : EnhancedSelectableEffect {
        #region Global Members
        [Section("Offset [UI Effect]")]

        [SerializeField, Enhanced, Required] private TextMeshProUGUI text = null;

        [Space(5f)]

        [SerializeField] private EnumValues<SelectableState, Vector2> offset = new EnumValues<SelectableState, Vector2>(Vector2.zero);
        #endregion

        #region Behaviour
        public override void OnSelectionState(EnhancedSelectable _selectable, SelectableState _state, bool _instant) {
            Vector2 _offset = offset[_state];
            Vector4 _margin = text.margin;

            text.margin = new Vector4(_offset.x, _offset.y, _margin.y, _margin.z);
        }
        #endregion
    }
}
#endif
