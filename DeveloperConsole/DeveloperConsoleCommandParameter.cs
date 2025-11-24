// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;

namespace EnhancedFramework.DeveloperConsoleSystem {
    /// <summary>
    /// <see cref="DeveloperConsoleCommand"/>-related parameter wrapper, with some additional descriptions.
    /// </summary>
    public sealed class DeveloperConsoleCommandParameter {
        #region Global Members
        /// <summary>
        /// Name used to call this command.
        /// <code>Example: "Print"</code>
        /// </summary>
        public readonly string Name = string.Empty;

        /// <summary>
        /// Description of the command displayed to the user.
        /// <code>Example: "Displays a message in the developer console"</code>
        /// </summary>
        public string Description = string.Empty;

        /// <summary>
        /// This parameter associated <see cref="System.Type"/> value.
        /// </summary>
        public Type Type = null;

        /// <summary>
        /// A user-friendly version of this parameter <see cref="System.Type"/> name.
        /// </summary>
        public string FriendlyTypeName = string.Empty;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="DeveloperConsoleCommandParameter"/>
        /// <param name="_name"><inheritdoc cref="Name" path="/summary"/></param>
        /// <param name="_description"><inheritdoc cref="Description" path="/summary"/></param>
        public DeveloperConsoleCommandParameter(string _name, string _description) {
            Description = _description;
            Name        = _name;
        }
        #endregion

        #region Operator
        public static implicit operator Type(DeveloperConsoleCommandParameter _parameter) {
            return _parameter.Type;
        }
        #endregion

        #region String Conversation
        public override string ToString() {
            return $"{Name}({FriendlyTypeName})";
        }

        /// <summary>
        /// Get a nicely formatted <see cref="string"/> of this parameter value,
        /// <br/> used for logging in the developer console (uses rich text).
        /// </summary>
        /// <returns>This parameter formated <see cref="string"/> value.</returns>
        public string ToFormattedString() {
            return $"{Name.Bold()}({FriendlyTypeName.Italic()})";
        }
        #endregion

        #region Utility
        private const int MaxEnumValues = 7;

        // -----------------------

        /// <typeparam name="T"><inheritdoc cref="Type" path="/summary"/></typeparam>
        /// <inheritdoc cref="Setup(Type)"/>
        public void Setup<T>() {
            Setup(typeof(T));
        }

        /// <summary>
        /// Setups this parameter <see cref="System.Type"/> value.
        /// </summary>
        /// <param name="_type"><inheritdoc cref="Type" path="/summary"/></param>
        public void Setup(Type _type) {
            Type = _type;
            FriendlyTypeName = _type.GetFriendlyName();

            // Enum related additional description.
            if (_type.IsEnum) {
                string _enumHelper = string.Empty;
                Array _values = _type.GetEnumValues();

                // If too large, recommand using the associated help command.
                if (_values.Length > MaxEnumValues) {
                    _enumHelper = $"use <b>enum {_type.Name}</b> to see options";
                } else {
                    for (int i = 0; i < _values.Length; i++) {
                        var _value = _values.GetValue(i);
                        _enumHelper += $"{((i != 0) ? ", " : string.Empty)}{_value}={(int)_value}";
                    }
                }

                Description += $"{((string.IsNullOrEmpty(Description)) ? string.Empty : " ")}({_enumHelper})";
            }
        }
        #endregion
    }
}
