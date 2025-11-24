// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.UI {
    /// <summary>
    /// Utility <see cref="Component"/> used to scroll on a panel.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "UI/Miscs/Scroll Wheel Panel"), DisallowMultipleComponent]
    public sealed class ScrollWheelPanel : EnhancedBehaviour, IScrollHandler {
        #region Global Members
        [Section("Scroll Wheel Panel")]

        [SerializeField, Enhanced, Required] private Scrollbar scroll = null;
        [SerializeField, Enhanced, Range(0f, 1f)] private float sensitivity = .1f;
        #endregion

        #region Enhanced Behaviour
        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        protected override void OnValidate() {
            base.OnValidate();

            // Reference initialization.
            if (!scroll) {
                scroll = transform.GetComponentInChildren<Scrollbar>();
            }
        }
        #endif
        #endregion

        #region Event
        void IScrollHandler.OnScroll(PointerEventData _data) {
            Scrollbar _scroll = scroll;

            _scroll.value = Mathf.Clamp(_scroll.value + (_scroll.size * sensitivity * _data.scrollDelta.y), 0f, 1f);
            _scroll.Rebuild(CanvasUpdate.Layout);
        }
        #endregion
    }
}
