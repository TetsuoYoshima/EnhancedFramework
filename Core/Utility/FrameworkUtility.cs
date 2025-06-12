// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using UnityEngine;

namespace EnhancedFramework {
    /// <summary>
    /// <see cref="EnhancedFramework"/>-related general utility class.
    /// </summary>
	public class FrameworkUtility {
        #region Content
        /// <summary>
        /// Menu item path used for <see cref="EnhancedFramework"/> utilities.
        /// </summary>
        public const string MenuItemPath    = "Tools/" + MenuPath;

        /// <summary>
        /// Menu path prefix used for creating new <see cref="ScriptableObject"/>, or any other special menu.
        /// </summary>
        public const string MenuPath        = Name + "/";

        /// <summary>
        /// Name of this plugin.
        /// </summary>
        public const string Name            = "Enhanced Framework";

        /// <summary>
        /// Menu order used for creating new <see cref="ScriptableObject"/> from the asset menu.
        /// </summary>
        public const int MenuOrder          = 200;
        #endregion
    }
}
