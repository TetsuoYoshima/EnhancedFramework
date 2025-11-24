// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;

namespace EnhancedFramework.UI {
    /// <summary>
    /// Ready-to-use <see cref="EnhancedBehaviour"/>-encapsulated <see cref="FadingGroup"/>.
    /// <br/> Use this to quickly implement instantly fading <see cref="CanvasGroup"/> objects.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "UI/Fading Group/Instant [Fading Group]"), DisallowMultipleComponent]
    public sealed class FadingGroupBehaviour : BaseFadingGroupBehaviour<InstantFadingGroup> { }
}
