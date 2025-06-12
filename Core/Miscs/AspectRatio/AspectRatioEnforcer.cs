// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

using Min = EnhancedEditor.MinAttribute;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="EnhancedSingleton{T}"/> used to enforce a specific game aspect ratio.
    /// </summary>
    [ExecuteInEditMode]
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Camera/Aspect Ratio Enforcer"), DisallowMultipleComponent]
    public sealed class AspectRatioEnforcer : EnhancedSingleton<AspectRatioEnforcer> {
        #region Mask
        private class DisplayMask {
            public Rect Viewport = new Rect();
            public Rect Mask1    = new Rect();
            public Rect Mask2    = new Rect();

            // -----------------------

            public void SetLetterbox(float _viewportHeight, float _maskHeight, float _viewportIncet) {
                float _width = Screen.width;

                Mask1   .Set(0f, 0f, _width, _maskHeight);
                Mask2   .Set(0f, _maskHeight + _viewportHeight, _width, _maskHeight);
                Viewport.Set(0f, _viewportIncet / 2f, 1f, 1f - _viewportIncet);
            }

            public void SetPillarbox(float _viewportWidth, float _maskWidth, float _viewportIncet) {
                float _height = Screen.height;

                Mask1   .Set(0f, 0f, _maskWidth, _height);
                Mask2   .Set(_maskWidth + _viewportWidth, 0f, _maskWidth, _height);
                Viewport.Set(_viewportIncet / 2f, 0f, 1f - _viewportIncet, 1f);
            }

            public void ClearBox() {
                Mask1   .Set(0f, 0f, 0f, 0f);
                Mask2   .Set(0f, 0f, 0f, 0f);
                Viewport.Set(0f, 0f, 1f, 1f);
            }
        }
        #endregion

        #region Global Members
        [Section("Aspect Ratio Enforcer")]

        [SerializeField] private Camera[] cameras = new Camera[0];

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Min(0f)] private float aspectRatio = 16f / 9f;
        [SerializeField] private Color maskColor = Color.black;

        [Space(10f)]

        [SerializeField] private bool previewInEditMode = true;

        // -----------------------

        /// <summary>
        /// Whether aspect ratio preview is enabled in edit mode or not.
        /// </summary>
        public bool PreviewInEditMode {
            get { return previewInEditMode; }
            set { previewInEditMode = value; }
        }

        /// <summary>
        /// Current camera mask color.
        /// </summary>
        public Color MaskColor {
            get { return maskColor; }
            set { maskColor = value; }
        }

        /// <summary>
        /// Current camera aspect ratio.
        /// </summary>
        public float AspectRatio {
            get { return aspectRatio; }
            set { aspectRatio = Mathf.Max(value, 0f); }
        }

        /// <summary>
        /// Current screen resolution aspect ratio.
        /// </summary>
        public float ScreenRatio {
            get { return Screen.width / (float)Screen.height; }
        }

        private float ViewportInset {
            get { return 1f - (ScreenRatio / AspectRatio); }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Init.
            maskTexture = new Texture2D(1, 1);
            style = new GUIStyle();

            SetMaskColor(maskColor);
        }

        private void OnGUI() {

            if (previewInEditMode || Application.isPlaying) {

                if (aspectRatio > ScreenRatio) {

                    float _viewportHeight = (aspectRatio <= 0f) ? Screen.height : (Screen.width / aspectRatio);
                    float _maskHeight     = (Screen.height - _viewportHeight) / 2f;

                    mask.SetLetterbox(_viewportHeight, _maskHeight, ViewportInset);

                } else if (aspectRatio < ScreenRatio) {

                    float _viewportWidth = Screen.height * aspectRatio;
                    float _maskWidth     = (Screen.width - _viewportWidth) / 2f;

                    mask.SetPillarbox(_viewportWidth, _maskWidth, ViewportInset);

                } else {
                    mask.ClearBox();
                }
            } else {
                mask.ClearBox();
            }

            DrawMask();
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Reset state.
            ResetCamera();
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        protected override void OnValidate() {
            base.OnValidate();

            // References.
            if (cameras.Length == 0) {
                cameras = GetComponentsInChildren<Camera>();
            }
        }
        #endif
        #endregion

        #region Mask
        private static readonly DisplayMask mask = new DisplayMask();
        private static Texture2D maskTexture     = null;
        private static GUIStyle style            = null;

        private static Color cacheMaskColor = default;

        // -----------------------

        /// <summary>
        /// Draws the camera mask on screen.
        /// </summary>
        private void DrawMask() {

            if (cacheMaskColor != maskColor) {
                SetMaskColor(maskColor);
            }

            GUIStyle _style = style;
            _style.normal.background = maskTexture;

            GUI.Box(mask.Mask1, GUIContent.none, _style);
            GUI.Box(mask.Mask2, GUIContent.none, _style);

            UpdateCameraRect();
        }

        /// <summary>
        /// Set the curretn camera mask color.
        /// </summary>
        /// <param name="_color">New camera mask color.</param>
        private void SetMaskColor(Color _color) {
            cacheMaskColor = _color;

            maskTexture.SetPixel(0, 0, _color);
            maskTexture.Apply();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Resets all cameras state.
        /// </summary>
        private void ResetCamera() {
            mask.ClearBox();
            UpdateCameraRect();
        }

        /// <summary>
        /// Updates all cameras rect according to the mask viewport.
        /// </summary>
        private void UpdateCameraRect() {
            ref Camera[] _span = ref cameras;
            Rect _rect = mask.Viewport;

            for (int i = _span.Length; i-- > 0;) {
                _span[i].rect = _rect;
            }
        }
        #endregion
    }
}
