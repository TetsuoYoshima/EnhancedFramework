// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

using Object = UnityEngine.Object;

namespace EnhancedFramework.DeveloperConsoleSystem {
    /// <summary>
    /// <see cref="DeveloperConsole"/>-related command wrapper, with all related callback informations.
    /// </summary>
    public sealed class DeveloperConsoleCommand : IComparer<DeveloperConsoleCommand> {
        #region Global Members
        public const char AliasSeparator = ',';

        // -----------------------

        /// <summary>
        /// Name used to call this command.
        /// <code>Example: "print"</code>
        /// </summary>
        public readonly string Name = string.Empty;

        /// <summary>
        /// Optional names that can be used for this command, separated by a comma.
        /// <code>Example: "write,log"</code>
        /// </summary>
        public readonly string[] Aliases = new string[0];

        /// <summary>
        /// Description of the command displayed to the user.
        /// <code>Example: "Display a message in the developer console"</code>
        /// </summary>
        public readonly string Description = string.Empty;

        /// <summary>
        /// Default parameterless callback to execute from this command.
        /// </summary>
        public readonly Action DefaultCallback      = null;

        /// <summary>
        /// Callback to execute from this command.
        /// </summary>
        public readonly Action<object[]> Callback   = null;

        /// <summary>
        /// Parameters informations for this command associated callback.
        /// </summary>
        public DeveloperConsoleCommandParameter[] Parameters = new DeveloperConsoleCommandParameter[0];

        /// <summary>
        /// Defines whether this command is part of the built-in commands or a user-defined one.
        /// </summary>
        public bool IsBuiltInCommand { get; private set; } = false;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Prevents from creating new instances using any constructor.
        /// </summary>
        private DeveloperConsoleCommand(string _name, string _aliases, string _description, Action _defaultCallback, Action<object[]> _callback,
                                        params DeveloperConsoleCommandParameter[] _parameters) {
            Name            = _name.RemoveWhitespace();
            Description     = _description;
            DefaultCallback = _defaultCallback;
            Callback        = _callback;
            Parameters      = _parameters;

            // Trim aliases.
            Aliases = _aliases.Split(AliasSeparator);

            ref string[] _span = ref Aliases;
            int _count = _span.Length;

            for (int i = _count; i-- > 0;) {
                string _alias = _span[i].RemoveWhitespace();

                if (!string.IsNullOrEmpty(_alias)) {
                    _span[i] = _alias;
                } else {
                    _span[i] = _span[--_count];
                }
            }

            if (_count != _span.Length) {
                Array.Resize(ref _span, _count);
            }
        }
        #endregion

        #region Creators
        /// <inheritdoc cref="Create{T1, T2, T3, T4, T5}(string, string, string, Action, Action{T1, T2, T3, T4, T5}, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter)"/>
        public static DeveloperConsoleCommand Create(string _name, string _aliases, string _description, Action _callback) {
            Action<object[]> _objectCallback = (p) => _callback.Invoke();
            DeveloperConsoleCommand _command = new DeveloperConsoleCommand(_name, _aliases, _description, _callback, _objectCallback);

            return _command;
        }

        /// <param name="_parameter">Callback parameter infos.</param>
        /// <inheritdoc cref="Create{T1, T2, T3, T4, T5}(string, string, string, Action, Action{T1, T2, T3, T4, T5}, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter)"/>
        public static DeveloperConsoleCommand Create<T>(string _name, string _aliases, string _description, Action _defaultCallback, Action<T> _callback,
                                                        DeveloperConsoleCommandParameter _parameter) {
            Action<object[]> _objectCallback = (p) => _callback.Invoke((T)p[0]);

            _parameter.Setup<T>();

            DeveloperConsoleCommand _command = new DeveloperConsoleCommand(_name, _aliases, _description, _defaultCallback, _objectCallback, _parameter);
            return _command;
        }

        /// <inheritdoc cref="Create{T1, T2, T3, T4, T5}(string, string, string, Action, Action{T1, T2, T3, T4, T5}, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter)"/>
        public static DeveloperConsoleCommand Create<T1, T2>(string _name, string _aliases, string _description, Action _defaultCallback, Action<T1, T2> _callback,
                                                             DeveloperConsoleCommandParameter _param1, DeveloperConsoleCommandParameter _param2) {
            Action<object[]> _objectCallback = (p) => _callback.Invoke((T1)p[0], (T2)p[1]);

            _param1.Setup<T1>();
            _param2.Setup<T2>();

            DeveloperConsoleCommand _command = new DeveloperConsoleCommand(_name, _aliases, _description, _defaultCallback, _objectCallback, _param1, _param2);
            return _command;
        }

        /// <inheritdoc cref="Create{T1, T2, T3, T4, T5}(string, string, string, Action, Action{T1, T2, T3, T4, T5}, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter)"/>
        public static DeveloperConsoleCommand Create<T1, T2, T3>(string _name, string _aliases, string _description, Action _defaultCallback, Action<T1, T2, T3> _callback,
                                                                 DeveloperConsoleCommandParameter _param1, DeveloperConsoleCommandParameter _param2, DeveloperConsoleCommandParameter _param3) {
            Action<object[]> _objectCallback = (p) => _callback.Invoke((T1)p[0], (T2)p[1], (T3)p[2]);

            _param1.Setup<T1>();
            _param2.Setup<T2>();
            _param3.Setup<T3>();

            DeveloperConsoleCommand _command = new DeveloperConsoleCommand(_name, _aliases, _description, _defaultCallback, _objectCallback, _param1, _param2, _param3);
            return _command;
        }

        /// <inheritdoc cref="Create{T1, T2, T3, T4, T5}(string, string, string, Action, Action{T1, T2, T3, T4, T5}, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter, DeveloperConsoleCommandParameter)"/>
        public static DeveloperConsoleCommand Create<T1, T2, T3, T4>(string _name, string _aliases, string _description, Action _defaultCallback,
                                                                     Action<T1, T2, T3, T4> _callback,
                                                                     DeveloperConsoleCommandParameter _param1, DeveloperConsoleCommandParameter _param2, DeveloperConsoleCommandParameter _param3, DeveloperConsoleCommandParameter _param4) {
            Action<object[]> _objectCallback = (p) => _callback.Invoke((T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3]);

            _param1.Setup<T1>();
            _param2.Setup<T2>();
            _param3.Setup<T3>();
            _param4.Setup<T4>();

            DeveloperConsoleCommand _command = new DeveloperConsoleCommand(_name, _aliases, _description, _defaultCallback, _objectCallback, _param1, _param2, _param3, _param4);
            return _command;
        }

        /// <summary>
        /// Creates a new <see cref="DeveloperConsoleCommand"/> with the provided informations.
        /// </summary>
        /// <param name="_name"><inheritdoc cref="Name" path="/summary"/></param>
        /// <param name="_aliases"><inheritdoc cref="Aliases" path="/summary"/></param>
        /// <param name="_description"><inheritdoc cref="Description" path="/summary"/></param>
        /// <param name="_defaultCallback">The default parameterless callback to execute from this command.</param>
        /// <param name="_callback">The callback to execute from this command.</param>
        /// <param name="_param1">Callback first parameter infos.</param>
        /// <param name="_param2">Callback second parameter infos.</param>
        /// <param name="_param3">Callback third parameter infos.</param>
        /// <param name="_param4">Callback fourth parameter infos.</param>
        /// <param name="_param5">Callback fifth parameter infos.</param>
        /// <returns>The newly created <see cref="DeveloperConsoleCommand"/> instance.</returns>
        public static DeveloperConsoleCommand Create<T1, T2, T3, T4, T5>(string _name, string _aliases, string _description, Action _defaultCallback,
                                                                         Action<T1, T2, T3, T4, T5> _callback,
                                                                         DeveloperConsoleCommandParameter _param1, DeveloperConsoleCommandParameter _param2, DeveloperConsoleCommandParameter _param3, DeveloperConsoleCommandParameter _param4, DeveloperConsoleCommandParameter _param5) {
            Action<object[]> _objectCallback = (p) => _callback.Invoke((T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3], (T5)p[4]);

            _param1.Setup<T1>();
            _param2.Setup<T2>();
            _param3.Setup<T3>();
            _param4.Setup<T4>();
            _param5.Setup<T5>();

            DeveloperConsoleCommand _command = new DeveloperConsoleCommand(_name, _aliases, _description, _defaultCallback, _objectCallback, _param1, _param2, _param3, _param4, _param5);
            return _command;
        }

        // -----------------------

        /// <summary>
        /// Creates a new <see cref="DeveloperConsoleCommand"/> from a specific method.
        /// </summary>
        /// <param name="_name"><inheritdoc cref="Name" path="/summary"/></param>
        /// <param name="_aliases"><inheritdoc cref="Aliases" path="/summary"/></param>
        /// <param name="_description"><inheritdoc cref="Description" path="/summary"/></param>
        /// <param name="_method">The method to call from this command.</param>
        /// <param name="_parameterDescriptions">The description of each parameters of the associated method.</param>
        /// <returns>The newly created <see cref="DeveloperConsoleCommand"/> instance.</returns>
        public static DeveloperConsoleCommand Create(string _name, string _aliases, string _description, MethodInfo _method, params string[] _parameterDescriptions) {
            ParameterInfo[] _infos = _method.GetParameters();
            DeveloperConsoleCommandParameter[] _parameters = new DeveloperConsoleCommandParameter[_infos.Length];

            for (int i = 0; i < _parameters.Length; i++) {
                string _paramDescription = (i < _parameterDescriptions.Length)
                                         ? _parameterDescriptions[i]
                                         : string.Empty;

                ParameterInfo _info = _infos[i];
                DeveloperConsoleCommandParameter _parameter = new DeveloperConsoleCommandParameter(_info.Name, _paramDescription);

                _parameter.Setup(_info.ParameterType);
                _parameters[i] = _parameter;
            }

            Action _defaultDelegate    = ()  => _method.Invoke(null, null);
            Action<object[]> _delegate = (p) => _method.Invoke(null, p);

            DeveloperConsoleCommand _command = new DeveloperConsoleCommand(_name, _aliases, _description, _defaultDelegate, _delegate, _parameters);
            return _command;
        }
        #endregion

        #region Operator
        public override bool Equals(object _object) {
            if ((_object != null) && (_object is DeveloperConsoleCommand _command)) {
                return (Name == _command.Name) && (Parameters.Length == _command.Parameters.Length);
            }

            return base.Equals(_object);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
        #endregion

        #region Comparer
        int IComparer<DeveloperConsoleCommand>.Compare(DeveloperConsoleCommand a, DeveloperConsoleCommand b) {
            return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        }
        #endregion

        #region String
        public override string ToString() {
            return Name;
        }

        /// <summary>
        /// Get the full command string of this value.
        /// </summary>
        /// <returns>This command full <see cref="string"/> value.</returns>
        public string ToCommandString() {
            string _string = Name;

            ref DeveloperConsoleCommandParameter[] _span = ref Parameters;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                _string += $" {_span[i]}";
            }

            return _string;
        }

        /// <summary>
        /// Get a nicely formatted <see cref="string"/> of this command value,
        /// <br/> used for logging in the developer console (uses rich text).
        /// </summary>
        /// <returns>This command formated <see cref="string"/> value.</returns>
        public string ToFormattedString() {
            string _string = Name.Bold();

            ref DeveloperConsoleCommandParameter[] _span = ref Parameters;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                _string += $" {_span[i].ToFormattedString()}";
            }

            return _string;
        }
        #endregion

        #region Utility
        private static readonly string logFormat = $"{$"{"DeveloperConsole".Bold().Size(12)}  {UnicodeEmoji.RightTriangle.Get()}".Color(SuperColor.Crimson)}  {{0}}";

        // -----------------------

        /// <summary>
        /// Executes this command with the provided parameters.
        /// </summary>
        /// <param name="_context">The context <see cref="Object"/> used for logging messages.</param>
        /// <param name="_parameters">Parameters used to execute this command (null if none).</param>
        /// <returns>True if this command could be fully executed, false otherwise.</returns>
        public bool Execute(Object _context, params object[] _parameters) {
            try {
                Callback.Invoke(_parameters);
                return true;
            } catch (Exception e) {
                _context.LogError(string.Format(logFormat, "Command callback threw an exception"));
                _context.LogException(e);
            }

            return false;
        }

        /// <summary>
        /// Executes the default behaviour of this command.
        /// </summary>
        /// <param name="_context">The context <see cref="Object"/> used for logging messages.</param>
        /// <returns>True if this command has a default behaviour, false otherwise.</returns>
        public bool ExecuteDefault(Object _context) {
            if (DefaultCallback != null) {
                try {
                    DefaultCallback.Invoke();
                } catch (Exception e) {
                    _context.LogError(string.Format(logFormat, "Command callback threw an exception"));
                    _context.LogException(e);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get if this command has a given alias name.
        /// </summary>
        /// <param name="_alias">the alias name to check.</param>
        /// <returns>True if this command does have the specified alias, false otherwise.</returns>
        public bool HasAlias(string _alias) {
            return ArrayUtility.Contains(Aliases, _alias);
        }

        /// <summary>
        /// Marks this command as built-in.
        /// </summary>
        internal void MarkAsBuiltIn() {
            IsBuiltInCommand = true;
        }
        #endregion
    }
}
