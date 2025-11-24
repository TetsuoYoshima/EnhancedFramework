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
    /// Ready-to-use <see cref="EnhancedBehaviour"/>-encapsulated <see cref="TransitionFadingGroup"/>.
    /// <br/> Use this to quickly implement fading <see cref="CanvasGroup"/> using another transition group.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "UI/Fading Group/Transition [Fading Group]"), DisallowMultipleComponent]
    public sealed class TransitionFadingGroupBehaviour : FadingObjectTransitionFadingGroupBehaviour<TransitionFadingGroup> { }
}
