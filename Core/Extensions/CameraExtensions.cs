// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Multiple extension methods related to the <see cref="Camera"/> class.
    /// </summary>
    public static class CameraExtensions {
        #region Content
        /// <summary>
        /// Get this <see cref="Camera"/>-related anchored position for a given world position.
        /// </summary>
        /// <param name="_camera">This reference <see cref="Camera"/>.</param>
        /// <param name="_anchor">UI anchor to get the relative anchored position from.</param>
        /// <param name="_worldPosition">World position to get the associated anchored position.</param>
        /// <returns>This world position <see cref="Camera"/>-related anchored position.</returns>
        public static Vector2 GetAnchoredPosition(this Camera _camera, RectTransform _anchor, Vector3 _worldPosition) {
            Vector3 viewportPosition = _camera.WorldToViewportPoint(_worldPosition);
            Vector2 canvasSize       = _anchor.sizeDelta;

            return new Vector2(viewportPosition.x * canvasSize.x, viewportPosition.y * canvasSize.y) - (canvasSize * .5f);
        }
        #endregion
    }
}
