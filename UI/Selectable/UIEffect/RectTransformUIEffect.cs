// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace EnhancedFramework.UI {
    /// <summary>
    /// Sets the position and size of a <see cref="RectTransform"/>.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(MenuPath + "RectTransform [UI Effect]")]
    public sealed class RectTransformUIEffect : EnhancedSelectableEffect {
        #region Global Members
        [Section("Rect Transform [UI Effect]")]

        [Tooltip("RectTransform instance to set the position and size")]
        [SerializeField, Enhanced, Required] private RectTransform source = null;

        [Tooltip("Target position and size of the RectTransform")]
        [SerializeField, Enhanced, Required] private RectTransform target = null;

        [Space(5f)]

        [Tooltip("Whether to set the RectTransform position and size or not")]
        [SerializeField] private EnumValues<SelectableState, bool> enable = new EnumValues<SelectableState, bool>();
        #endregion

        #region Behaviour
        private const int CornerBufferSize = 4;
        private static Vector3[] cornerBuffer = new Vector3[CornerBufferSize];

        // -----------------------

        public override void OnSelectionState(EnhancedSelectable _selectable, SelectableState _state, bool _instant) {

            // Ignore.
            if (!enable.GetValue(_state, out bool _enable) || !_enable) {
                return;
            }

            RectTransform _source = source;

            // Clockwise: Bottom-Left, Top-Left, Top-Right, Bottom-Right.
            ref Vector3[] _buffer = ref cornerBuffer;
            target.GetWorldCorners(_buffer);

            for (int i = 0; i < CornerBufferSize; i++) {
                _buffer[i] = _source.InverseTransformPoint(_buffer[i]);
            }

            Vector3 _size     = _buffer[2] - _buffer[0];
            Vector2 _position = _source.anchoredPosition + (Vector2)((_buffer[0] + (_size / 2f)));

            _source.anchoredPosition = _position;
            _source.sizeDelta = _size;
        }
        #endregion
    }
}
