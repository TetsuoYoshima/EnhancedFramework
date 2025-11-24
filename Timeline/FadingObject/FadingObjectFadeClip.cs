// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using System.ComponentModel;

using DisplayName = System.ComponentModel.DisplayNameAttribute;

namespace EnhancedFramework.Timeline {
    /// <summary>
    /// Manages the visibility of a <see cref="FadingObjectBehaviour"/> for the duration of the clip.
    /// </summary>
    [DisplayName(NamePrefix + "Fade")]
    public sealed class FadingObjectFadeClip : FadingObjectFadePlayableAsset<FadingObjectFadePlayableBehaviour> {
        #region Utility
        public override string ClipDefaultName {
            get { return "Fading Object"; }
        }
        #endregion
    }
}
