// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if DOTWEEN_ENABLED
#define TWEENING
#endif

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;

#if TWEENING
using DG.Tweening;
#endif

namespace EnhancedFramework.UI {
    /// <summary>
    /// Ready-to-use <see cref="EnhancedBehaviour"/>-encapsulated <see cref="TweeningFadingGroup"/>.
    /// <br/> Use this to quickly implement fading <see cref="CanvasGroup"/> objects using tweening.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "UI/Fading Group/Tween [Fading Group]"), DisallowMultipleComponent]
    public sealed class TweeningFadingGroupBehaviour : BaseFadingGroupBehaviour<TweeningFadingGroup> {
        #region Behaviour
        #if TWEENING
        /// <inheritdoc cref="TweeningFadingGroup.Show(float, Ease, Action)"/>
        public void Show(float _duration, Ease _ease, Action<bool> _onComplete = null) {
            group.Show(_duration, _ease, _onComplete);
        }

        /// <inheritdoc cref="TweeningFadingGroup.Hide(float, Ease, Action)"/>
        public void Hide(float _duration, Ease _ease, Action<bool> _onComplete = null) {
            group.Hide(_duration, _ease, _onComplete);
        }
        #endif
        #endregion
    }
}
