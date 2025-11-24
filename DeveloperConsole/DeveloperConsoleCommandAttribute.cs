// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using System;
using System.Diagnostics;
using System.Reflection;

namespace EnhancedFramework.DeveloperConsoleSystem {
    /// <summary>
    /// Creates a new command from the <see cref="DeveloperConsole"/> with the associated method,
    /// with the supplied additional informations.
    /// <para/>
    /// The method must be static, but can either be private or public and have any number of parameter
    /// (that can be entered from the console).
    /// </summary>
    [Conditional("DEVELOPER_CONSOLE")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class DeveloperConsoleCommandAttribute : Attribute {
        #region Global Members
        /// <summary>
        /// <inheritdoc cref="DeveloperConsoleCommand.Name" path="/summary"/>
        /// </summary>
        public readonly string Name = string.Empty;

        /// <summary>
        /// <inheritdoc cref="DeveloperConsoleCommand.Aliases" path="/summary"/>
        /// </summary>
        public readonly string Aliases = string.Empty;

        /// <summary>
        /// <inheritdoc cref="DeveloperConsoleCommand.Description" path="/summary"/>
        /// </summary>
        public readonly string Description = string.Empty;

        /// <summary>
        /// <inheritdoc cref="DeveloperConsoleCommand.Create(string, string, string, MethodInfo, string[])" path="/param[@name='_parameterDescriptions']"/>
        /// </summary>
        public readonly string[] ParameterDescriptions = new string[0];

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <param name="_name"><inheritdoc cref="Name" path="/summary"/></param>
        /// <param name="_aliases"><inheritdoc cref="Aliases" path="/summary"/></param>
        /// <param name="_description"><inheritdoc cref="Description" path="/summary"/></param>
        /// <param name="_parameterDescriptions"><inheritdoc cref="ParameterDescriptions" path="/summary"/></param>
        /// <inheritdoc cref="DeveloperConsoleCommandAttribute"/>
        public DeveloperConsoleCommandAttribute(string _name, string _aliases, string _description, params string[] _parameterDescriptions) {
            ParameterDescriptions = _parameterDescriptions;
            Description           = _description;
            Aliases               = _aliases;
            Name                  = _name;
        }
        #endregion
    }
}
