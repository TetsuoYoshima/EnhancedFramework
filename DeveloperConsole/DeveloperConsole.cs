// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
//  (Greatly) based on David's own Unity developer console.
//  Many thanks to you David, you rock!
//
//      https://github.com/DavidF-Dev/Unity-DeveloperConsole
//
// ================================================================================== //

#if ENABLE_INPUT_SYSTEM
#define NEW_INPUT_SYSTEM
#endif

#if TEXT_MESH_PRO_PACKAGE
#define TEXT_MESH_PRO
#endif

using EnhancedEditor;
using EnhancedFramework.Core;
using EnhancedFramework.Core.GameStates;
using EnhancedFramework.DeveloperConsoleSystem.GameStates;
using EnhancedFramework.Inputs;
using EnhancedFramework.UI;
using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if NEW_INPUT_SYSTEM
using UnityEngine.InputSystem;
using InputKey = UnityEngine.InputSystem.Key;
#else
using InputSystem = UnityEngine.Input;
using InputKey    = UnityEngine.KeyCode;
#endif

#if TEXT_MESH_PRO
using InputField = TMPro.TMP_InputField;
using Text       = TMPro.TextMeshProUGUI;
#else
using InputField = UnityEngine.UI.InputField;
using Text       = UnityEngine.UI.Text;
#endif

using Command   = EnhancedFramework.DeveloperConsoleSystem.DeveloperConsoleCommand;
using Debug     = UnityEngine.Debug;
using Enum      = System.Enum;

namespace EnhancedFramework.DeveloperConsoleSystem {
    /// <summary>
    /// <see cref="DeveloperConsole"/>-related binding, used to execute a command,
    /// expression or statement when pressing a specific key combination.
    /// </summary>
    [Serializable]
    public sealed class DeveloperConsoleBinding {
        #region Global Members
        /// <summary>
        /// The input command, expression or statement of this binding.
        /// </summary>
        public string Expression = string.Empty;

        /// <summary>
        /// All keys requiring to be pressed to execute this binding.
        /// </summary>
        public EnhancedCollection<InputKey> Keys = new EnhancedCollection<InputKey>();

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="DeveloperConsoleBinding(string, InputKey[])"/>
        public DeveloperConsoleBinding() { }

        /// <param name="_expression"><inheritdoc cref="Expression" path="/summary"/></param>
        /// <param name="_keys"><inheritdoc cref="Keys" path="/summary"/></param>
        /// <inheritdoc cref="DeveloperConsoleBinding"/>
        public DeveloperConsoleBinding(string _expression, params InputKey[] _keys) {
            Expression = _expression;
            Keys.AddRange(_keys);
        }
        #endregion

        #region Operator
        public override string ToString() {
            return $"{Expression} - {string.Join(" + ", Keys)}";
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get if this bindings was performed this frame.
        /// </summary>
        /// <param name="_expression">The input expression of this binding.</param>
        /// <returns>True if this binding was performed this frame, false otherwise.</returns>
        public bool Performed(out string _expression) {
            bool _performed = false;
            _expression = Expression;

            List<InputKey> _keySpan = Keys.collection;
            for (int i = _keySpan.Count; i-- > 0;) {

                InputKey _key = _keySpan[i];

                if (DeveloperConsole.GetKeyDown(_key)) {
                    _performed = true;
                    continue;
                }

                if (!DeveloperConsole.GetKey(_key)) {
                    return false;
                }
            }

            return _performed;
        }

        /// <summary>
        /// Get if this binding match all specified keys.
        /// </summary>
        /// <param name="_keys">Keys to match with this binding.</param>
        /// <returns>True if these keys match this binding, false otherwise.</returns>
        public bool Match(IList<InputKey> _keys) {

            int _keyCount = _keys.Count;
            if (_keyCount != Keys.Count) {
                return false;
            }

            for (int i = 0; i < _keyCount; i++) {

                InputKey _key = _keys[i];
                if (!Keys.Contains(_key)) {
                    return false;
                }
            }

            return true;
        }
        #endregion
    }

    /// <summary>
    /// Runtime developer console.
    /// </summary>
    [ScriptGizmos(false, true)]
    [ScriptingDefineSymbol("DEVELOPER_CONSOLE", "Developer Console")]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Developer Console/Developer Console"), DisallowMultipleComponent]
    #pragma warning disable
    public sealed class DeveloperConsole : EnhancedSingleton<DeveloperConsole>, IStableUpdate, IGameStateLifetimeCallback {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init | UpdateRegistration.Stable;

        #region Global Members
        [Section("Developer Console")]

        [SerializeField]
        private SerializedType<IDeveloperConsoleState> gameStateType = new SerializedType<IDeveloperConsoleState>(SerializedTypeConstraint.None,
                                                                                                                  typeof(DefaultDeveloperConsoleGameState));

        [Tooltip("Input action used to open/close the console")]
        [SerializeField, Enhanced, Required] public InputActionEnhancedAsset OpenConsoleInput = null;


        [Space(5f)]

        [SerializeField, Enhanced, Required] private FadingGroupBehaviour group = null;
        [SerializeField, Enhanced, Required] private ResizablePanel resizePanel = null;
        [SerializeField, Enhanced, Required] private DraggablePanel dragPanel   = null;

        [Space(10f)]

        [SerializeField, Enhanced, Required] private InputField inputField      = null;
        [SerializeField, Enhanced, Required] private Text suggestionText        = null;
        [SerializeField, Enhanced, Required] private Text hintText              = null;
        [SerializeField, Enhanced, Required] private ScrollRect scroll          = null;

        [Space(10f)]

        [SerializeField, Enhanced, Required] private Text logObjectPrefab       = null;
        [SerializeField, Enhanced, Required] private RectTransform logParent    = null;

        [Space(10f)]

        [SerializeField, Enhanced, Required] private FadingGroupBehaviour watcherGroup  = null;
        [SerializeField, Enhanced, Required] private Text watcherText                   = null;

        // -----------------------

        /// <summary>
        /// Whether the developer console is currently open or not.
        /// </summary>
        public static bool IsOpen {
            get { return gameState.IsActive(); }
        }

        /// <summary>
        /// The settings of the developer console.
        /// </summary>
        public static DeveloperConsoleSettings Settings {
            get { return DeveloperConsoleSettings.Settings; }
        }

        // -----------------------

        /// <inheritdoc cref="DeveloperConsoleSettings.DisplayedLogs"/>
        public static FlagLogType DisplayedLogs {
            get { return Settings.DisplayedLogs; }
            set {
                var _settings = Settings;

                _settings.DisplayedLogs = value;
                _settings.Save();
            }
        }

        /// <inheritdoc cref="DeveloperConsoleSettings.IncludedUsing"/>
        private Set<string> IncludedUsing {
            get { return Settings.IncludedUsing; }
            set {
                var _settings = Settings;

                _settings.IncludedUsing = value;
                _settings.Save();
            }
        }

        /// <inheritdoc cref="DeveloperConsoleSettings.EnableBindings"/>
        public static bool EnableBindings {
            get { return Settings.EnableBindings; }
            set {
                var _settings = Settings;

                _settings.EnableBindings = value;
                _settings.Save();
            }
        }

        /// <inheritdoc cref="DeveloperConsoleSettings.Bindings"/>
        public static EnhancedCollection<DeveloperConsoleBinding> Bindings {
            get { return Settings.Bindings; }
            set {
                var _settings = Settings;

                _settings.Bindings = value;
                _settings.Save();
            }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            OpenConsoleInput.Enable();
        }

        protected override void OnInit() {
            base.OnInit();

            // Console deactivation.
            #if !DEVELOPER_CONSOLE
            enabled = false;
            return;
            #endif

            // Initialization.
            RegisterAttributes();
            RegisterBuiltInParsers();

            InitBuiltInCommands();
            InitMonoEvaluator();
            InitPanel();

            Application.logMessageReceivedThreaded += OnLogMessageReceived;

            inputField.onValueChanged.AddListener(x => OnInputValueChanged(x));
            inputField.onValidateInput += OnValidateInput;

            InputField = string.Empty;
            logObjectPrefab.gameObject.SetActive(false);

            #if TEXT_MESH_PRO
            suggestionText.fontSize = inputField.pointSize;
            #endif

            Clear();
        }

        void IStableUpdate.Update() {
            // Binding commands.
            if (EnableBindings) {

                List<DeveloperConsoleBinding> _bindingSpan = Bindings.collection;

                for (int i = _bindingSpan.Count; i-- > 0;) {

                    DeveloperConsoleBinding _binding = _bindingSpan[i];
                    try {
                        if (_binding.Performed(out string _expression)) {
                            RunCommand(_expression);
                            break;
                        }
                    } catch (Exception e) {
                        this.LogErrorMessage($"Binding \"{_binding.Expression}\" execution threw an exception: {e.Message}");
                    }
                }
            }

            // Watchers.
            UpdateWatchers();

            // Console activation.
            if (OpenConsoleInput.IsValid() && OpenConsoleInput.Performed()) {
                if (IsOpen) {
                    Close();
                } else {
                    Open();
                }
            }

            // Opened console update.
            if (!IsOpen) {
                return;
            }

            // Log buffer.
            if (!string.IsNullOrEmpty(logBuffer)) {
                bool _resetScroll = scroll.verticalNormalizedPosition < .01f;

                ProcessLogText(ref logBuffer, logBuffer.Length);

                if (_resetScroll) {
                    ResetScroll();
                }
            }

            // Input field utility.
            if (inputField.isFocused) {

                // Command suggestion cycling.
                if (commandSuggestions.Count != 0) {

                    if (GetKeyDown(UpArrowKey)) {
                        CommandSuggestionIndex++;
                        ResetInputCaret();
                    } else if (GetKeyDown(DownArrowKey)) {
                        CommandSuggestionIndex--;
                        ResetInputCaret();
                    }
                }

                // Command history cycling.
                else if (commandHistory.Count != 0) {

                    if (GetKeyDown(UpArrowKey)) {
                        CommandHistoryIndex++;
                    } else if (GetKeyDown(DownArrowKey)) {
                        CommandHistoryIndex--;
                    }
                }
            }
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            OpenConsoleInput.Disable();
        }
        #endregion

        #region Command
        private const string DefaultKeyword = "default";
        private const string NullKeyword    = "null";
        private const string NoneKeyword    = "~";

        private const string StringSymbol   = "\"";
        private const char StringSymbolChar = '"';

        private const int CommandHistoryLimit = 10;

        private static readonly Set<Command> commands                       = new Set<Command>();
        private static readonly EnhancedCollection<string> commandHistory   = new EnhancedCollection<string>();

        private static readonly PairCollection<Type, Func<string, object>> parameterParsers = new PairCollection<Type, Func<string, object>>();

        private static string currentCommand        = string.Empty; // Name of the currently executing command.
        private static string previousCommand       = string.Empty; // Name of the last previous entered command.
        private static int commandHistoryIndex      = -1;           // Index of the currently displayed command from the history (-1 for none).

        /// <summary>
        /// Index of the currently displayed command from the history.
        /// </summary>
        public static int CommandHistoryIndex {
            get { return commandHistoryIndex; }
            set {
                value = Mathf.Clamp(value, -1, commandHistory.Count - 1);

                // If the index changed, display the associated command.
                if (value != commandHistoryIndex) {
                    commandHistoryIndex = value;

                    Instance.SuggestionText = (value == -1)
                                            ? string.Empty
                                            : commandHistory[value];
                }
            }
        }

        // -------------------------------------------
        // Execution
        // -------------------------------------------

        /// <inheritdoc cref="AddCommand(Command, bool)"/>
        public static void AddCommand(Command _command) {
            AddCommand(_command, false);
        }

        /// <summary>
        /// Registers and adds a new <see cref="Command"/> to the developer console.
        /// </summary>
        /// <param name="_command">The new command to add.</param>
        /// <param name="_isBuiltInCommand">Whether this command is a built-in command or a user-defined one.</param>
        [Conditional("DEVELOPER_CONSOLE")]
        private static void AddCommand(Command _command, bool _isBuiltInCommand = false) {
            string _name = _command.Name;

            // Wrong command handling.
            if (string.IsNullOrEmpty(_name)) {
                Debug.Log("Cannot register a command with an empty name");
                return;
            }

            if (GetCommand(_name, out Command _other) && (_other.Parameters.Length == _command.Parameters.Length)) {
                Debug.Log($"A command with the same name is already registered:\n{_command.ToFormattedString().Italic()}");
                return;
            }

            if (commands.Find((c) => c.HasAlias(_name), out _other) && (_other.Parameters.Length == _command.Parameters.Length)) {
                Debug.Log($"A command with the same name as alias is already registered:\n{_other.ToFormattedString().Italic()}");
                return;
            }

            // Registration.
            if (_isBuiltInCommand) {
                _command.MarkAsBuiltIn();
            }

            commands.Add(_command);
        }

        /// <param name="_name">Name of the command to remove.</param>
        /// <inheritdoc cref="RemoveCommand(Command)"/>
        [Conditional("DEVELOPER_CONSOLE")]
        public static void RemoveCommand(string _name) {
            if (commands.FindIndex(c => c.Name == _name, out int _index) && !commands[_index].IsBuiltInCommand) {
                commands.RemoveAt(_index);
            }
        }

        /// <summary>
        /// Unregisters and removes a specific <see cref="Command"/> from the developer console.
        /// </summary>
        /// <param name="_command">The registered command to remove.</param>
        [Conditional("DEVELOPER_CONSOLE")]
        public static void RemoveCommand(Command _command) {
            if (!_command.IsBuiltInCommand) {
                commands.Remove(_command);
            }
        }

        /// <summary>
        /// Runs a command from a given <see cref="string"/> input.
        /// </summary>
        /// <param name="_rawInput">The raw <see cref="string"/> input to read and run the command from.</param>
        [Conditional("DEVELOPER_CONSOLE")]
        public static void RunCommand(string _rawInput) {
            // Get the different inputs:
            //  • Command name, as first element
            //  • Raw parameters, for the remaining elements

            string _commandInput = GetRawInputCommand(_rawInput, out string[] _arguments);

            // Register in command history, even if it isn't valid.
            AddToCommandHistory(_commandInput, _rawInput);
            LogCommand(_rawInput);

            // If the command could not be found, try to evaluate it as a C# expression.
            if (!GetCommands(_commandInput, _arguments, out Command _command)) {

                if (!EvaluateInput(_rawInput, out object _, false)) {
                    LogError($"Could not find the specified command: \"{_commandInput}\"");
                }

                return;
            }

            // Default behaviour.
            if (_arguments.Length == 0) {
                if (!_command.ExecuteDefault(Instance.LogObject)) {
                    LogError($"The command {_command.ToFormattedString()} has no default behaviour");
                }

                return;
            }

            // Determine the actual parameters now that we know the expected parameters.
            CleanParameters(ref _arguments, _command.Parameters.Length);

            if (_arguments.Length != _command.Parameters.Length) {
                LogError($"Invalid number of parameter(s): {_arguments.Length} provided instead of {_command.Parameters.Length} - {_command.ToFormattedString()}");
                return;
            }

            // Get callback parameters.
            object[] _parameters = null;
            int _parameterCount = _command.Parameters.Length;

            if (_parameterCount != 0) {
                _parameters = new object[_parameterCount];

                for (int i = 0; i < _parameterCount; i++) {
                    string _argument = _arguments[i];
                    var _parameter = _command.Parameters[i];

                    // Try to convert the parameter input into the appropriate type
                    if (!ParseParameter(_argument, _parameter.Type, out _parameters[i])) {
                        LogError($"Invalid parameter type: \"{_argument}\" cannot be converted to \"{_parameter.Type.Name}\" - {_parameter.ToFormattedString()}");
                        return;
                    }
                }
            }

            // Execute the command.
            _command.Execute(Instance.LogObject, _parameters);
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get the first registered <see cref="Command"/> from its name.
        /// </summary>
        /// <param name="_name">The name of the command to get.</param>
        /// <param name="_command">The command associated with the given name (null if none).</param>
        /// <returns>True if the associated command could be successfully found, false otherwise.</returns>
        public static bool GetCommand(string _name, out Command _command) {
            return commands.Find(c => (c.Name == _name) || ArrayUtility.Contains(c.Aliases, _name), out _command);
        }

        /// <summary>
        /// Get the best matching registered <see cref="Command"/> from its name and parameters.
        /// </summary>
        /// <param name="_name">The name of the command to get.</param>
        /// <param name="_parameters">The parameters of the command get.</param>
        /// <param name="_command">The best command associated with the given name and parameters (null if none).</param>
        /// <returns>True if an associated command could be successfully found, false otherwise.</returns>
        public static bool GetCommands(string _name, string[] _parameters, out Command _command) {
            int _parameterCount = _parameters.Length;
            _command = null;

            for (int i = 0; i < commands.Count; i++) {

                Command _temp = commands[i];

                if ((_temp.Name == _name) || ArrayUtility.Contains(_temp.Aliases, _name)) {

                    int _tempCount = _temp.Parameters.Length;

                    // If the command has the same number of parameter, select it.
                    if (_tempCount == _parameterCount) {
                        _command = _temp;
                        return true;
                    }

                    // Select the command with the highest matching number of parameter.
                    if ((_command == null) || ((_tempCount < _parameterCount) && (_tempCount > _command.Parameters.Length))) {
                        _command = _temp;
                    }
                }
            }

            return _command != null;
        }

        /// <summary>
        /// Add a command to the command history given the name and user input.
        /// </summary>
        private static void AddToCommandHistory(string _command, string _rawInput) {
            previousCommand = currentCommand;
            currentCommand = _command;

            commandHistory.Insert(0, _rawInput);

            if (commandHistory.Count == CommandHistoryLimit) {
                commandHistory.RemoveLast();
            }

            CommandHistoryIndex = -1;
        }

        // -------------------------------------------
        // Parsing
        // -------------------------------------------

        /// <summary>
        /// Registers a new <see cref="string"/> parser method for a specific <see cref="Type"/>.
        /// <para/>
        /// Used to parse an input into the corresponding command argument type.
        /// </summary>
        /// <param name="_type">The target parsing <see cref="Type"/>.</param>
        /// <param name="_parser">Method used to parse a given <see cref="string"/> into this type.</param>
        /// <param name="_overrideExisting">If true, any existing parser for this type
        /// will be overriden by the newly supplied one.</param>
        [Conditional("DEVELOPER_CONSOLE")]
        public static void RegisterTypeParser(Type _type, Func<string, object> _parser, bool _overrideExisting = false) {
            if (_overrideExisting || !parameterParsers.ContainsKey(_type)) {
                parameterParsers.Set(_type, _parser);
            }
        }

        /// <summary>
        /// Get the raw command and arguments from a given <see cref="string"/> input.
        /// </summary>
        /// <param name="_rawInput">The input to get the command from.</param>
        /// <param name="_parameters">The raw arguments from this input.</param>
        /// <returns>The raw command name from this input.</returns>
        private static string GetRawInputCommand(string _rawInput, out string[] _parameters) {
            string[] _inputs = _rawInput.Split(' ');

            // Parameterless input.
            switch (_inputs.Length) {
                case 0:
                    _parameters = new string[0];
                    return string.Empty;

                case 1:
                    _parameters = new string[0];
                    return _inputs[0];

                default:
                    break;
            }

            List<string> _parametersList = new List<string>();

            // Parse parameters (note that strings may have spaces). 
            string _parameter = string.Empty;
            bool _isString = false;

            for (int i = 1; i < _inputs.Length; i++) {
                _parameter = _isString
                           ? $"{_parameter} {_inputs[i]}"
                           : _inputs[i];

                if (!_isString) {
                    if (_parameter.StartsWith(StringSymbol)) {
                        _isString = true;
                    }
                }

                if (_isString) {
                    if (!_parameter.EndsWith(StringSymbol)) {
                        continue;
                    }

                    _parameter = _parameter.Trim(StringSymbolChar);
                    _isString = false;
                }

                _parametersList.Add(_parameter);
                _parameter = string.Empty;
            }

            _parameters = _parametersList.ToArray();
            return _inputs[0];
        }

        /// <summary>
        /// Clean the given <see cref="string"/> raw parameters
        /// <br/> (the last parameter may be a params argument, or the parameter count may not fit).
        /// </summary>
        /// <param name="_parameters">The raw parameters array to modify.</param>
        /// <param name="_argumentCount">The total count of arguments from the associated command callback.</param>
        private static void CleanParameters(ref string[] _parameters, int _argumentCount) {
            if ((_argumentCount >= _parameters.Length) || (_argumentCount == 0)) {
                return;
            }

            // Callback last parameter may be a params argument.
            string _lastParameter = _parameters[_argumentCount - 1];
            for (int i = _argumentCount; i < _parameters.Length; i++) {
                _lastParameter += $" {_parameters[i]}";
            }

            _parameters[_argumentCount - 1] = _lastParameter;
            Array.Resize(ref _parameters, _argumentCount);
        }

        /// <summary>
        /// Parse a given parameter to the specified type.
        /// </summary>
        /// <param name="_parameter">The <see cref="string"/> parameter to parse.</param>
        /// <param name="_type">The target parameter <see cref="Type"/> to parse it into.</param>
        /// <param name="_parsedParameter">The parsed parameter object (null if failed).</param>
        /// <returns>True if this parameter could be successfully parsed, false otherwise.</returns>
        private static bool ParseParameter(string _parameter, Type _type, out object _parsedParameter) {
            try {
                // Class and nullables values.
                if ((_type.IsClass || (Nullable.GetUnderlyingType(_type) != null)) && ((_parameter == NoneKeyword) || (_parameter.ToLower() == NullKeyword))) {
                    _parsedParameter = null;
                    return true;
                }

                // Class and struct default values.
                if ((_type.IsValueType || _type.IsClass) && ((_parameter == NoneKeyword) || (_parameter.ToLower() == DefaultKeyword))) {
                    _parsedParameter = System.Activator.CreateInstance(_type);
                    return true;
                }

                // Check if a matching parse method has been registered.
                if (parameterParsers.TryGetValue(_type, out Func<string, object> parseFunc)) {
                    _parsedParameter = parseFunc(_parameter);
                    return true;
                }

                // Enum.
                if (_type.IsEnum) {
                    var _enum = Enum.Parse(_type, _parameter, true);

                    if ((_enum != null) || (int.TryParse(_parameter, out int _intEnum) && (_enum = Enum.ToObject(_type, _intEnum)) != null)) {
                        _parsedParameter = _enum;
                        return true;
                    }
                }

                // Generic parse.
                _parsedParameter = Convert.ChangeType(_parameter, _type);
                return true;
            } catch (Exception e) {
                LogError(e.Message);

                _parsedParameter = null;
                return false;
            }
        }

        // -------------------------------------------
        // Init
        // -------------------------------------------

        private static void RegisterBuiltInParsers() {
            // Booleans.
            RegisterTypeParser(typeof(bool), s => {
                // Allow booleans to be in the form of "0" and "1".
                if (int.TryParse(s, out int result)) {
                    if (result == 0) {
                        return false;
                    } else if (result == 1) {
                        return true;
                    }
                }

                return Convert.ChangeType(s, typeof(bool));
            });

            // Nullable.
            RegisterTypeParser(typeof(bool?), s => {
                // Allow null value, representing a toggle.
                if ((s.ToLower() == NullKeyword) || (s == NoneKeyword)) {
                    return null;
                }

                if (ParseParameter(s, typeof(bool), out object _parameter)) {
                    return _parameter;
                }

                return null;
            });

            // Color.
            RegisterTypeParser(typeof(Color), s => {
                if (ColorUtility.TryParseHtmlString(s, out Color _color)) {
                    return _color;
                }

                string[] _components = s.Split(',');
                int _length = Math.Min(4, _components.Length);

                try {
                    _color = Color.black;
                    for (int i = 0; i < _length; ++i) {
                        _color[i] = float.Parse(_components[i]);
                    }
                } catch {
                    _color = default;
                }

                return _color;
            });
        }

        private static void RegisterAttributes() {
            try {
                // Assembly.
                Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();
                const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

                foreach (var _assembly in _assemblies) {

                    // Some assemblies may throw an exception when iterating over their types.
                    try {
                        // Types.
                        Type[] _types = _assembly.GetTypes();

                        foreach (var _type in _types) {
                            // Commands.
                            MethodInfo[] _methods = _type.GetMethods(bindingFlags);
                            foreach (MethodInfo _method in _methods) {

                                DeveloperConsoleCommandAttribute _attribute = _method.GetCustomAttribute<DeveloperConsoleCommandAttribute>(false);
                                if (_attribute != null) {

                                    // Create command.
                                    AddCommand(Command.Create(_attribute.Name, _attribute.Aliases, _attribute.Description,
                                                              _method, _attribute.ParameterDescriptions), true);
                                }
                            }

                            // Watchers
                            foreach (FieldInfo _field in _type.GetFields(bindingFlags)) {

                                DeveloperConsoleMemberWatcherAttribute _attribute = _field.GetCustomAttribute<DeveloperConsoleMemberWatcherAttribute>();
                                if (_attribute != null) {

                                    string _name = _attribute.Name;
                                    if (string.IsNullOrEmpty(_name)) {
                                        _name = _field.Name;
                                    }

                                    watchers.Add(_name, new FieldWatcher(_field, _attribute.Description, _attribute.StartEnabled, _attribute.Order));
                                }
                            }

                            foreach (PropertyInfo _property in _type.GetProperties(bindingFlags)) {
                                DeveloperConsoleMemberWatcherAttribute _attribute = _property.GetCustomAttribute<DeveloperConsoleMemberWatcherAttribute>();
                                if (_attribute != null) {

                                    string _name = _attribute.Name;
                                    if (string.IsNullOrEmpty(_name)) {
                                        _name = _property.Name;
                                    }

                                    watchers.Add(_name, new PropertyWatcher(_property, _attribute.Description, _attribute.StartEnabled, _attribute.Order));
                                }
                            }
                        }
                    } catch (Exception e) {
                        // Security.
                        Instance.LogWarningMessage($"Assembly {_assembly.FullName} threw an exception: {e.Message}");
                    }
                }
            } catch (Exception e) {
                // Security.
                Instance.LogException(e);
            }

            watchers.Sort();
        }

        private void InitBuiltInCommands() {
            const string Separator = ", ";
            const string Indent    = "   ";
            const string List      = " • ";

            #region Console
            // Devconsole
            {
                AddBuiltInCommand(Command.Create(
                    "devconsole",
                    "console,developerconsole",
                    "Displays instructions on how to use the developer console",
                    () => {
                        if (GetCommand("commands", out Command _command)) {
                            Log($"Use {_command.ToFormattedString()} to display a list of all available commands");
                        }
                        if (GetCommand("help", out _command)) {
                            Log($"Use {_command.ToFormattedString()} to display information about a specific command");
                        }

                        Log("Use UP / DOWN to cycle through command history and suggested commands");
                        Log("Use TAB to autocomplete the current suggestion\n");
                        LogSeparator();
                    }
                ));
            }

            // Log
            {
                AddBuiltInCommand(Command.Create<string>(
                    "log",
                    "print",
                    "Logs a message to the developer console",
                    null,
                    s => Log(s),
                    new DeveloperConsoleCommandParameter("message", "Message to log")
                ));

                AddBuiltInCommand(Command.Create<string>(
                    "debug",
                    "debuglog",
                    "Logs a message to the Unity console",
                    null,
                    s => Instance.LogMessage(s),
                    new DeveloperConsoleCommandParameter("message", "Message to log")
                ));

                AddBuiltInCommand(Command.Create<FlagLogType>(
                   "displayed_logs",
                   "displayedlogs",
                   "Get/Set the displayed Unity logs in the developer console",
                   () => {
                       LogValue("Displayed logs", DisplayedLogs);
                   },
                   f => {
                       DisplayedLogs = f;
                       LogSuccess($"Successfully set the displayed Unity logs to: {f}");
                   },
                   new DeveloperConsoleCommandParameter("enabled", "Whether Unity logs should be displayed in the developer console (use \"NULL\" to toggle)")
               ));
            }

            // Reset
            {
                AddBuiltInCommand(Command.Create(
                    "reset",
                    "",
                    "Resets the position and size of the developer console",
                    ResetPosition
                ));
            }

            // Clear
            {
                AddBuiltInCommand(Command.Create(
                   "clear",
                   "",
                   "Clears the developer console logs and content",
                   Clear
                ));
            }

            // Close
            {
                AddBuiltInCommand(Command.Create(
                    "close",
                    "hide",
                    "Closes the developer console",
                    Close
                ));
            }

            // Log size
            {
                AddBuiltInCommand(Command.Create<int>(
                    "logsize",
                    "log_size",
                    "Get/Set the font size of the displayed logs",
                    // Get
                    () => LogValue("Log font size", LogTextSize, $" (Default: {logObjectPrefab.fontSize})"),
                    // Set
                    i => {
                        if (!Mathm.IsInRange(i, MinLogTextSize, MaxLogTextSize)) {
                            LogError($"Invalid font size: {i} - Must be between {MinLogTextSize} and {MaxLogTextSize}");
                            return;
                        }

                        float _previous = LogTextSize;
                        LogTextSize = i;

                        LogSuccess($"Successfully changed the log font size from {_previous} to {i}");
                    },
                    new DeveloperConsoleCommandParameter("fontSize", "New displayed logs font size")
                ));
            }

            // Help
            {
                AddBuiltInCommand(Command.Create<string>(
                    "help",
                    "info",
                    "Displays information about a specific command",
                    // Default
                    () => {
                        if (string.IsNullOrEmpty(previousCommand.Trim())) {
                            RunCommand($"help help");
                            return;
                        }

                        RunCommand($"help {previousCommand}");
                    },
                    // Specific
                    s => {
                        // Invalid command.
                        if (!GetCommand(s, out Command _command)) {
                            string _suffix = string.Empty;

                            if (GetCommand("commands", out _command)) {
                                _suffix = $" - Use {_command.ToFormattedString()} for a list of all commands";
                            }

                            LogError($"Unknown command name: \"{s}\"{_suffix}");
                            return;
                        }

                        LogSeparator(_command.Name);

                        // Information.
                        if (!string.IsNullOrEmpty(_command.Description)) {
                            Log($"{_command.Description}");
                        }

                        if ((_command.Aliases.Length != 0) && Array.Exists(_command.Aliases, a => !string.IsNullOrEmpty(a))) {
                            string[] _formattedAliases = Array.ConvertAll(_command.Aliases, a => a.Italic());
                            Log($"Aliases: {string.Join(Separator, _formattedAliases)}");
                        }

                        if (_command.Parameters.Length != 0) {
                            Log($"Syntax: {_command.ToFormattedString()}");
                        }

                        if (_command.Parameters.Length != 0) {
                            Log(string.Empty);

                            foreach (var _parameter in _command.Parameters) {
                                if (!string.IsNullOrEmpty(_parameter.Description)) {
                                    Log($"{List}{_parameter.Name.Bold()}: {_parameter.Description}");
                                }
                            }
                        }

                        LogSeparator();
                    },
                    new DeveloperConsoleCommandParameter("commandName", "Name of the command to get information about")
                ));
            }

            // Enum
            {
                AddBuiltInCommand(Command.Create<string>(
                    "enum",
                    "",
                    "Displays information about a specific enum",
                    null,
                    s => {
                        List<Type> _enums = GetEnum(s);

                        switch (_enums.Count) {
                            case 0:
                                LogError($"Could not find any enum with the name name: \"{s}\"");
                                return;

                            case 1:
                                break;

                            default:
                                LogWarning($"Multiple results found: {string.Join(Separator, _enums.ConvertAll(e => e.FullName))}");
                                return;
                        }

                        Type _enum = _enums.First();
                        string _flag = (_enum.GetCustomAttribute(typeof(FlagsAttribute)) != null) ? " [Flags]" : string.Empty;

                        LogSeparator($"{_enum.FullName} ({_enum.GetEnumUnderlyingType().Name}){_flag}");

                        string _enumValues = string.Empty;
                        Array _values = _enum.GetEnumValues();

                        for (int i = 0; i < _values.Length; i++) {
                            var _value = _values.GetValue(i);
                            Log($"{List}{_value} = {(int)_value}");
                        }

                        LogSeparator();
                    },
                    new DeveloperConsoleCommandParameter("enumName", "Name of the enum to get information about (case-sensitive)")
                ));
            }

            // Command
            {
                AddBuiltInCommand(Command.Create(
                    "commands",
                    "get_commands",
                    "Displays a sorted list of all available commands",
                    () => {
                        commands.Sort(new Comparison<Command>((a, b) => a.Name.CompareTo(b.Name)));

                        LogCollection("Commands", commands, List);
                        LogSeparator();
                    }
                ));

                AddBuiltInCommand(Command.Create(
                    "builtin_commands",
                    "builtincommands",
                    "Displays a sorted list of all available built-in commands",
                    () => {
                        List<Command> _builtInCommands = new List<Command>();

                        for (int i = 0; i < commands.Count; i++) {

                            Command _command = commands[i];

                            if (_command.IsBuiltInCommand) {
                                _builtInCommands.Add(_command);
                            }
                        }

                        _builtInCommands.Sort(new Comparison<Command>((a, b) => a.Name.CompareTo(b.Name)));

                        LogCollection("Built-in Commands", _builtInCommands, List);
                        LogSeparator();
                    }
                ));

                AddBuiltInCommand(Command.Create(
                    "custom_commands",
                    "customcommands",
                    "Displays a sorted list of all available custom command(s)",
                    () => {
                        List<Command> _customCommands = new List<Command>();

                        for (int i = 0; i < commands.Count; i++) {

                            Command _command = commands[i];

                            if (!_command.IsBuiltInCommand) {
                                _customCommands.Add(_command);
                            }
                        }

                        if (_customCommands.Count == 0) {
                            Log("There is no custom command defined");
                            return;
                        }

                        _customCommands.Sort(new Comparison<Command>((a, b) => a.Name.CompareTo(b.Name)));

                        LogCollection("Custom Commands", _customCommands, List);
                        LogSeparator();
                    }
                ));
            }
            #endregion

            #region Bindings
            // Bindings
            {
                AddBuiltInCommand(Command.Create(
                    "bindings",
                    "bindingslist,bindings_list,get_bindings,getbindings",
                    "Displays a list of all the key bindings",
                    () => {
                        if (Bindings.Count == 0) {
                            string _suffix = string.Empty;

                            if (GetCommand("bind", out Command _command)) {
                                _suffix = $" - Use {_command.ToFormattedString()} to register a new key binding";
                            }

                            Log($"No key binding registered{_suffix}");
                            return;
                        }

                        LogCollection("Key Bindings", Bindings, List);
                        Log($"Key bindings: {EnableBindings.ToString(BooleanStringParser.EnabledDisabled, false)}");
                        LogSeparator();
                    }
                ));
            }

            // Add
            {
                AddBuiltInCommand(Command.Create<string, string>(
                    "bind",
                    "addbinding,add_binding,registerbinding,register_binding",
                    "Adds a new expression key binding",
                    null,
                    (_expression, _keysInput) => {
                        List<InputKey> _keys = GetKeys(_keysInput);

                        if (_keys.Count == 0) {
                            LogError($"You must specify at least one valid key for this binding");
                            return;
                        }

                        if (Bindings.FindIndex(b => b.Match(_keys), out int _index)) {
                            LogError($"There is already a registered binding with the given keys: {Bindings[_index].Expression}");
                            return;
                        }

                        Bindings.Add(new DeveloperConsoleBinding(_expression, _keys.ToArray()));
                        SaveSettings();

                        LogSuccess($"Successfully registered a new key binding");
                    },
                    new DeveloperConsoleCommandParameter("Expression", "Expression, statement or command to execute when all keys are pressed"),
                    new DeveloperConsoleCommandParameter("Keys", "Keys to bind this expression to, separated by a coma \",\"")
                ));
            }

            // Remove
            {
                AddBuiltInCommand(Command.Create<string>(
                    "unbind",
                    "removebinding,remove_binding",
                    "Removes an existing key binding",
                    null,
                    _keysInput => {
                        List<InputKey> _keys = GetKeys(_keysInput);

                        if (!Bindings.FindIndex(b => b.Match(_keys), out int _index)) {
                            LogError($"Could not find any matching binding for the specified keys");
                            return;
                        }

                        string _expression = Bindings[_index].Expression;

                        Bindings.RemoveAt(_index);
                        SaveSettings();

                        LogSuccess($"Successfully removed the following key binding: {_expression}");
                    },
                    new DeveloperConsoleCommandParameter("Keys", "Keys of the binding to remove, separated by a coma \",\"")
                ));
            }

            // Set
            {
                AddBuiltInCommand(Command.Create<bool>(
                    "setbindings",
                    "set_bindings",
                    "Enables/Disables the developer console bindings",
                    () => {
                        Log($"The developer console bindings are currently {EnableBindings.ToString(BooleanStringParser.EnabledDisabled, false)}");
                    },
                    b => {
                        EnableBindings = b;
                        LogSuccess($"{b.ToString(BooleanStringParser.EnabledDisabled)} the developer console bindings");
                    },
                    new DeveloperConsoleCommandParameter("Enable", "Whether to enable or disable the developer console bindings")
                ));
            }
            #endregion

            #region Application
            // Quit
            {
                AddBuiltInCommand(Command.Create(
                     "quit",
                     "exit",
                     "Quits the player application",
                     () => {
                         if (Application.isEditor) {
                             LogError("Cannot quit the player application when running in the Editor");
                             return;
                         }

                         Application.Quit();
                     }
                 ));
            }

            // Versions
            {
                AddBuiltInCommand(Command.Create(
                     "appversion",
                     "applicationversion,app_version,application_version",
                     "Displays the application version number",
                     () => LogValue("Application version", Application.version)
                 ));

                AddBuiltInCommand(Command.Create(
                     "unityversion",
                     "unity_version",
                     "Displays the Unity engine version",
                     () => LogValue("Unity version", Application.unityVersion)
                 ));
            }

            // Path
            {
                AddBuiltInCommand(Command.Create(
                     "path",
                     "applicationpath,application_path",
                     "Displays the path of the application executable",
                     () => LogValue("Application path", AppDomain.CurrentDomain.BaseDirectory)
                 ));

                AddBuiltInCommand(Command.Create(
                    "datapath",
                    "data_path",
                    "Displays information about Unity data path",
                    () => {
                        LogSeparator("Data paths");
                        LogValue("Data path", Application.dataPath);
                        LogValue("Persistent data path", Application.persistentDataPath);
                        LogSeparator();
                    }
                ));
            }
            #endregion

            #region Screen
            // Resolution
            {
                AddBuiltInCommand(Command.Create(
                    "resolution",
                    "screenresolution,screen_resolution",
                    "Displays the current screen resolution",
                    () => LogValue("Screen resolution", Screen.currentResolution)
                ));

                AddBuiltInCommand(Command.Create(
                    "appsize",
                    "app_size,applicationsize,application_size",
                    "Displays the current size of the screen window (in pixels)",
                    () => LogValue("Application size", $"{Screen.width} x {Screen.height}")
                ));
            }

            // Full screen
            {
                AddBuiltInCommand(Command.Create<bool>(
                    "fullscreen",
                    "",
                    "Get/Set full-screen mode for the application",
                    () => LogValue("Full screen", Screen.fullScreen),
                    b => {
                        Screen.fullScreen = b;
                        LogSuccess($"{b.ToString(BooleanStringParser.EnabledDisabled)} fullscreen mode");
                    },
                    new DeveloperConsoleCommandParameter("enabled", "Whether to enable or disalbe full-screen mode for the application")
                ));

                AddBuiltInCommand(Command.Create<FullScreenMode>(
                    "fullscreen_mode",
                    "fullscreenmode",
                    $"Get/Set the display mode of the application",
                    () => LogValue("Full screen mode", Screen.fullScreenMode),
                    m => {
                        Screen.fullScreenMode = m;
                        LogSuccess($"Full screen mode set to {m}");
                    },
                    new DeveloperConsoleCommandParameter("mode", "Display mode of the application")
                ));
            }

            // Vsync
            {
                AddBuiltInCommand(Command.Create<int>(
                    "vsync",
                    "vsynccount,vsync_count",
                    "Get/Set the current VSync count",
                    () => LogValue("VSync count", QualitySettings.vSyncCount),
                    i => {
                        if ((i < 0) || (i > 4)) {
                            LogError($"Invalid VSync count value: \"{i}\" - Must be betwwen 0 and 4");
                            return;
                        }

                        QualitySettings.vSyncCount = i;
                        LogSuccess($"VSync count set to {i}.");
                    },
                    new DeveloperConsoleCommandParameter("vSyncCount", "The number of VSyncs that should pass between each frame (0, 1, 2, 3, or 4)")
                ));
            }

            // Frame rate
            {
                AddBuiltInCommand(Command.Create<int>(
                    "framerate",
                    "targetframerate,target_framerate",
                    "Get/Set the target frame rate of the application",
                    () => LogValue("Target frame rate", Application.targetFrameRate),
                    i => {
                        Application.targetFrameRate = i;
                        LogSuccess($"Target frame rate set to {i}");
                    },
                    new DeveloperConsoleCommandParameter("targetFrameRate", "Frame rate the application will try to render at")
                ));
            }
            #endregion

            #region Camera
            // Orthographic
            {
                AddBuiltInCommand(Command.Create<bool>(
                    "cam_ortho",
                    "camortho",
                    "Get/Set whether the main camera is in orthographic projection mode",
                    () => {
                        Camera _camera = Camera.main;

                        if (_camera == null) {
                            LogError("Could not find the main camera");
                            return;
                        }

                        LogValue("Orthographic", _camera.orthographic);
                    },
                    b => {
                        Camera _camera = Camera.main;

                        if (_camera == null) {
                            LogError("Could not find the main camera");
                            return;
                        }

                        _camera.orthographic = b;
                        LogSuccess($"{b.ToString(BooleanStringParser.EnabledDisabled)} orthographic mode on the main camera");
                    },
                    new DeveloperConsoleCommandParameter("enabled", "Whether the main camera is orthographic or perspective")
                ));
            }

            // FOV
            {
                AddBuiltInCommand(Command.Create<int>(
                    "cam_fov",
                    "camfov",
                    "Get/Set the main camera field of view",
                    () => {
                        Camera _camera = Camera.main;

                        if (_camera == null) {
                            LogError("Could not find the main camera");
                            return;
                        }

                        LogValue("Field of view", _camera.fieldOfView);
                    },
                    f => {
                        Camera _camera = Camera.main;

                        if (_camera == null) {
                            LogError("Could not find the main camera");
                            return;
                        }

                        _camera.fieldOfView = f;
                        LogSuccess($"Main camera field of view set to {f}");
                    },
                    new DeveloperConsoleCommandParameter("fieldOfView", "Field of view")
                ));
            }
            #endregion

            #region Scene
            // Load
            {
                AddBuiltInCommand(Command.Create<string>(
                    "loadscene",
                    "",
                    "Load the scene by its name",
                    null,
                    s => {
                        SceneManager.LoadScene(s);
                        LogSuccess($"Tried to load scene {s}");
                    },
                    new DeveloperConsoleCommandParameter("sceneName", "Name of the scene to load")
                ));

                AddBuiltInCommand(Command.Create<int>(
                    "loadscene_index",
                    "loadsceneindex",
                    "Load the scene at the specified build index",
                    null,
                    i => {
                        if (!Mathm.IsInRange(i, 0, SceneManager.sceneCountInBuildSettings)) {
                            LogError($"Invalid scene build index: \"{i}\" - Must be between 0 and {SceneManager.sceneCountInBuildSettings - 1}");
                            return;
                        }

                        SceneManager.LoadScene(i);
                        LogSuccess($"Scene at build index {i} loaded");
                    },
                    new DeveloperConsoleCommandParameter("buildIndex", "Build index of the scene to load, from the Unity build settings")
                ));
            }

            // Info
            {
                AddBuiltInCommand(Command.Create<int>(
                    "scene_info",
                    "sceneinfo",
                    "Display information about the current scene(s)",
                    () => {
                        if (SceneManager.sceneCount == 0) {
                            Log("Could not find any active scenes");
                            return;
                        }

                        LogSeparator("Active scene(s)");
                        for (int i = 0; i < SceneManager.sceneCount; i++) {
                            Scene _scene = SceneManager.GetSceneAt(i);
                            Log($"{Indent}[{i}] {_scene.name} - Build index: {_scene.buildIndex}");
                        }

                        LogSeparator();
                    },
                    i => {
                        if (!Mathm.IsInRange(i, 0, SceneManager.sceneCount - 1)) {
                            LogError($"Could not find any active scene at index: {i}");
                            return;
                        }

                        Scene _scene = SceneManager.GetSceneAt(i);

                        LogSeparator(_scene.name);
                        Log($"Scene index: {i}");
                        Log($"Build index: {_scene.buildIndex}");
                        Log($"Path: {_scene.path}");
                        LogSeparator();
                    },
                    new DeveloperConsoleCommandParameter("sceneIndex", "Index of the scene to get, in the currently loaded scenes")
                ));
            }

            // Object info
            {
                AddBuiltInCommand(Command.Create<string>(
                    "object_info",
                    "objectinfo",
                    "Displays information about a GameObject in the scene",
                    null,
                    s => {
                        GameObject _object = GameObject.Find(s);

                        if (_object == null) {
                            LogError($"Could not find any GameObject with name: \"{s}\"");
                            return;
                        }

                        LogSeparator($"{_object.name} ({(_object.activeInHierarchy ? "enabled" : " disabled")})");

                        if (_object.TryGetComponent(out RectTransform _rect)) {
                            Log("RectTransform:");
                            LogValue($"{Indent}Anchored position", _rect.anchoredPosition);
                            LogValue($"{Indent}Size", _rect.sizeDelta);
                            LogValue($"{Indent}Pivot", _rect.pivot);
                        } else {
                            Log("Transform:");
                            LogValue($"{Indent}Position", _object.transform.position);
                            LogValue($"{Indent}Rotation", _object.transform.rotation);
                            LogValue($"{Indent}Scale", _object.transform.localScale);
                        }

                        LogValue("Tag", _object.tag);
                        LogValue("Physics layer", LayerMask.LayerToName(_object.layer));

                        if (_object.transform.TryGetComponent(out ExtendedBehaviour _behaviour)) {
                            var _tags = _behaviour.Tags.Tags;

                            Log($"Enhanced Tags [{_tags.Count}]");
                            LogCollection(string.Empty, _tags, List);
                        }

                        Component[] components = _object.GetComponents(typeof(Component));

                        // Ignore transform.
                        if (components.Length > 1) {
                            Log("Components:");

                            for (int i = 1; i < components.Length; i++) {
                                if (components[i] is MonoBehaviour _mono) {
                                    Log($"{Indent}[{i}] {_mono.GetType().Name} ({_mono.enabled.ToString(BooleanStringParser.EnabledDisabled, false)})");
                                } else {
                                    Log($"{Indent}[{i}] {components[i].GetType().Name}");
                                }
                            }
                        }

                        if (_object.transform.childCount > 0) {
                            Log("Children:");

                            for (int i = 0; i < _object.transform.childCount; i++) {
                                Transform _child = _object.transform.GetChild(i);
                                Log($"{Indent}[{i}] {_child.gameObject.name} ({_child.gameObject.activeInHierarchy.ToString(BooleanStringParser.EnabledDisabled, false)})");
                            }
                        }

                        LogSeparator();
                    },
                    new DeveloperConsoleCommandParameter("name", "Name of the GameObject")
                ));
            }

            // Object list
            {
                AddBuiltInCommand(Command.Create(
                    "object_list",
                    "objectlist",
                    "Displays a hierarchical list of all GameObject in the active scene",
                    () => {
                        GameObject[] _root = SceneManager.GetActiveScene().GetRootGameObjects();
                        string _log = string.Empty;
                        const int space = 2;

                        foreach (GameObject _rootObject in _root) {
                            _log += $"{List}{_rootObject.gameObject.name}\n";
                            LogChildren(_rootObject, space);
                        }

                        LogSeparator($"Hierarchy [{SceneManager.GetActiveScene().name}]");
                        Log(_log.TrimEnd('\n'));
                        LogSeparator();

                        // ----- Local Methods ----- \\

                        void LogChildren(GameObject obj, int _indentValue) {
                            for (int i = 0; i < obj.transform.childCount; i++) {
                                Transform _transform = obj.transform.GetChild(i);
                                _log += $"{GetIndent(_indentValue)}{_transform.gameObject.name}\n";

                                LogChildren(_transform.gameObject, _indentValue + 1);
                            }
                        }

                        string GetIndent(int _ind) {
                            string _value = string.Empty;
                            for (int i = 0; i < _ind; i++) {
                                _value += Indent;
                            }

                            return _value;
                        }
                    }
                ));
            }
            #endregion

            #region CSharp Evaluator
            // Evaluate
            {
                AddBuiltInCommand(Command.Create<string>(
                    "cs_evaluate",
                    "cs_eval,evaluate,eval",
                    "Evaluates a C# expression or statement, and displays the result",
                    null,
                    s => {
                        EvaluateInput(s, out object _);
                    },
                    new DeveloperConsoleCommandParameter("expression", "The expression to evaluate")
                ));
            }

            // Run
            {
                AddBuiltInCommand(Command.Create<string>(
                    "cs_run",
                    "run",
                    "Executes a C# expression or statement",
                    null,
                    s => {
                        RunInput(s);
                    },
                    new DeveloperConsoleCommandParameter("statement", "The statement to execute")
                ));
            }

            // Using
            {
                AddBuiltInCommand(Command.Create(
                    "cs_usings",
                    "cs_getusings",
                    "Displays a list of all active using statements",
                    () => {
                        if (!IsEvaluatorEnabled()) {
                            return;
                        }

                        LogCollection("Usings", IncludedUsing, List);
                        LogSeparator();
                    }
                ));

                AddBuiltInCommand(Command.Create<string, bool>(
                    "cs_setusing",
                    "cs_set_usings",
                    "Set whether a specific using statement should be automatically included in the developer console",
                    null,
                    (_usingName, _enabled) => {
                        Set<string> _usings = IncludedUsing;

                        if (_enabled) {
                            if (_usings.Contains(_usingName)) {
                                LogError($"The specifed using statement is already enabled: \"{_usingName}\"");
                                return;
                            }

                            _usings.Add(_usingName);
                            SaveSettings();

                            RunInput($"using {_usingName};", false);
                            LogSuccess($"Enabled \"{_usingName}\" as an automatically included using statement");
                        } else {
                            if (!_usings.Contains(_usingName)) {
                                LogError($"The specified using statement is already disabled: \"{_usingName}\"");
                                return;
                            }

                            _usings.Remove(_usingName);
                            SaveSettings();

                            LogSuccess($"Disabled \"{_usingName}\" as an automatically included using statement");
                        }

                        SaveSettings();
                    },
                    new DeveloperConsoleCommandParameter("namespace", "Namespace to use as a using statement (e.g. \"System.Collections\""),
                    new DeveloperConsoleCommandParameter("enabled", "Whether the using statement should be automatically included in the developer console")
                ));
            }

            // Variables
            {
                AddBuiltInCommand(Command.Create(
                    "cs_variables",
                    "cs_vars",
                    "Displays a list of all defined local variables",
                    () => {
                        if (!IsEvaluatorEnabled()) {
                            return;
                        }

                        string _variables = monoEvaluator.GetVars();

                        if (string.IsNullOrEmpty(_variables)) {
                            Log("There are no local variables defined");
                            return;
                        }

                        LogSeparator("Local Variables");
                        Log(_variables.TrimEnd('\n'));
                        LogSeparator();
                    }
                ));
            }

            // Help
            {
                AddBuiltInCommand(Command.Create(
                    "cs_help",
                    "",
                    "Displays information about the CSharp evaluator commands",
                    () => {
                        List<string> _commands = new List<string>() { "cs_evaluate", "cs_run", "cs_usings", "cs_setusing", "cs_vars" };
                        LogSeparator("CSharp Commands Help");

                        foreach (string _cmd in _commands) {
                            if (GetCommand(_cmd, out Command _command)) {
                                LogValue(_command.ToFormattedString(), _command.Description);
                            }
                        }

                        LogSeparator();
                    }
                ));

                AddBuiltInCommand(Command.Create(
                   "cs_commands",
                   "",
                   "Get a list of all CSharo evaluator-related commands",
                   () => {
                       List<Command> _commands = new List<Command>();

                       for (int i = 0; i < commands.Count; i++) {

                           Command _command = commands[i];
                           if (_command.Name.StartsWith("cs_")) {
                               _commands.Add(_command);
                           }
                       }

                       _commands.Sort(new Comparison<Command>((a, b) => a.Name.CompareTo(b.Name)));

                       LogCollection("Evaluator Commands", _commands, List);
                       LogSeparator();
                   }
               ));
            }
            #endregion

            #region Watchers
            // Help
            {
                AddBuiltInCommand(Command.Create(
                   "watch_help",
                   "watchershelp,watchers_help",
                   "Displays information about the developer console watchers feature",
                   () => {
                       LogSeparator("Watchers");
                       Log("When enabled, a watcher can be used to displayed a specific value on-screen during gameplay");
                       Log("There are multiple ways to register a watcher:\n");
                       Log($"1. Declaring a <i>{typeof(DeveloperConsoleMemberWatcherAttribute).Name}</i> attribute on a static field or property declaration");
                       Log($"2. Calling <i>DeveloperConsole.RegisterWatcher()</i> to register a new watcher using a provided lambda method");

                       if (GetCommand("watch_register", out Command _command)) {
                           Log($"3. Using the command \"{_command.ToFormattedString()}\" to register a new watcher being evaluated as a C# expression");
                       }

                       LogSeparator();
                   }
               ));

                AddBuiltInCommand(Command.Create(
                   "watch_commands",
                   "watcherscommands,watchers_commands",
                   "Get a list of all watchers-related commands",
                   () => {
                       List<Command> _commands = new List<Command>();

                       for (int i = 0; i < commands.Count; i++) {

                           Command _command = commands[i];
                           if (_command.Name.StartsWith("watch")) {
                               _commands.Add(_command);
                           }
                       }

                       _commands.Sort(new Comparison<Command>((a, b) => a.Name.CompareTo(b.Name)));

                       LogCollection("Watchers Commands", _commands, List);
                       LogSeparator();
                   }
               ));
            }

            // List
            {
                AddBuiltInCommand(Command.Create(
                    "watch_list",
                    "watcherslist,watchers_list,watchers",
                    "Displays a list of all registered watchers that can be displayed on-screen",
                    () => {
                        if (watchers.Count == 0) {
                            string _suffix = string.Empty;

                            if (GetCommand("watch_register", out Command _command)) {
                                _suffix = $" - Use {_command.ToFormattedString()} to register a new watcher";
                            }

                            Log($"No watcher registered{_suffix}");
                            return;
                        }

                        List<string> _values = new List<string>();

                        for (int i = 0; i < watchers.Count; i++) {

                            Pair<string, Watcher> _pair = watchers[i];

                            var _watcher = _pair.Second;
                            _values.Add($"[{_watcher.Order}] {$"{_pair.First}:".Bold()} {_watcher.ToString().Italic()} ({_watcher.Type})" +
                                        $"{(!_watcher.Enabled ? " [Disabled]" : string.Empty)}" +
                                        $"\n{Indent}{_watcher.Description}");
                        }

                        LogCollection("Watchers", _values, List);
                        LogSeparator();
                    }
                ));
            }

            // Display
            {
                AddBuiltInCommand(Command.Create<bool>(
                    "watch_display",
                    "watchersdisplay,watchers_display",
                    "Set or query whether the developer console stats are being displayed on-screen",
                    () => LogValue("Display Watchers", DisplayWatchers),
                    b => {
                        DisplayWatchers = b;
                        LogSuccess($"{DisplayWatchers.ToString(BooleanStringParser.EnabledDisabled)} watchers display");
                    },
                    new DeveloperConsoleCommandParameter("display", "Whether to display or hide the console watchers")
                ));
            }

            // Registration
            {
                AddBuiltInCommand(Command.Create<string, string, string, int>(
                    "watch_register",
                    "registerwatcher,register_watcher,watchers_registration,watchersregistration",
                    "Registers a new member watcher so that its value can be displayed on-screen",
                    null,
                    (_name, _description, _expression, _order) => {
                        if (string.IsNullOrEmpty(_name) || string.IsNullOrEmpty(_expression)) {
                            LogError("Name or expression cannot be null or empty");
                            return;
                        }

                        if (!_expression.EndsWith(";")) {
                            _expression += ";";
                        }

                        watchers.Add(_name, new EvaluatorWatcher(_expression, _description, true, _order));
                        LogSuccess($"Successfully registered {_name} as a console watcher");
                    },
                    new DeveloperConsoleCommandParameter("name", "Name of this watcher"),
                    new DeveloperConsoleCommandParameter("description", "Description of this watcher"),
                    new DeveloperConsoleCommandParameter("expression", "The C# expression to watch"),
                    new DeveloperConsoleCommandParameter("order", "Display order of this watcher (the higher the value, the higher on screen it is displayed")
                ));

                AddBuiltInCommand(Command.Create<string>(
                    "watch_unregister",
                    "unregisterwatcher,unregister_watcher,watchers_unregistration,watchersunregistration",
                    "Unregisters a specific watcher from the developer console",
                    null,
                    _name => {
                        if (!watchers.Remove(_name)) {
                            string _suffix = string.Empty;

                            if (GetCommand("watch_list", out Command _command)) {
                                _suffix = $" - Use {_command.ToFormattedString()} to get a list of all registered watchers";
                            }

                            Log($"Could not find any watcher with the name {_name}{_suffix}");
                            return;
                        }

                        LogSuccess($"Successfully unregistered watcher {_name} from the developer console");
                    },
                    new DeveloperConsoleCommandParameter("name", "Name of the watcher to unregister")
                ));
            }

            // Enable
            {
                AddBuiltInCommand(Command.Create<string, bool>(
                    "watch_enable",
                    "watch_toggle,watch_set",
                    "Enables/Disables a specific watcher from on-screen display",
                    null,
                    (_name, _enabled) => {
                        if (!watchers.ContainsKey(_name, out int _index)) {
                            string _suffix = string.Empty;

                            if (GetCommand("watch_list", out Command _command)) {
                                _suffix = $" - Use {_command.ToFormattedString()} to get a list of all registered watchers";
                            }

                            Log($"Could not find any watcher with the name {_name}{_suffix}");
                            return;
                        }

                        var _watcher = watchers[_index].Second;
                        _watcher.Enabled = _enabled;

                        LogSuccess($"{_enabled.ToString(BooleanStringParser.EnabledDisabled)} the watcher {_name} from on-screen display");
                    },
                    new DeveloperConsoleCommandParameter("name", "Name of the watcher to set display"),
                    new DeveloperConsoleCommandParameter("enabled", "Whether to enable or disable this watcher from on-screen display")
                ));
            }

            // Order
            {
                AddBuiltInCommand(Command.Create<string, int>(
                    "watch_order",
                    "watchersorder,watchers_order",
                    "Set the display order of a specific watcher",
                    null,
                    (_name, _order) => {
                        if (!watchers.ContainsKey(_name, out int _index)) {
                            string _suffix = string.Empty;

                            if (GetCommand("watch_list", out Command _command)) {
                                _suffix = $" - Use {_command.ToFormattedString()} to get a list of all registered watchers";
                            }

                            Log($"Could not find any watcher with the name {_name}{_suffix}");
                            return;
                        }

                        var _watcher = watchers[_index].Second;
                        _watcher.Order = _order;

                        watchers.Sort();
                        LogSuccess($"Successfully set the watcher {_name} display order to {_order}");
                    },
                    new DeveloperConsoleCommandParameter("name", "Name of the watcher to set the order"),
                    new DeveloperConsoleCommandParameter("order", "Display order of this watcher (the higher the value, the higher on screen it is displayed")
                ));
            }

            // Evaluate
            {
                AddBuiltInCommand(Command.Create<string>(
                    "watch_evaluate",
                    "watchersevaluate,watchers_evalute",
                    "Get the value of a specific watcher",
                    null,
                    _name => {
                        if (!watchers.ContainsKey(_name, out int _index)) {
                            string _suffix = string.Empty;

                            if (GetCommand("watch_list", out Command _command)) {
                                _suffix = $" - Use {_command.ToFormattedString()} to get a list of all registered watchers";
                            }

                            Log($"Could not find any watcher with the name {_name}{_suffix}");
                            return;
                        }

                        try {
                            LogValue(_name, EvaluateWatcher(watchers[_index].Second));
                        } catch (Exception e) {
                            Instance.LogException(e);
                        }
                    },
                    new DeveloperConsoleCommandParameter("name", "Name of the watcher to evaluate and get the value")
                ));
            }
            #endregion

            #region Miscs
            // Time.
            {
                AddBuiltInCommand(Command.Create(
                    "datetime",
                    "date_time",
                    "Displays the current date time",
                    () => LogValue("Date time", DateTime.Now.ToString(CultureInfo.CurrentCulture))
                ));

                AddBuiltInCommand(Command.Create(
                    "time",
                    "gametime,game_time",
                    "Displays the real time since the game started (in seconds)",
                    () => LogValue("Game time", Time.realtimeSinceStartupAsDouble)
                ));

                AddBuiltInCommand(Command.Create(
                    "frame",
                    "framecount,frame_count",
                    "Displays the total number of frames since the start of the application",
                    () => LogValue("Frame", Time.frameCount)
                ));
            }

            // System.
            {
                AddBuiltInCommand(Command.Create(
                    "system_info",
                    "systeminfo,sysinfo,sys_info",
                    "Displays information about the current system",
                    () => {
                        LogSeparator("System information");

                        LogValue("Name", SystemInfo.deviceName);
                        LogValue("Model", SystemInfo.deviceModel);
                        LogValue("Type", SystemInfo.deviceType, SystemInfo.operatingSystemFamily == OperatingSystemFamily.Other ? "" : $" ({SystemInfo.operatingSystemFamily})");
                        LogValue("OS", SystemInfo.operatingSystem);
                        if (SystemInfo.batteryLevel != -1) {
                            Log($"Battery status: {SystemInfo.batteryStatus} ({SystemInfo.batteryLevel * 100f}%)");
                        }

                        Log(string.Empty);

                        LogValue("CPU", SystemInfo.processorType);
                        LogValue($"{Indent}Memory size", SystemInfo.systemMemorySize, " megabytes");
                        LogValue($"{Indent}Processors", SystemInfo.processorCount);
                        LogValue($"{Indent}Frequency", SystemInfo.processorFrequency, " MHz");

                        Log(string.Empty);

                        LogValue("GPU", SystemInfo.graphicsDeviceName);
                        LogValue($"{Indent}Type", SystemInfo.graphicsDeviceType);
                        LogValue($"{Indent}Vendor", SystemInfo.graphicsDeviceVendor);
                        LogValue($"{Indent}Version", SystemInfo.graphicsDeviceVersion);
                        LogValue($"{Indent}Memory size", SystemInfo.graphicsMemorySize, " megabytes");
                        LogValue($"{Indent}Multi threaded", SystemInfo.graphicsMultiThreaded);

                        LogSeparator();
                    }
                ));
            }

            // Color
            {
                AddBuiltInCommand(Command.Create<Color>(
                    "color",
                    "colour",
                    "Displays a colour in the developer console",
                    null,
                    c => LogValue($"<color=#{ColorUtility.ToHtmlStringRGBA(c)}>Colour</color>", c),
                    new DeveloperConsoleCommandParameter("color", "Color to display - Formats: #RRGGBBAA (hex), #RRGGBB (hex), name (red,yellow,etc.), R.R,G.G,B.B (0.0-1.0)")
                ));
            }
            #endregion

            // --- Enhanced Editor --- \\

            #region Flag
            // Get.
            {
                AddBuiltInCommand(Command.Create<string>(
                    "getflag",
                    "get_flag",
                    "Get the current value of a flag",
                    null,
                    s => {
                        if (!FlagDatabase.Database.FindFlag(s, out Flag _flag)) {
                            LogError("Could not find the specified flag");
                            return;
                        }

                        LogValue(_flag.Name, _flag.Value);
                    },
                    new DeveloperConsoleCommandParameter("name", "Name of the flag to get")
                ));
            }

            // Set.
            {
                AddBuiltInCommand(Command.Create<string, bool>(
                    "setflag",
                    "set_flag",
                    "Set the current value of a flag",
                    null,
                    (s, b) => {
                        if (!FlagDatabase.Database.SetFlag(s, b)) {
                            LogError("Could not find the specified flag");
                            return;
                        }

                        LogSuccess($"{s} value set to {b}");
                    },
                    new DeveloperConsoleCommandParameter("name", "Name of the flag to set"),
                    new DeveloperConsoleCommandParameter("value", "Value to assign to the flag")
                ));

                AddBuiltInCommand(Command.Create<string, string, bool>(
                    "set_flag",
                    "setflag",
                    "Set the current value of a flag",
                    null,
                    (flagName, holderName, value) => {
                        if (!FlagDatabase.Database.SetFlag(flagName, holderName, value)) {
                            LogError("Could not find the specified flag");
                            return;
                        }

                        LogSuccess($"{flagName} value set to {value}");
                    },
                    new DeveloperConsoleCommandParameter("flagName", "Name of the flag to set"),
                    new DeveloperConsoleCommandParameter("holderName", "Name of the holder containing the flag"),
                    new DeveloperConsoleCommandParameter("value", "Value to assign to the flag")
                ));

                AddBuiltInCommand(Command.Create<string, bool>(
                    "set_flag_holder",
                    "setflagholder",
                    "Set the current value of all flags in a given holder",
                    null,
                    (holderName, value) => {
                        if (!FlagDatabase.Database.FindHolder(holderName, out FlagHolder _holder)) {
                            LogError("Could not find the specified FlagHolder");
                            return;
                        }

                        foreach (var _flag in _holder.Flags) {
                            _flag.Value = value;
                        }

                        LogSuccess($"{_holder.name} flag values set to {value}");
                    },
                    new DeveloperConsoleCommandParameter("holderName", "Name of the holder to set the flag values"),
                    new DeveloperConsoleCommandParameter("value", "Value to assign to all flags in the holder")
                ));
            }

            // List.
            {
                AddBuiltInCommand(Command.Create(
                    "flags",
                    "flaglist,flag_list",
                    "Get the current value of all flags",
                    () => {
                        List<Pair<Flag, FlagHolder>> _flags = new List<Pair<Flag, FlagHolder>>();
                        foreach (var _holder in FlagDatabase.Database.Holders) {
                            _flags.AddRange(Array.ConvertAll(_holder.Flags, f => new Pair<Flag, FlagHolder>(f, _holder)));
                        }

                        LogCollection("Flags", _flags, List, string.Empty, (f) => $"[{f.Second.name}] {f.First.Name}: {f.First.Value}");
                    }
                ));

                AddBuiltInCommand(Command.Create<string>(
                    "holder_flags",
                    "holderflags",
                    "Get the current value of all flags in a holder",
                    null,
                    s => {
                        if (!FlagDatabase.Database.FindHolder(s, out FlagHolder _holder)) {
                            LogError("Could not find the specified FlagHolder");
                            return;
                        }

                        LogCollection($"{_holder.name} Flags", _holder.Flags, List, string.Empty, (f) => $"{f.Name}: {f.Value}");
                    },
                    new DeveloperConsoleCommandParameter("name", "Name of the holder to get the associated flags")
                ));
            }

            // Enabled.
            {
                AddBuiltInCommand(Command.Create(
                    "enabledflags",
                    "enabled_flags",
                    "Get all currently enabled flags in the project",
                    () => {
                        List<Pair<Flag, FlagHolder>> _enabledFlags = new List<Pair<Flag, FlagHolder>>();

                        foreach (FlagHolder _holder in FlagDatabase.Database.Holders) {

                            for (int i = 0; i < _holder.Count; i++) {

                                Flag _flag = _holder[i];
                                if (_flag.Value) {
                                    _enabledFlags.Add(new Pair<Flag, FlagHolder>(_flag, _holder));
                                }
                            }
                        }

                        LogCollection($"Enabled Flags", _enabledFlags, List, string.Empty, (f) => $"[{f.Second.name}] {f.First.Name}");
                    }
                ));

                AddBuiltInCommand(Command.Create<string>(
                    "enabled_holderflags",
                    "enabledholderflags",
                    "Get all currently enabled flags in a specific holder",
                    null,
                    s => {
                        if (!FlagDatabase.Database.FindHolder(s, out FlagHolder _holder)) {
                            LogError("Could not find the specified FlagHolder");
                            return;
                        }

                        List<Flag> _enabledFlags = new List<Flag>();

                        for (int i = 0; i < _holder.Count; i++) {

                            Flag _flag = _holder[i];
                            if (_flag.Value) {
                                _enabledFlags.Add(_flag);
                            }
                        }

                        LogCollection($"{_holder.name} Enabled Flags", _enabledFlags, List, string.Empty, (f) => f.Name);
                    },
                    new DeveloperConsoleCommandParameter("name", "Name of the holder to get the enabled flags")
                ));
            }

            // Disabled.
            {
                AddBuiltInCommand(Command.Create(
                    "disabledflags",
                    "disabled_flags",
                    "Get all currently disabled flags in the project",
                    () => {
                        List<Pair<Flag, FlagHolder>> _disabledFlags = new List<Pair<Flag, FlagHolder>>();

                        foreach (var _holder in FlagDatabase.Database.Holders) {

                            for (int i = 0; i < _holder.Count; i++) {

                                Flag _flag = _holder[i];
                                if (!_flag.Value) {
                                    _disabledFlags.Add(new Pair<Flag, FlagHolder>(_flag, _holder));
                                }
                            }
                        }

                        LogCollection($"Disabled Flags", _disabledFlags, List, string.Empty, (f) => $"[{f.Second.name}] {f.First.Name}");
                    }
                ));

                AddBuiltInCommand(Command.Create<string>(
                    "disabled_holderflags",
                    "disabledholderflags",
                    "Get all currently disabled flags in a specific holder",
                    null,
                    s => {
                        if (!FlagDatabase.Database.FindHolder(s, out FlagHolder _holder)) {
                            LogError("Could not find the specified FlagHolder");
                            return;
                        }

                        List<Flag> _disabledFlags = new List<Flag>();

                        for (int i = 0; i < _holder.Count; i++) {

                            Flag _flag = _holder[i];
                            if (!_flag.Value) {
                                _disabledFlags.Add(_flag);
                            }
                        }

                        LogCollection($"{_holder.name} Disabled Flags", _disabledFlags, List, string.Empty, (f) => f.Name);
                    },
                    new DeveloperConsoleCommandParameter("name", "Name of the holder to get the disabled flags")
                ));
            }
            #endregion

            #region Scene Management
            // Load
            {
                AddBuiltInCommand(Command.Create<string>(
                    "loadscenebundle",
                    "load_scenebundle,loadbundle,load_bundle",
                    "Loads a SceneBundle by its name",
                    null,
                    s => {
                        if (!BuildSceneDatabase.Database.GetSceneBundle(s, out SceneBundle _bundle)) {
                            LogError($"{s.Bold()} Scene Bundle could not be found");
                            return;
                        }

                        EnhancedSceneManager.Instance.LoadSceneBundle(_bundle, LoadSceneMode.Single);
                        LogSuccess($"Loading SceneBundle {s.Bold()}");
                    },
                    new DeveloperConsoleCommandParameter("sceneBundleName", "Name of the SceneBundle to load")
                ));

                AddBuiltInCommand(Command.Create<string>(
                    "loadscenebundle_additively",
                    "load_scenebundle_additively,loadbundle_additively,load_bundle_additively",
                    "Loads additively a SceneBundle by its name",
                    null,
                    s => {
                        if (!BuildSceneDatabase.Database.GetSceneBundle(s, out SceneBundle _bundle)) {
                            LogError($"{s.Bold()} Scene Bundle could not be found");
                            return;
                        }

                        EnhancedSceneManager.Instance.LoadSceneBundle(_bundle, LoadSceneMode.Additive);
                        LogSuccess($"Loading additively SceneBundle {s.Bold()}");
                    },
                    new DeveloperConsoleCommandParameter("sceneBundleName", "Name of the SceneBundle to load")
                ));
            }

            // Unload
            {
                AddBuiltInCommand(Command.Create<string>(
                    "unloadscenebundle",
                    "unload_scenebundle,unloadbundle,unload_bundle",
                    "Unloads a SceneBundle by its name",
                    null,
                    s => {
                        if (!BuildSceneDatabase.Database.GetSceneBundle(s, out SceneBundle _bundle)) {
                            LogError($"{s.Bold()} Scene Bundle could not be found");
                            return;
                        }

                        EnhancedSceneManager.Instance.UnloadSceneBundle(_bundle, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
                        LogSuccess($"Loading SceneBundle {s.Bold()}");
                    },
                    new DeveloperConsoleCommandParameter("sceneBundleName", "Name of the SceneBundle to unload")
                ));
            }

            // List.
            {
                AddBuiltInCommand(Command.Create(
                    "scenebundles",
                    "scenebundlelist,scenebundle_list",
                    "Get a list of all SceneBundle in the project",
                    () => {
                        var _database = BuildSceneDatabase.Database;
                        SceneBundle[] _bundles = new SceneBundle[_database.SceneBundleCount];

                        for (int i = 0; i < _bundles.Length; i++) {
                            _bundles[i] = _database.GetSceneBundleAt(i);
                        }

                        LogCollection("Scene Bundles", _bundles, List, string.Empty, (s) => s.name);
                    }
                ));
            }
            #endregion

            // ----- Local Method ----- \\

            void AddBuiltInCommand(Command _command) {
                AddCommand(_command, true);
            }
        }
        #endregion

        #region Suggestion
        private static readonly PairCollection<string, Command> commandSuggestions  = new PairCollection<string, Command>();

        private static int commandSuggestionIndex   = -1;           // Index of the currently displayed command from the suggestions (-1 for none).
        private static Command commandSuggestion    = default;      // Currently displayed command from the suggestions (used to retrieve its index when the input changed).
        
        /// <summary>
        /// Index of the currently displayed command suggestion.
        /// </summary>
        public static int CommandSuggestionIndex {
            get { return commandSuggestionIndex; }
            set {
                if (commandSuggestions.Count == 0) {
                    value = -1;
                } else {
                    while (value < 0) {
                        value += commandSuggestions.Count;
                    }

                    while (value >= commandSuggestions.Count) {
                        value -= commandSuggestions.Count;
                    }
                }

                // Update the associated command.
                commandSuggestionIndex = value;
                Instance.UpdateCommandSuggestion();
            }
        }

        /// <summary>
        /// Get the current command suggestion as a string.
        /// </summary>
        public static string CommandSuggestion {
            get { return commandSuggestions[commandSuggestionIndex].First; }
        }

        /// <summary>
        /// Suggestion text, displayed behind the input field.
        /// </summary>
        public string SuggestionText {
            get { return suggestionText.text; }
            set {
                suggestionText.text = value;
                inputField.placeholder.gameObject.SetActive(string.IsNullOrEmpty(value));
            }
        }

        /// <summary>
        /// Not implemented yet.
        /// </summary>
        public string HintText {
            get { return hintText.text; }
            set { hintText.text = value; }
        }

        // -------------------------------------------
        // Suggestion
        // -------------------------------------------

        /// <summary>
        /// Refreshes the command suggestions based on the current user input.
        /// </summary>
        private void RefreshCommandSuggestions() {
            commandSuggestions.Clear();
            CommandHistoryIndex = -1;

            string _text = InputField;

            // Do not display any command suggestion if the user is not entering the name of the command.
            if ((_text.Length == 0) || _text.StartsWith(" ")) {
                CommandSuggestionIndex = -1;
                return;
            }

            // Get a list of all command names and aliases that match the current input.
            PairCollection<string, Command> _aliasSuggestions = new PairCollection<string, Command>();
            string[] _inputs = _text.Split(' ');

            _text = _inputs.First().ToLower();

            for (int i = 0; i < commands.Count; i++) {

                Command _command = commands[i];

                // Ignore command with less parameters than specified in the input.
                if (_command.Parameters.Length < (_inputs.Length - 1)) {
                    continue;
                }

                if (_command.Name.StartsWith(_text)) {
                    commandSuggestions.Add(new Pair<string, Command>(_command.Name, _command));
                }

                // Store aliases separately and insert them at the end of the list,
                // so that the suggestions contains real command names before any alias.
                foreach (string _alias in _command.Aliases) {
                    if (_alias.StartsWith(_text)) {
                        _aliasSuggestions.Add(new Pair<string, Command>(_alias, _command));
                    }
                }
            }

            commandSuggestions.AddRange(_aliasSuggestions);

            int _commandIndex = Mathf.Max(0, commandSuggestions.FindIndex((c) => c.Second == commandSuggestion));
            CommandSuggestionIndex = _commandIndex;
        }

        /// <summary>
        /// Set the current command suggestion.
        /// </summary>
        private void UpdateCommandSuggestion() {
            // No suggestion available.
            if ((commandSuggestionIndex == -1) || (commandSuggestions.Count == 0)) {
                commandSuggestion = null;

                SuggestionText = string.Empty;
                HintText = string.Empty;

                return;
            }

            // Get the new selected command suggestion.
            commandSuggestion = commandSuggestions[commandSuggestionIndex].Second;

            string[] _inputs = InputField.Split(' ');
            string _fullSuggestion = CommandSuggestion;

            // Display the current suggestion command remaining name characters or parameters.
            string _suggestion = InputField;

            if (_inputs.Length == 1) {
                _suggestion += _fullSuggestion.Substring(_inputs.First().Length);
            } else {
                string[] _suggestions = _fullSuggestion.Split(' ');

                if (_suggestions.Length >= _inputs.Length) {
                    _suggestion += " " + string.Join(" ", _suggestions, _inputs.Length, _suggestions.Length - _inputs.Length);
                }
            }

            SuggestionText = _suggestion;
            HintText = commandSuggestion.ToFormattedString();
        }

        /// <summary>
        /// Autocompletes the input field based on the current command suggestion.
        /// </summary>
        private void AutoComplete() {
            string _input = InputField;

            if (CommandHistoryIndex != -1) {
                _input = commandHistory[CommandHistoryIndex];
            } else if ((commandSuggestionIndex != -1) && (_input.Split(' ').Length == 1)) {
                _input = string.Concat(_input, CommandSuggestion.Substring(_input.Length));
            }

            InputField = _input;
            ResetInputCaret();
        }
        #endregion

        #region Evaluator
        private const string MonoNotSupportedText = "C# expression evaluation is not supported on this platform";

        /// <summary>
        /// Used by "cs_evaluate" and "cs_run" to execute C# expressions or statements.
        /// </summary>
        private static Evaluator monoEvaluator = null;

        // -----------------------

        private void InitMonoEvaluator() {
            try {
                // References all assemblies of the domain in the settings.
                CompilerSettings _settings = new CompilerSettings();
                Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (Assembly _assembly in _assemblies) {
                    _settings.AssemblyReferences.Add(_assembly.FullName);
                }

                CompilerContext _context = new CompilerContext(_settings, new ConsoleReportPrinter());
                monoEvaluator = new Evaluator(_context);

                // Register the included using statements.
                for (int i = 0; i < IncludedUsing.Count; i++) {

                    string _includedUsing = IncludedUsing[i];
                    monoEvaluator.Run($"using {_includedUsing};");
                }

            } catch (Exception) {
                LogWarning($"Some features may not be available: {MonoNotSupportedText}");
            }
        }

        /// <summary>
        /// Tries to evaluate the specified input as a regular C# expression or statement.
        /// </summary>
        /// <param name="_input">The input to evaluate.</param>
        /// <param name="_result">The result of the evaluated input.</param>
        /// <param name="_enableLogs">If true, logs will be sent to the console depending on the expression result.</param>
        /// <returns>True if the specified input could be evaluated, false otherwise.</returns>
        public static bool EvaluateInput(string _input, out object _result, bool _enableLogs = true) {
            // No evaluator.
            if (!IsEvaluatorEnabled(_enableLogs)) {
                _result = null;
                return false;
            }

            try {
                if (!_input.EndsWith(";")) {
                    _input += ";";
                }

                string _evaluation = monoEvaluator.Evaluate(_input, out _result, out bool _resultSet);

                if (!string.IsNullOrEmpty(_evaluation)) {
                    if (_enableLogs) {
                        Log($"The C# expression or statement could not be evaluated: {_evaluation}");
                    }

                    return false;
                }

                // Log expression result.
                _evaluation = string.Empty;

                if (_result == null) {
                    // Early return.
                    if (!_enableLogs) {
                        return true;
                    }

                    _evaluation = "Null";
                } else if ((_result.GetType() != typeof(string)) && typeof(IEnumerable).IsAssignableFrom(_result.GetType())) {

                    IEnumerable _collection = _result as IEnumerable;
                    List<string> _stringCollection = new List<string>();

                    foreach (var _element in _collection) {
                        _stringCollection.Add(_element.ToString());
                    }

                    _evaluation = $"{{ {string.Join(", ", _stringCollection)} }}";
                }

                if (_result != null) {
                    _evaluation += $" ({_result.GetType().Name})";
                }

                LogSuccess($"Result: {_evaluation}");
                return true;
            } catch (Exception e) {
                LogError($"An exception was thrown whilst evaluating the C# expression or statement: {e.Message}");
            }

            _result = null;
            return false;
        }

        /// <summary>
        /// Tries to run the specified input as a regular C# expression or statement.
        /// </summary>
        /// <param name="_input">The input to execute.</param>
        /// <param name="_enableLogs">If true, logs will be sent to the console depending on the expression result.</param>
        /// <returns>True if the specified input could be successfully executed, false otherwise.</returns>
        public static bool RunInput(string _input, bool _enableLogs = true) {
            // No evaluator.
            if (!IsEvaluatorEnabled(_enableLogs)) {
                return false;
            }

            try {
                if (!_input.EndsWith(";")) {
                    _input += ";";
                }

                if (monoEvaluator.Run(_input)) {
                    if (_enableLogs) {
                        LogSuccess("Successfully executed the C# expression or statement");
                    }

                    return true;
                }

                if (_enableLogs) {
                    LogError("Failed to parse the C# expression or statement");
                }
            } catch (Exception e) {
                LogError($"An exception was thrown whilst evaluating the C# expression or statement: {e.Message}");
            }

            return false;
        }

        // -----------------------

        /// <summary>
        /// Get if the CSharp evaluator is currently enabled.
        /// </summary>
        /// <param name="_logMessage">If true, a message will be logged to the console if the evaluator is not enabled.</param>
        /// <returns>True if the evaluator is enabled, false otherwise.</returns>
        public static bool IsEvaluatorEnabled(bool _logMessage = true) {
            if (monoEvaluator == null) {
                if (_logMessage) {
                    LogError(MonoNotSupportedText);
                }

                return false;
            }

            return true;
        }
        #endregion

        #region Log
        private const string ClearLogText = "Welcome to the enhanced developer console.\n" +
                                            "Type <b>devconsole</b> for instructions on how to use the developer console.";

        private const string DefaultSeparator   = "─────────────────────\n";
        private const string SeparatorFormat    = "\n// ──────────────────────────────────────\n" +
                                                    "// {0}\n" +
                                                    "// ──────────────────────────────────────\n";

        private static readonly Color successColor      = SuperColor.Green.Get();
        private static readonly Color warningColor      = SuperColor.Orange.Get();
        private static readonly Color errorColor        = SuperColor.Crimson.Get();
        private static readonly Color assertColor       = SuperColor.Pumpkin.Get();
        private static readonly Color exceptionColor    = SuperColor.Indigo.Get();

        private static string logBuffer = string.Empty;

        // -----------------------

        /// <summary>
        /// Logs a message to the developer console.
        /// </summary>
        /// <param name="_message">Message as <see cref="string"/> to display.</param>
        [Conditional("DEVELOPER_CONSOLE")]
        public static void Log(string _message) {
            logBuffer += $"\n{_message}";
        }

        /// <param name="_color"><see cref="Color"/> of the message to display.</param>
        /// <inheritdoc cref="Log(string)"/>
        public static void Log(string _message, Color _color) {
            Log(_message.Color(_color));
        }

        // -------------------------------------------
        // Color
        // -------------------------------------------

        /// <summary>
        /// Logs a success message to the developer console.
        /// </summary>
        /// <inheritdoc cref="Log(string)"/>
        public static void LogSuccess(string _message) {
            Log(_message, successColor);
        }

        /// <summary>
        /// Logs a warning message to the developer console.
        /// </summary>
        /// <inheritdoc cref="Log(string)"/>
        public static void LogWarning(string _message) {
            Log(_message, warningColor);
        }

        /// <summary>
        /// Logs an error message to the developer console.
        /// </summary>
        /// <inheritdoc cref="Log(string)"/>
        public static void LogError(string _message) {
            Log(_message, errorColor);
        }

        // -------------------------------------------
        // Specials
        // -------------------------------------------

        /// <inheritdoc cref="LogSeparator(string)"/>
        public static void LogSeparator() {
            Log(DefaultSeparator);
        }

        /// <summary>
        /// Logs a separator to the developer console.
        /// </summary>
        /// <param name="_separator">Separator to display.</param>
        public static void LogSeparator(string _separator) {
            Log(string.Format(SeparatorFormat, _separator));
        }

        /// <summary>
        /// Logs a specific value to the developer console.
        /// </summary>
        /// <param name="_variableName">Displayed name of the value.</param>
        /// <param name="_value">The actual value to log.</param>
        /// <param name="_suffix">Optional message displayed to right of the value.</param>
        public static void LogValue(string _variableName, object _value, string _suffix = "") {
            Log($"{$"{_variableName}:".Bold()} {_value}{_suffix}");
        }

        /// <summary>
        /// Logs a specific collection to the developer console.
        /// </summary>
        /// <typeparam name="T">Description displayed before the collection content.</typeparam>
        /// <param name="_header">The header displayed above the collection.</param>
        /// <param name="_collection">The collection to display.</param>
        /// <param name="_prefix">Prefix displayed to the left of each element of the collection.</param>
        /// <param name="_suffix">Suffix displayed to the right of each element of the collection.</param>
        /// <param name="_parser">Optional collection type parser.</param>
        public static void LogCollection<T>(string _header, in IList<T> _collection, string _prefix = " • ", string _suffix = "", Func<T, string> _parser = null) {
            string _collectionMessage = string.Empty;

            if (!string.IsNullOrEmpty(_header)) {
                LogSeparator($"{_header} [{_collection.Count}]");
            }

            if ((_collection == null) || (_collection.Count == 0)) {
                // Empty collection message.
                _collectionMessage += "empty".Italic();
            } else {
                // Display each element on a new line.
                _collectionMessage += string.Join("\n", new List<T>(_collection).ConvertAll((v) => {
                    return $"{_prefix}{Parse(v)}{_suffix}";
                }));
            }

            Log(_collectionMessage);

            // ----- Local Method ----- \\

            string Parse(T _value) {
                if (_parser != null) {
                    return _parser(_value);
                }

                return _value.ToString();
            }
        }

        /// <inheritdoc cref="LogCommand(string)"/>
        /// <param name="_command">The command to log.</param>
        public static void LogCommand(Command _command) {
            LogCommand(_command.ToFormattedString());
        }

        /// <summary>
        /// Logs a command to the developer console.
        /// <para/>
        /// /!\ Doesn't execute the associated command /!\
        /// </summary>
        /// <param name="_name">Name of the command to log.</param>
        public static void LogCommand(string _name) {
            Log($">> {_name}");
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Clears the current log buffer.
        /// </summary>
        public static void ClearLog() {
            logBuffer = ClearLogText;
        }

        // -------------------------------------------
        // Callback
        // -------------------------------------------

        /// <summary>
        /// Called when a Unity console log message is received.
        /// </summary>
        private static void OnLogMessageReceived(string _log, string _stackTrace, LogType _type) {

            if (!DisplayedLogs.HasFlagUnsafe(_type.ToFlag())) {
                return;
            }

            Color _color;

            switch (_type) {
                case LogType.Warning:
                    _color = warningColor;
                    break;

                case LogType.Error:
                    _color = errorColor;
                    break;

                case LogType.Assert:
                    _color = assertColor;
                    break;

                case LogType.Exception:
                    _color = exceptionColor;
                    break;

                case LogType.Log:
                default:
                    _color = Color.white;
                    break;
            }

            Log($"[{DateTime.Now.ToLongTimeString()}] {_type.ToString().Color(_color).Bold()}: {_log.Italic()}");
        }
        #endregion

        #region Panel
        private const int MaxLogObjectCount     = 9;
        private const int MinLogTextSize        = 8;
        private const int MaxLogTextSize        = 36;

        private const int MaxProcessPerFrame    = 8000;
        private const int MaximumTextCharacter  = 6000;
        private const int MaximumLogCharacter   = 5900;

        private const float ScrollStepPerecent  = .2f;

        private static readonly EnhancedCollection<Text> logObjects = new EnhancedCollection<Text>();
        private static float logTextSize = 0f;

        private static int frameCount       = 0;
        private static int processThisFrame = 0;

        /// <summary>
        /// The font size of each log text.
        /// </summary>
        public static float LogTextSize {
            get {
                return logTextSize;
            }
            set {
                value = Mathf.Clamp(value, MinLogTextSize, MaxLogTextSize);
                if (value == logTextSize) {
                    return;
                }

                logTextSize = value;

                if (logObjects.Count != 0) {

                    for (int i = 0; i < logObjects.Count; i++) {

                        Text _log = logObjects[i];
                        _log.fontSize = (int)value;
                    }
                }
            }
        }

        /// <summary>
        /// The value used for increase/decrease the scroll bar position.
        /// </summary>
        public float ScrollStepValue {
            get {
                return scroll.verticalScrollbar.size * ScrollStepPerecent;
            }
        }

        // -------------------------------------------
        // Log
        // -------------------------------------------

        /// <summary>
        /// Processes and displays the given log to the console.
        /// </summary>
        private bool ProcessLogText(ref string _logText, int _count) {
            if (_count == 0) {
                return false;
            }

            // Maximum process per frame reset.
            int _frame = Time.frameCount;
            if (_frame != frameCount) {
                frameCount = _frame;
                processThisFrame = 0;
            }

            int _characterCount = logObjects.Last().text.Length;
            int _remainingCount = MaximumTextCharacter - _characterCount;

            // If the current log cannot hold all the new characters to add, split the log text.
            if (_count > _remainingCount) {
                int _index = (_remainingCount > 0)
                           ? _logText.LastIndexOf('\n', _remainingCount - 1, _remainingCount)
                           : -1;

                // Fill the last log content.
                if (_index != -1) {
                    logObjects.Last().text += _logText.Substring(0, _index);

                    _logText = _logText.Substring(_index);
                    _count -= _index;

                    processThisFrame += _index;
                    if (processThisFrame > MaxProcessPerFrame) {
                        return false;
                    }
                }

                // Split the current text if too long for a single log.
                if (_count > MaximumLogCharacter) {
                    _index = _logText.LastIndexOf('\n', MaximumLogCharacter - 1, MaximumLogCharacter);

                    if (_index == -1) {
                        _index = _logText.LastIndexOf(' ', MaximumLogCharacter - 1, MaximumLogCharacter);

                        if (_index == -1) {
                            _index = MaximumLogCharacter;
                        }
                    }

                    if (!ProcessLogText(ref _logText, _index)) {
                        return false;
                    }

                    _count -= _index;
                }

                // Instantiate a new log object.
                AddLogObject();
            }

            logObjects.Last().text += _logText.Substring(0, _count);
            _logText = _logText.Substring(_count);

            processThisFrame += _count;
            return processThisFrame < MaxProcessPerFrame;
        }

        /// <summary>
        /// Creates a new log object and add it to the panel.
        /// </summary>
        private void AddLogObject() {
            Text _log = Instantiate(logObjectPrefab, logParent);
            _log.text = string.Empty;
            _log.gameObject.SetActive(true);

            logObjects.Add(_log);

            // Log limit.
            if (logObjects.Count == MaxLogObjectCount) {
                Destroy(logObjects.First().gameObject);
                logObjects.RemoveFirst();
            }
        }

        /// <summary>
        /// Clears all existing log object instances.
        /// </summary>
        private static void ClearLogFields() {

            for (int i = 0; i < logObjects.Count; i++) {

                Text _log = logObjects[i];
                Destroy(_log.gameObject);
            }

            logObjects.Clear();
            Instance.AddLogObject();
        }

        // -------------------------------------------
        // Event
        // -------------------------------------------

        /// <summary>
        /// Called from a UI event when pressing the increase scroll position button.
        /// </summary>
        public void OnIncreaseScrollPosition() {
            SetScrollPosition(scroll.verticalNormalizedPosition + ScrollStepValue);
        }

        /// <summary>
        /// Called from a UI event when pressing the decrease scroll position button.
        /// </summary>
        public void OnDecreaseScrollPosition() {
            SetScrollPosition(scroll.verticalNormalizedPosition - ScrollStepValue);
        }

        // -------------------------------------------
        // Log
        // -------------------------------------------

        /// <summary>
        /// Resets the panel scroll view position.
        /// </summary>
        public void ResetScroll() {
            SetScrollPosition(0f);
        }

        /// <summary>
        /// Set the current scroll position.
        /// </summary>
        /// <param name="_value">Normalized value of the new scroll position (between 0 and 1).</param>
        private void SetScrollPosition(float _value) {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.verticalScrollbar.transform as RectTransform);

            scroll.verticalNormalizedPosition = _value;
        }

        // -------------------------------------------
        // Init
        // -------------------------------------------

        private void InitPanel() {
            logTextSize = logObjectPrefab.fontSize;
        }
        #endregion

        #region Input Field
        // -------------------------------------------
        // Constant & Property
        // -------------------------------------------

        private const InputKey UpArrowKey   = InputKey.UpArrow;
        private const InputKey DownArrowKey = InputKey.DownArrow;
        private const InputKey BackspaceKey = InputKey.Backspace;

        #if NEW_INPUT_SYSTEM
        private const InputKey LeftControlKey   = InputKey.LeftCtrl;
        #else
        private const InputKey LeftControlKey   = InputKey.LeftControl;
        #endif

        private static bool ignoreInputChange = false;

        /// <summary>
        /// Text value of the input field.
        /// </summary>
        public string InputField {
            get { return inputField.text; }
            set { inputField.text = value; }
        }

        /// <summary>
        /// Position of the input field caret.
        /// </summary>
        private int InputCaretPosition {
            get => inputField.caretPosition;
            set => inputField.caretPosition = value;
        }

        // -------------------------------------------
        // Event
        // -------------------------------------------

        /// <summary>
        /// Called from a UI event when the input field text is changed.
        /// </summary>
        private void OnInputValueChanged(string _text) {
            if (ignoreInputChange) {
                return;
            }

            // If Ctrl+ Backspace was pressed, remove the last word before the caret position.
            if (GetKey(LeftControlKey) && GetKeyDown(BackspaceKey) && !string.IsNullOrEmpty(_text)) {
                int _caretPosition = InputCaretPosition;
                int _lastWordIndex = _text.Substring(0, _caretPosition).LastIndexOf(' ');

                if (_lastWordIndex != -1) {
                    _text = _text.Substring(0, _lastWordIndex) + _text.Substring(_caretPosition, _text.Length - _caretPosition);

                    // Ignore next callback caused by the following text set.
                    ignoreInputChange = true;

                    InputField = _text;
                    InputCaretPosition = _lastWordIndex;

                    ignoreInputChange = false;
                }
            }

            RefreshCommandSuggestions();
        }

        /// <summary>
        /// Called from a UI event when a character is going to be added to the input field.
        /// </summary>
        private char OnValidateInput(string _text, int _charIndex, char _addedChar) {
            const char EmptyChar = '\0';

            // Ignore any input while the console is disabled.
            if (!IsOpen) {
                return EmptyChar;
            }

            // If a new line character is entered, submit the command.
            if (_addedChar == '\n') {
                _addedChar = EmptyChar;
                SubmitInput();
            }

            // If a tab character is entered, autocomplete the suggested command.
            else if (_addedChar == '\t') {
                _addedChar = EmptyChar;
                AutoComplete();
            }

            return _addedChar;
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Submits the currently entered input in the input field, and try the run the associated command.
        /// </summary>
        public void SubmitInput() {
            string _input = InputField;

            if (!string.IsNullOrWhiteSpace(_input)) {
                RunCommand(_input);
            }

            InputField = string.Empty;

            ResetScroll();
            FocusInputField();
        }

        /// <summary>
        /// Focuses the input field.
        /// </summary>
        public void FocusInputField() {
            EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
            inputField.ActivateInputField();
        }

        /// <summary>
        /// Resets the current input caret position to the end of the line.
        /// </summary>
        public void ResetInputCaret() {
            var _inputField = Instance.inputField;

            #if TEXT_MESH_PRO
            _inputField.MoveToEndOfLine(false, false);
            #else
            _inputField.caretPosition = _inputField.text.Length;
            #endif
        }
        #endregion

        #region Activation
        private static GameState gameState = null;

        // -----------------------

        /// <summary>
        /// Opens the developer console and enable it.
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.Green)]
        public static void Open() {
            if (!gameState.IsActive()) {
                gameState = GameState.CreateState(Instance.gameStateType);
            }
        }

        /// <summary>
        /// Closes the developer console and disable it.
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.Crimson)]
        public static void Close() {
            if (gameState.IsActive()) {
                gameState.RemoveState();
            }
        }

        // -----------------------

        void IGameStateLifetimeCallback.OnInit(GameState _state) {
            #if !DEVELOPER_CONSOLE
            _state.RemoveState();
            return;
            #endif

            gameState = _state;
            group.Show();

            FocusInputField();
        }

        void IGameStateLifetimeCallback.OnTerminate(GameState _state) {
            gameState = null;
            group.Hide();

            // Restore selected object.
            foreach (var _object in FindObjectsOfType<Selectable>()) {
                if (_object.IsActive() && _object.IsInteractable()) {
                    EventSystem.current.SetSelectedGameObject(_object.gameObject);
                    break;
                }
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Clears the developer console content.
        /// </summary>
        public static void Clear() {
            ClearLog();
            ClearLogFields();
        }

        /// <summary>
        /// Resets the dveloper console position and size on screen.
        /// </summary>
        public static void ResetPosition() {
            Instance.resizePanel.ResetSize();
            Instance.dragPanel.ResetPosition();
        }

        /// <summary>
        /// Saves the developer console settings.
        /// </summary>
        public static void SaveSettings() {
            Settings.Save();
        }

        // -------------------------------------------
        // Input
        // -------------------------------------------

        internal static bool GetKeyDown(InputKey key) {
            #if NEW_INPUT_SYSTEM
            return Keyboard.current[key].wasPressedThisFrame;
            #else
            return InputSystem.GetKeyDown(key);
            #endif
        }

        internal static bool GetKey(InputKey key) {
            #if NEW_INPUT_SYSTEM
            return Keyboard.current[key].isPressed;
            #else
            return InputSystem.GetKey(key);
            #endif
        }

        private static List<InputKey>GetKeys(string _input) {
            List<InputKey> _keys = new List<InputKey>();

            foreach (string _value in _input.Split(',')) {
                if (Enum.TryParse(_value, false, out InputKey _key)) {
                    _keys.Add(_key);
                }
            }

            return _keys;
        }

        // -------------------------------------------
        // Enum
        // -------------------------------------------

        private const int MaxCachedEnumTypes = 25;
        private static readonly EnhancedCollection<Type> cacheEnumTypes = new EnhancedCollection<Type>(MaxCachedEnumTypes);

        // -----------------------

        private static List<Type> GetEnum(string _name) {
            List<Type> _enums = new List<Type>();

            if (cacheEnumTypes.Find(t => (t.Name == _name) || (t.FullName == _name), out Type _enumType)) {
                _enums.Add(_enumType);
                return _enums;
            }

            // Cache limit.
            if (cacheEnumTypes.Count > MaxCachedEnumTypes) {
                cacheEnumTypes.RemoveAt(0);
            }

            try {
                // Assembly.
                Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var _assembly in _assemblies) {
                    try {
                        // Types.
                        Type[] _types = _assembly.GetTypes();

                        foreach (var _type in _types) {
                            if (!_type.IsEnum) {
                                continue;
                            }

                            // If the type exactly matches the given name, only return it.
                            if (_type.FullName == _name) {
                                _enums.Clear();
                                _enums.Add(_type);

                                cacheEnumTypes.Add(_type);
                                return _enums;
                            }

                            if (_type.Name == _name) {
                                _enums.Add(_type);
                            }
                        }
                    } catch (Exception) { }
                }
            } catch (Exception e) {
                // Security.
                Instance.LogException(e);
            }

            // Cache enum type.
            if (_enums.Count == 1) {
                cacheEnumTypes.Add(_enums.First());
            }

            return _enums;
        }
        #endregion

        // ===== Watchers ===== \\

        #region Watcher
        private const float WatcherUpdateInterval = .1f;

        /// <summary>
        /// Whether value watchers are currently displayed on screen or not.
        /// </summary>
        public static bool DisplayWatchers = false;

        private static readonly PairCollection<string, Watcher> watchers = new PairCollection<string, Watcher>();
        private static double watcherUpdateTime = 0d;

        // -------------------------------------------
        // Registration
        // -------------------------------------------

        /// <summary>
        /// Registers a new member watcher to be displayed on screen.
        /// </summary>
        /// <param name="_name">Identifier name of the watcher.</param>
        /// <param name="_description">Description of this watcher.</param>
        /// <param name="_getter">Getter method for this watcher associated value.</param>
        /// <param name="_enabled">Whether to enable or not this watcher.</param>
        /// <param name="_order">Order used to draw watchers. The higher the value, the higher the position on screen.</param>
        [Conditional("DEVELOPER_CONSOLE")]
        public static void RegisterWatcher(string _name, string _description, Func<object> _getter, bool _enabled, int _order = 0) {
            if (string.IsNullOrEmpty(_name)) {
                Instance.LogErrorMessage("Cannot register a watcher with no name");
                return;
            }

            if (_getter == null) {
                Instance.LogErrorMessage("Cannot register a watcher without a valid getter");
                return;
            }

            // Registration.
            if (watchers.ContainsKey(_name)) {
                Instance.LogWarningMessage($"Overriding watcher with name: \"{_name}\"");
            }

            watchers.Set(_name, new LambdaWatcher(_getter, _description, _enabled));
            watchers.Sort((a, b) => a.Second.CompareTo(b.Second));
        }

        /// <summary>
        /// Unregisters and removes a specific watcher from the developer console.
        /// </summary>
        /// <param name="_name">Name of the watcher to unregister.</param>
        [Conditional("DEVELOPER_CONSOLE")]
        public static void UnregisterWatcher(string _name) {
            watchers.Remove(_name);
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        private void UpdateWatchers() {
            // Hide pin values.
            if (!DisplayWatchers || (watchers.Count == 0)) {
                watcherGroup.Hide();
                return;
            }

            // Check if should update the cached stats
            if (Time.realtimeSinceStartupAsDouble <= watcherUpdateTime) {
                return;
            }

            watcherUpdateTime = Time.realtimeSinceStartupAsDouble + WatcherUpdateInterval;
            StringBuilder _builder = new StringBuilder(string.Empty);

            // Get each 
            for (int i = 0; i < watchers.Count; i++) {

                Pair<string, Watcher> _pair = watchers[i];
                Watcher _watcher = _pair.Second;

                if (_watcher.Enabled) {
                    _builder.AppendLine($"{_pair.First}: {EvaluateWatcher(_watcher)}");
                }
            }

            string _fullText = _builder.ToString();

            watcherText.text = _fullText;
            watcherGroup.SetVisibility(!string.IsNullOrEmpty(_fullText));
        }

        private static string EvaluateWatcher(Watcher _watcher) {
            if (monoEvaluator == null) {
                return MonoNotSupportedText;
            }

            string _value;

            try {
                object _rawValue = _watcher.GetValue(monoEvaluator);

                if (_rawValue == null) {
                    _value = "NULL".Color(SuperColor.Orange.Get());
                } else {
                    _value = _rawValue.ToString().Color(SuperColor.Green.Get());
                }
            } catch (Exception) {
                _value = "ERROR".Color(SuperColor.Crimson.Get());
            }

            return _value;
        }
        #endregion

        #region Wrappers
        private abstract class Watcher : IComparable, IComparable<Watcher> {
            #region Global Members
            public readonly string Description = string.Empty;
            public bool Enabled = false;
            public int Order = 0;

            public string Type {
                get { return GetType().Name.Replace("Watcher", string.Empty); }
            }

            // -----------------------

            public Watcher(string _description, bool _enabled, int _order = 0) {
                Description = _description;
                Enabled = _enabled;
                Order = _order;
            }
            #endregion

            #region Comparison
            public int CompareTo(object _object) {
                if (_object is Watcher _watcher) {
                    return CompareTo(_watcher);
                }

                return 0;
            }

            public int CompareTo(Watcher _other) {
                return Order.CompareTo(_other.Order);
            }
            #endregion

            #region Behaviour
            public abstract object GetValue(Evaluator _evaluator);

            public abstract override string ToString();
            #endregion
        }

        private sealed class EvaluatorWatcher : Watcher {
            #region Global Members
            public readonly string Expression = string.Empty;

            // -----------------------

            public EvaluatorWatcher(string _expression, string _description, bool _enabled, int _order = 0) : base(_description, _enabled, _order) {
                Expression = _expression;
            }
            #endregion

            #region Behaviour
            public override object GetValue(Evaluator _evaluator) {
                return _evaluator.Evaluate(Expression);
            }

            public override string ToString() {
                return Expression;
            }
            #endregion
        }

        private sealed class LambdaWatcher : Watcher {
            #region Global Members
            private readonly Func<object> getter;

            // -----------------------

            public LambdaWatcher(Func<object> _getter, string _description, bool _enabled, int _order = 0) : base(_description, _enabled, _order) {
                getter = _getter;
            }
            #endregion

            #region Behaviour
            public override object GetValue(Evaluator _evaluator) {
                return getter.Invoke();
            }

            public override string ToString() {
                return string.Empty;
            }
            #endregion
        }

        private sealed class FieldWatcher : Watcher {
            #region Global Members
            private readonly FieldInfo field = null;

            // -----------------------

            public FieldWatcher(FieldInfo _field, string _description, bool _enabled, int _order = 0) : base(_description, _enabled, _order) {
                field = _field;
            }
            #endregion

            #region Behaviour
            public override object GetValue(Evaluator _evaluator) {
                return field.GetValue(null);
            }

            public override string ToString() {
                return field.FieldType.Name;
            }
            #endregion
        }

        private sealed class PropertyWatcher : Watcher {
            #region Global Members
            private readonly PropertyInfo property = null;

            // -----------------------

            public PropertyWatcher(PropertyInfo _property, string _description, bool _enabled, int _order = 0) : base(_description, _enabled, _order) {
                property = _property;
            }
            #endregion

            #region Behaviour
            public override object GetValue(Evaluator _evaluator) {
                return property.GetValue(null);
            }

            public override string ToString() {
                return property.PropertyType.Name;
            }
            #endregion
        }
        #endregion
    }
}
