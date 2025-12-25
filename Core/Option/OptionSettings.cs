// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core.Settings;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;

using Range = EnhancedEditor.RangeAttribute;

[assembly: InternalsVisibleTo("EnhancedFramework.Editor")]
namespace EnhancedFramework.Core.Option {
    /// <summary>
    /// <see cref="OptionSettings"/>-related option base class.
    /// <br/> Inherit from this class to create your own options.
    /// </summary>
    [Serializable]
    public abstract class BaseGameOption {
        #region Global Members
        [SerializeField, Enhanced, ReadOnly] internal int guid = EnhancedUtility.GenerateGUID();
        [SerializeField] internal string name = "New Option";

        // -----------------------

        /// <summary>
        /// Identifier name of this option.
        /// </summary>
        public string Name {
            get { return name; }
        }

        /// <summary>
        /// Unique GUID of this option.
        /// </summary>
        public int GUID {
            get { return guid; }
        }

        /// <summary>
        /// Raw value of this option selection.
        /// </summary>
        public virtual float SelectedValue {
            get { throw new GameOptionException("Get Selection"); }
            set { throw new GameOptionException("Set Selection"); }
        }

        /// <summary>
        /// Index of this option selected value.
        /// </summary>
        public virtual int SelectedValueIndex {
            get { throw new GameOptionException("Get Selection"); }
            set { throw new GameOptionException("Set Selection"); }
        }

        /// <summary>
        /// Total count of availble choice for this option.
        /// </summary>
        public virtual int AvailableOptionCount {
            get { throw new GameOptionException("Option Count"); }
        }

        /// <summary>
        /// Default string displaying this option selected value.
        /// </summary>
        public virtual string SelectedValueString {
            get { throw new GameOptionException("Selection String"); }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="BaseGameOption"/>
        public BaseGameOption() { }

        /// <param name="_name"><inheritdoc cref="Name" path="/summary"/></param>
        /// <param name="_guid"><inheritdoc cref="GUID" path="/summary"/></param>
        /// <inheritdoc cref="BaseGameOption"/>
        public BaseGameOption(string _name, int _guid) {
            name = _name;
            guid = _guid;
        }
        #endregion

        #region Option
        /// <summary>
        /// Applies this option value(s).
        /// </summary>
        public abstract void Apply(bool _isInit = false);

        /// <summary>
        /// Refreshes this option value(s) from the game current state.
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// Initializes this option with the default option settings and references.
        /// </summary>
        /// <param name="_option">Default option.</param>
        public abstract void Initialize(BaseGameOption _option);
        #endregion
    }

    // -------------------------------------------
    // Options
    // -------------------------------------------

    /// <summary>
    /// Default <see cref="BaseGameOption"/> empty class.
    /// </summary>
    [Serializable, DisplayName("<Default>")]
    public sealed class DefaultGameOption : BaseGameOption {
        #region Behaviour
        public override void Apply(bool _isInit = false) { }

        public override void Refresh() { }

        public override void Initialize(BaseGameOption _option) { }
        #endregion
    }

    /// <summary>
    /// <see cref="BaseGameOption"/> used to save a specific <see cref="AudioMixer"/> volume.
    /// </summary>
    [Serializable, DisplayName("Audio/Volume")]
    public sealed class AudioVolumeOption : BaseGameOption {
        private const float MinVolume = .0001f;

        #region Global Members
        [Space(10f)]

        [Tooltip("Audio Mixer to control a volume from")]
        [SerializeField, Enhanced, Required] private AudioMixer audioMixer = null;

        [Tooltip("Name of the volume to control")]
        [SerializeField] private string audioGroupName = "Master Volume";

        [Space(10f)]

        [Tooltip("Value of this volume")]
        [SerializeField, Enhanced, Range(MinVolume, 1f)] private float volume = 1f;

        [Tooltip("Total amount of available volume steps")]
        [SerializeField, Enhanced, Range(1f, 100f)] private int stepCount = 10;

        [Space(10f)]

        [Tooltip("Whether this audio is currently mute or not")]
        [SerializeField] private bool isMute = false;

        // -----------------------

        /// <summary>
        /// Value of this volume option (between 0 and 1).
        /// </summary>
        public float Volume {
            get { return volume; }
            set {
                volume = Mathf.Clamp(value, MinVolume, 1f);
                Apply();
            }
        }

        /// <summary>
        /// Whether this audio is currently mute or not.
        /// </summary>
        public bool IsMute {
            get { return isMute; }
        }

        // -----------------------

        public override int SelectedValueIndex {
            get {

                float _value = Volume - MinVolume;
                float _max   = 1f     - MinVolume;

                float _percent = (_value == 0f) ? 0f : (_value / _max);
                return Mathf.RoundToInt(_percent * (stepCount - 1));
            }
            set {

                float _percent = (value == 0) ? 0f : (value / (stepCount - 1f));
                //this.LogMessage("Selected => " + _percent + " - " + (value / (stepCount - 1f)) + " - " + value);
                Volume = Mathf.Lerp(MinVolume, 1f, _percent);
            }
        }

        public override int AvailableOptionCount {
            get { return stepCount; }
        }

        public override string SelectedValueString {
            get { return SelectedValueIndex.ToString(); }
        }
        #endregion

        #region Behaviour
        public override void Apply(bool _isInit = false) {

            float _volume = isMute  ? MinVolume : volume;
            float _value  = _volume - MinVolume;
            float _max    = 1f      - MinVolume;

            float _percent = (_value == 0f) ? 0f : (_value / _max);

            _value = Mathf.Lerp(MinVolume, 1f, _percent);
            _value = Mathf.Log10(_value);
            _value *= 20f;

            audioMixer.SetFloat(audioGroupName, _value);

            //audioMixer.GetFloat(audioGroupName, out float _v);
            //this.LogMessage("Apply Volume => " + _value + " - " + volume + " - " + SelectedValueIndex + " | " + _v);
        }

        public override void Refresh() {

            // Read mixer float value.
            if (audioMixer.GetFloat(audioGroupName, out float _volume)) {

                //float _v = _volume;

                _volume /= 20f;
                _volume  = Mathf.Pow(10f, _volume);
                //_volume = Mathf.Lerp(MinVolume, 1f, _volume);

                Volume = _volume;

                //this.LogMessage("Refresh Volume => " + _volume + " - " + _v);
            }
        }

        public override void Initialize(BaseGameOption _option) {

            if (_option is not AudioVolumeOption _audio) {
                return;
            }

            audioMixer      = _audio.audioMixer;
            audioGroupName  = _audio.audioGroupName;
            stepCount       = _audio.stepCount;

            isMute = false;
        }

        // -----------------------

        /// <summary>
        /// Mutes/Unmutes this audio.
        /// </summary>
        /// <param name="_isMute">True to mute this audio, false to unmute it.</param>
        public void Mute(bool _isMute) {
            isMute = _isMute;
            Apply();
        }
        #endregion
    }

    /// <summary>
    /// <see cref="BaseGameOption"/> used to set the game <see cref="FullScreenMode"/>.
    /// </summary>
    [Serializable, DisplayName("General/Full Screen Mode")]
    public sealed class FullScreenModeOption : BaseGameOption {
        #region Global Members
        [Space(10f)]

        [Tooltip("Game full screen mode")]
        [SerializeField] private FullScreenMode mode = FullScreenMode.FullScreenWindow;

        // -----------------------

        /// <summary>
        /// Game full screen mode.
        /// </summary>
        public FullScreenMode Mode {
            get { return mode; }
            set {
                mode = value;
                Apply();
            }
        }

        // -----------------------

        public override int SelectedValueIndex {
            get { return (int)Mode; }
            set {

                #if UNITY_STANDALONE_WIN
                // Ignore Mac setting.
                if (value == 2) {
                    value += (SelectedValueIndex == 3) ? -1 : 1;
                }
                #endif

                Mode = (FullScreenMode)value;
            }
        }

        public override int AvailableOptionCount {
            get { return 4; }
        }

        public override string SelectedValueString {
            get { return Mode.ToString(); }
        }
        #endregion

        #region Behaviour
        public override void Apply(bool _isInit = false) {
            if (_isInit) {
                Refresh();
                return;
            }

            Screen.fullScreenMode = Mode;
        }

        public override void Refresh() {
            Mode = Screen.fullScreenMode;
        }

        public override void Initialize(BaseGameOption _option) { }
        #endregion
    }

    // -------------------------------------------
    // Settings
    // -------------------------------------------

    /// <summary>
    /// Game option wrapper.
    /// </summary>
    [Serializable]
    public sealed class GameOptionsWrapper {
        #region Global Members
        [SerializeReference] public BaseGameOption[] Options = new BaseGameOption[0];

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Prevents from creating any new instance of this class.
        /// </summary>
        internal GameOptionsWrapper() { }
        #endregion
    }

    /// <summary>
    /// Game option related settings, saved on disk.
    /// </summary>
    public sealed class OptionSettings : BaseSettings<OptionSettings> {
        #region Global Members
        [Section("Option Settings")]

        [Tooltip("Path where to write these settings on disk")]
        [SerializeField] private FilePath path = Core.FilePath.PersistentPath;

        [Tooltip("Name of the option file to write on disk (including extension)")]
        [SerializeField] private string fileName = "GameOption.ini";

        [Space(10f)]

        [Tooltip("All game Scriptable Object option wrappers")]
        [SerializeField] internal ScriptableGameOption[] scriptableOptions = new ScriptableGameOption[0];

        // -----------------------

        [NonSerialized] private GameOptionsWrapper option = new GameOptionsWrapper();

        // -----------------------

        /// <summary>
        /// Total amount of <see cref="BaseGameOption"/> in these settings.
        /// </summary>
        public int Count {
            get { return option.Options.Length; }
        }

        /// <summary>
        /// These option settings file path.
        /// </summary>
        public string FilePath {
            get { return Path.Combine(path.Get(true), fileName); }
        }
        #endregion

        #region Initialization
        protected internal override void Init() {
            base.Init();
            InitOption();
        }
        #endregion

        #region Option
        /// <summary>
        /// Get a specific <see cref="BaseGameOption"/> from its registered guid and name.
        /// </summary>
        /// <param name="_guid">The guid to get the associated option.</param>
        /// <param name="_name">The name of the option to get.</param>
        /// <param name="_creator">Called to create this option if not found.</param>
        /// <returns>The option associated with the given guid and name.</returns>
        public BaseGameOption GetOption(int _guid, string _name, ScriptableGameOption _gameOption) {

            ref BaseGameOption[] _options = ref option.Options;
            int _index = GeOptionIndex(ref _options);

            if (_index != -1) {
                return _options[_index];
            }

            BaseGameOption _option = _gameOption.CreateOption();

            _option.name = _name;
            _option.guid = _guid;

            ArrayUtility.Add(ref _options, _option);
            return _option;

            // ----- Local Method ----- \\

            int GeOptionIndex(ref BaseGameOption[] _options) {

                for (int i = _options.Length; i-- > 0;) {
                    BaseGameOption _option = _options[i];

                    if ((_option.GUID == _guid) && _option.Name.Equals(_name, StringComparison.OrdinalIgnoreCase)) {
                        return i;
                    }
                }

                return -1;
            }
        }

        /// <summary>
        /// Get the <see cref="BaseGameOption"/> at the given index.
        /// <br/> Use <see cref="Count"/> to get the total amount of <see cref="BaseGameOption"/> in these settings.
        /// </summary>
        public BaseGameOption GetOption(int _index) {
            return option.Options[_index];
        }

        /// <summary>
        /// Applies all game option values.
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.Crimson, IsDrawnOnTop = false)]
        public void InitOption() {
            // Loading and initialization.
            Load();

            ref ScriptableGameOption[] _span = ref scriptableOptions;
            int _count = _span.Length;

            for (int i = 0; i < _count; i++) {
                _span[i].Initialize(this);
            }

            Apply(true);
            Save();
        }

        /// <summary>
        /// Applies all game option values.
        /// </summary>
        [Button(SuperColor.Green, IsDrawnOnTop = false)]
        public void Apply(bool _isInit = false) {
            ref BaseGameOption[] _options = ref option.Options;
            int _count = _options.Length;

            for (int i = 0; i < _count; i++) {
                _options[i].Apply(_isInit);
            }
        }

        /// <summary>
        /// Refreshes all game option value from the current game state.
        /// </summary>
        [Button(SuperColor.HarvestGold, IsDrawnOnTop = false)]
        public void Refresh() {
            ref BaseGameOption[] _options = ref option.Options;
            int _count = _options.Length;

            for (int i = 0; i < _count; i++) {
                _options[i].Refresh();
            }
        }
        #endregion

        #region File
        /// <summary>
        /// Loads these option data from disk.
        /// </summary>
        public void Load() {
            string _filePath = FilePath;

            // Read data from file.
            if (File.Exists(_filePath)) {
                string _json = File.ReadAllText(_filePath);

                try {

                    GameOptionsWrapper _wrapper = new GameOptionsWrapper();
                    JsonUtility.FromJsonOverwrite(_json, _wrapper);

                    option = _wrapper;

                } catch (Exception e) {
                    this.LogException(e);
                }
            }
        }

        /// <summary>
        /// Saves these option data on disk.
        /// </summary>
        public void Save() {

            string _filePath = FilePath;
            string _json = JsonUtility.ToJson(option, true);

            File.WriteAllText(_filePath, _json);
        }

        /// <summary>
        /// Clears and deletes these option file from disk.
        /// </summary>
        [Button(SuperColor.Crimson, IsDrawnOnTop = false)]
        public void Delete() {

            string _filePath = FilePath;

            if (File.Exists(_filePath)) {
                File.Delete(_filePath);
            }
        }

        /// <summary>
        /// Opens the game option directory.
        /// </summary>
        [Button(SuperColor.Green, IsDrawnOnTop = false)]
        public void OpenDirectory() {

            string _filePath = Path.GetDirectoryName(FilePath);

            if (Directory.Exists(_filePath)) {
                Application.OpenURL(_filePath);
            }
        }
        #endregion
    }

    /// <summary>
    /// Exception raised when a <see cref="GameOptionException"/> is not compatible.
    /// </summary>
    public sealed class GameOptionException : Exception {
        #region Global Members
        public const string MessageFormat = "Game Option incompatible with this request \'{0}\'";

        // -----------------------

        /// <inheritdoc cref="MissingCrossCeneReferenceException(string, Exception)"/>
        public GameOptionException() : base(string.Format(MessageFormat, "[Unknown]")) { }

        /// <inheritdoc cref="MissingCrossCeneReferenceException(string, Exception)"/>
        public GameOptionException(string _guid) : base(string.Format(MessageFormat, _guid)) { }

        /// <param name="_guid">Guid of the missing cross scene object.</param>
        /// <inheritdoc cref="MissingCrossCeneReferenceException"/>
        public GameOptionException(string _guid, Exception _innerException) : base(string.Format(MessageFormat, _guid), _innerException) { }
        #endregion
    }
}
