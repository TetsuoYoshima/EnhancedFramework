// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using UnityEngine;

namespace EnhancedFramework.UI {
    /// <summary>
    /// Ready-to-use <see cref="EnhancedBehaviour"/>-encapsulated <see cref="ScreenTransitionFadingGroup"/>.
    /// <br/> Use this to quickly implement fading <see cref="CanvasGroup"/> using <see cref="ScreenFadingGroup"/> transitions.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "UI/Fading Group/Screen Transition [Fading Group]"), DisallowMultipleComponent]
    public sealed class ScreenTransitionFadingGroupBehaviour : FadingObjectTransitionFadingGroupBehaviour<ScreenTransitionFadingGroup> { }
}
