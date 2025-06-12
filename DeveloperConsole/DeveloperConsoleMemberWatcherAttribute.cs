// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using System;
using System.Diagnostics;

namespace EnhancedFramework.DeveloperConsoleSystem {
    /// <summary>
    /// Declares a static field or property so its value can be pinned and watched on screen, using the <see cref="DeveloperConsole"/>.
    /// </summary>
    [Conditional("DEVELOPER_CONSOLE")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DeveloperConsoleMemberWatcherAttribute : Attribute {
        #region Global Members
        /// <summary>
        /// Name used to reference this watcher.
        /// </summary>
        public readonly string Name         = string.Empty;

        /// <summary>
        /// Description of this watcher.
        /// </summary>
        public readonly string Description  = string.Empty;

        /// <summary>
        /// Whether this watcher starts being pinned and enabled or not.
        /// </summary>
        public readonly bool StartEnabled   = true;

        /// <summary>
        /// The order used to display this watcher.
        /// <br/> The higher the value, the higher on screen it will be displayed.
        /// </summary>
        public readonly int Order           = 0;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <param name="_name"><inheritdoc cref="Name" path="/summary"/></param>
        /// <param name="_description"><inheritdoc cref="Description" path="/summary"/></param>
        /// <param name="_startEnabled"><inheritdoc cref="StartEnabled" path="/summary"/></param>
        /// <param name="_order"><inheritdoc cref="Order" path="/summary"/></param>
        /// <inheritdoc cref="DeveloperConsoleCommandAttribute"/>
        public DeveloperConsoleMemberWatcherAttribute(string _name, string _description, bool _startEnabled = true, int _order = 0) {
            Name         = _name;
            Description  = _description;
            StartEnabled = _startEnabled;
            Order        = _order;
        }
        #endregion
    }
}
