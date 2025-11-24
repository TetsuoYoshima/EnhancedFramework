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
    /// Set the font material of the button text.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(MenuPath + "Font Material [UI Effect]"), DisallowMultipleComponent]
    public sealed class FontMaterialUIEffect : EnhancedSelectableEffect {
        #region Global Members
        [Section("Font Material [UI Effect]")]

        [SerializeField, Enhanced, Required] private TextMeshProUGUI text = null;

        [Space(5f)]

        [SerializeField] private EnumValues<SelectableState, Material> styles = new EnumValues<SelectableState, Material>(null);
        #endregion

        #region Behaviour
        public override void OnSelectionState(EnhancedSelectable _selectable, SelectableState _state, bool _instant) {
            text.fontMaterial = styles[_state];
        }
        #endregion
    }
}
#endif
