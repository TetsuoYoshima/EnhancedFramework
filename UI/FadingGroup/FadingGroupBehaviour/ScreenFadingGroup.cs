// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedFramework.UI {
    /// <summary>
    /// <see cref="ScreenFadingGroup"/>-related <see cref="FadingObjectTransitionFadingGroup"/>.
    /// </summary>
    [Serializable]
    public sealed class ScreenTransitionFadingGroup : FadingObjectTransitionFadingGroup {
        #region Global Members
        public override IFadingObject TransitionGroup {
            get { return ScreenFadingGroup.Instance; }
        }
        #endregion
    }

    // ===== Component ===== \\

    /// <summary>
    /// <see cref="EnhancedBehaviour"/> UI class used to make a screen fade. 
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "UI/Special/Screen Fade"), DisallowMultipleComponent]
    public sealed class ScreenFadingGroup : FadingObjectSingleton<ScreenFadingGroup> {
        #region Global Members
        [Space(10f)]

        [SerializeField, Enhanced, Required] private Image image = null;

        // -----------------------

        /// <summary>
        /// The <see cref="UnityEngine.UI.Image"/> used for fading.
        /// </summary>
        public Image Image {
            get { return image; }
        }
        #endregion

        #region Content
        /// <summary>
        /// Sets the color of this fading screen <see cref="UnityEngine.UI.Image"/>.
        /// </summary>
        /// <param name="_color">The new color of the image.</param>
        /// <returns>This <see cref="ScreenFadingGroup"/> instance, for chaining sequences.</returns>
        public ScreenFadingGroup SetColor(Color _color) {
            image.color = _color;
            return this;
        }
        #endregion
    }
}
