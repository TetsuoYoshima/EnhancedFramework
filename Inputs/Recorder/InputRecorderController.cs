// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if ENABLE_INPUT_SYSTEM
#define NEW_INPUT_SYSTEM
#endif

#if INPUT_RECORDER
#define RECORDER
#endif

#if !UNITY_EDITOR
#define BUILD_BEHAVIOUR
#endif

#if NEW_INPUT_SYSTEM
using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Inputs {
    /// <summary>
    /// <see cref="InputRecorder"/> controller class, used to automatically record the game or replay saved content.
    /// </summary>
    [ScriptGizmos(false, true)]
    [DefaultExecutionOrder(100)] // Execute late to avoid machine slowdowns on process.
    [RequireComponent(typeof(InputRecorder))]
    [ScriptingDefineSymbol("INPUT_RECORDER", "Input Recording in both Editor and Builds")]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Input/Input Recorder Controller"), DisallowMultipleComponent]
    #pragma warning disable
    public sealed class InputRecorderController : EnhancedBehaviour, ILoadingProcessor {
        #region Quit Build Behaviour
        /// <summary>
        /// Behaviour when quitting a build.
        /// </summary>
        public enum QuitBuildBehaviour {
            None = 0,

            OpenDirectory = 1,
            OpenDirectoryAndReadme = 2,
        }
        #endregion

        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init;

        #region Loading Processor
        public override bool IsLoadingProcessor => true;

        public bool IsProcessing {
            get { return isProcessing; }
        }
        #endregion

        #region Global Members
        [Section("Input Recorder Controller")]

        [Tooltip("Input recorder instance to control")]
        [SerializeField, Enhanced, Required] private InputRecorder inputRecorder = null;

        [Space(10f)]

        [Tooltip("Input used to dynamically start / stop input recording")]
        [SerializeField] private InputActionEnhancedAsset recordInput = null;

        [Tooltip("Input used to dynamically start / stop input replay")]
        [SerializeField] private InputActionEnhancedAsset replayInput = null;

        [Tooltip("Input used to automatically load and replay last capture during process")]
        [SerializeField] private InputActionEnhancedAsset replayCaptureInput = null;

        [Space(10f)]

        [Tooltip("If true, automatically starts input recording on game initialization")]
        [SerializeField] private bool startRecordOnInit = false;

        [Tooltip("If true, automatically records inputs and save them on disk in build")]
        [SerializeField] private bool autoRecordBuild = false;

        [Space(10f)]

        [Tooltip("Behaviour to execute when quitting a build exe")]
        [SerializeField, Enhanced, ShowIf(nameof(autoRecordBuild))] private QuitBuildBehaviour quitBuildBehaviour = QuitBuildBehaviour.None;

        [Tooltip("Selected capture to replay using the button below")]
        [SerializeField, Enhanced, Popup(nameof(GetCaptureFileNames)), DisplayName("Capture")] private int selectedCapture = 0;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Total duration if this controller processing operation (in seconds)")]
        [SerializeField, Enhanced, Range(0f, 30f)] private float processDuration = 1f;

        [Tooltip("Whether this object is currently still processing or not")]
        [SerializeField, Enhanced, ReadOnly] private bool isProcessing = true;

        [Space(10f)]

        [Tooltip("Path where to write the records on disk")]
        [SerializeField] private FilePath path = FilePath.PersistentPath;

        // -----------------------

        /// <summary>
        /// Input used to dynamically start / stop input record.
        /// </summary>
        public InputActionEnhancedAsset RecordInput {
            get { return recordInput; }
        }

        /// <summary>
        /// Input used to dynamically start / stop input replay.
        /// </summary>
        public InputActionEnhancedAsset ReplayInput {
            get { return replayInput; }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {

            // Disable object if record is not enable.
            #if !RECORDER
            inputRecorder.enabled = false;
            enabled = false;

            return;
            #endif

            base.OnBehaviourEnabled();

            // Inputs.
            EnableInput(recordInput, true);
            EnableInput(replayInput, true);
        }

        protected override void OnInit() {
            base.OnInit();

            // Process.
            isProcessing = true;
            Delayer.Call(processDuration, OnProcessComplete, true);

            // Inputs.
            InitInput(recordInput, OnRecordInput);
            InitInput(replayInput, OnReplayInput);

            InitInput(replayCaptureInput, OnAutoReplayInput);
            EnableInput(replayCaptureInput, true);

            // ----- Local Method ----- \\

            static void InitInput(InputActionEnhancedAsset _input, Action<InputActionEnhancedAsset> _callback) {

                if (_input.IsValid()) {
                    _input.OnPerformed += _callback;
                }
            }
        }

        protected override void OnBehaviourDisabled() {

            #if !RECORDER
            return;
            #endif

            base.OnBehaviourDisabled();

            // Inputs.
            EnableInput(recordInput, false);
            EnableInput(replayInput, false);
            EnableInput(replayCaptureInput, false);
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        protected override void OnValidate() {
            base.OnValidate();

            // Reference.
            if (!inputRecorder) {
                inputRecorder = GetComponent<InputRecorder>();
            }
        }
        #endif
        #endregion

        #region Process
        private bool autoReplay = false;

        // -----------------------

        /// <summary>
        /// Called after object process is completed.
        /// <br/> Initializes this object and setup automatic behaviour.
        /// </summary>
        private void OnProcessComplete() {

            // Input.
            EnableInput(replayCaptureInput, false);

            isProcessing = false;

            #if UNITY_EDITOR
            // Load selected capture.
            if (SessionState.GetBool(ReplayKey, false)) {

                SessionState.SetBool(ReplayKey, false);
                LoadCapture(selectedCapture);

                return;
            }
            #endif

            // Load record.
            if (autoReplay && LoadCapture(0)) {
                return;
            }

            // Build record.
            #if BUILD_BEHAVIOUR
            if (autoRecordBuild) {

                Application.wantsToQuit += OnWantsToQuit;
                inputRecorder.StartRecord();

                return;
            }
            #endif

            // Init.
            if (startRecordOnInit) {

                Application.wantsToQuit += OnWantsToQuit;
                inputRecorder.StartRecord();
            }
        }

        /// <summary>
        /// Called on build before quitting the application.
        /// Used to save current record on disk.
        /// </summary>
        /// <returns>True to continue the qui process, false to cancel it.</returns>
        private bool OnWantsToQuit() {

            SaveRecord();

            #if BUILD_BEHAVIOUR
            switch (quitBuildBehaviour) {

                // Directory.
                case QuitBuildBehaviour.OpenDirectory:
                    OpenFileDirectory();
                    break;

                // Readme.
                case QuitBuildBehaviour.OpenDirectoryAndReadme:
                    OpenFileDirectory();
                    Application.OpenURL(ReadmePath);
                    break;

                case QuitBuildBehaviour.None:
                default:
                    break;
            }
            #endif

            return true;
        }
        #endregion

        #region Record Files
        private const string CaptureFileExtension = InputRecorder.InputRecordFileExtension;
        private const string CaptureFolder  = "Record";
        private const string ReadmeName     = "README.txt";
        private const string ReplayKey      = "ReplayCapture";

        private const string ReadmeContent = "Congratulations, and thank you for taking the time to play the game!\n\n" +
                                             "All of your games are automatically recorded and store in this directory.\n" +
                                             "Please, keep in mind to contact a developer and send them the files located in this folder!\n\n" +
                                             "Have a lovely day, and thank you again for your time!";

        private readonly List<string> captureFiles = new List<string>();
        private List<string> captureFileNames = new List<string>();

        [NonSerialized] private bool initializedCaptures = false;

        /// <summary>
        /// Folder path where all captures are saved and loaded on disk.
        /// </summary>
        public string CapturePath {
            get {
                return path.Get(CaptureFolder, true);
            }
        }

        /// <summary>
        /// Path of the record readme file.
        /// </summary>
        public string ReadmePath {
            get {
                return Path.Combine(CapturePath, ReadmeName);
            }
        }

        /// <summary>
        /// Generates a name for writing a new capture on disk.
        /// </summary>
        public string CaptureName {
            get { return $"{Environment.UserName}-{DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss")}.{GameManager.Instance.Seed}.{inputRecorder.FirstCaptureEventID}.{CaptureFileExtension}"; }
        }

        /// <summary>
        /// Indicates if a capture can currently be replayed.
        /// </summary>
        private bool CanReplayCapture {
            get { return captureFiles.Count != 0; }
        }

        // -----------------------

        /// <summary>
        /// Loads the main capture saved file from this machine.
        /// </summary>
        /// <returns>True if the capture could be successfully loaded, false otherwise.</returns>
        private bool LoadCapture(int _captureIndex) {

            // Capture.
            RefreshCaptures();
            List<string> _files = GetCaptureFiles();

            if (_files.Count <= _captureIndex) {
                return false;
            }

            // Parse.
            string _file = _files[_captureIndex];
            string _name = Path.GetFileNameWithoutExtension(_file);

            string[] _splits = _name.Split('.');

            // Seed.
            if ((_splits.Length < 1) || !int.TryParse(_splits[_splits.Length - 2], out int _seed)) {

                this.LogErrorMessage("Seed could not be retrieved from saved file");
                return false;
            }

            // Event ID.
            if (!int.TryParse(_splits[_splits.Length - 1], out int _eventID)) {
                _eventID = 0;
            }

            // Load file.
            if (!inputRecorder.LoadRecordFromFile(_file)) {
                return false;
            }

            // Replay.
            GameManager.Instance.SetSeed(_seed);

            inputRecorder.FirstCaptureEventID = _eventID;
            inputRecorder.StartReplay();

            return true;
        }

        /// <summary>
        /// Saves the current capture on disk.
        /// </summary>
        private void SaveRecord() {

            // Readme.
            string _readmePath = ReadmePath;
            File.WriteAllText(_readmePath, ReadmeContent);

            // Capture.
            string _path = Path.Combine(CapturePath, CaptureName);

            // Save.
            inputRecorder.StopRecord();
            inputRecorder.SaveRecordToFile(_path);
        }

        /// <summary>
        /// Replay the selected capture in the editor.
        /// </summary>
        [Button(ActivationMode.Editor, "CanReplayCapture", ConditionType.True, SuperColor.HarvestGold,
                IsDrawnOnTop = false, Order = 0, Tooltip = "Enter play mode and replay the selected capture")]
        private void ReplayCapture() {

            #if UNITY_EDITOR
            SessionState.SetBool(ReplayKey, true);
            EditorApplication.EnterPlaymode();
            #endif
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get all cached capture files on disk.
        /// </summary>
        /// <returns>The path of all cache capture files on disk.</returns>
        public List<string> GetCaptureFiles() {

            // Captures initialization.
            if (!initializedCaptures) {

                string _path = CapturePath;
                string[] _files = Directory.GetFiles(_path, $"*.{CaptureFileExtension}", SearchOption.TopDirectoryOnly);

                captureFiles.Clear();
                captureFiles.AddRange(_files);

                captureFiles.Sort((a, b) => {
                    return File.GetCreationTime(b).CompareTo(File.GetCreationTime(a));
                });

                captureFileNames = captureFiles.ConvertAll(f => Path.GetFileNameWithoutExtension(f));

                initializedCaptures = true;
            }

            return captureFiles;
        }

        /// <summary>
        /// Get the name of all cached capture files on disk.
        /// </summary>
        /// <returns>The name of all cache capture files on disk.</returns>
        public List<string> GetCaptureFileNames() {

            GetCaptureFiles();
            return captureFileNames;
        }

        /// <summary>
        /// Refreshes cached capture files.
        /// </summary>
        [ContextMenu("Refresh Capture(s)", false, 50)]
        private void RefreshCaptures() {
            initializedCaptures = false;
        }

        /// <summary>
        /// Opens in the explorer the directory where all captures are stored.
        /// </summary>
        [ContextMenu("Open Capture Directory", false, 51)]
        private void OpenFileDirectory() {
            Application.OpenURL(CapturePath);
        }
        #endregion

        #region Input
        /// <summary>
        /// Called when the record input is being performed.
        /// </summary>
        /// <param name="_input">Record input asset.</param>
        private void OnRecordInput(InputActionEnhancedAsset _input) {

            InputRecorder _recorder = inputRecorder;

            if (_recorder.IsRecording) {
                _recorder.StopRecord();
            } else {
                _recorder.StartRecord();
            }
        }

        /// <summary>
        /// Called when the replay input is being performed.
        /// </summary>
        /// <param name="_input">Replay input asset.</param>
        private void OnReplayInput(InputActionEnhancedAsset _input) {

            InputRecorder _recorder = inputRecorder;

            if (_recorder.IsReplaying) {
                _recorder.StopReplay();
            } else {
                _recorder.StartRecord();
            }
        }

        /// <summary>
        /// Called when the auto replay input is being performed.
        /// </summary>
        /// <param name="_input">Auto replay input asset.</param>
        private void OnAutoReplayInput(InputActionEnhancedAsset _input) {
            autoReplay = true;
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Enables or disables a specific input.
        /// </summary>
        /// <param name="_input">Input to enable.</param>
        /// <param name="_enabled">Whether to enable or not this input.</param>
        private void EnableInput(InputActionEnhancedAsset _input, bool _enabled) {

            if (_input.IsValid()) {
                _input.IsEnabled = _enabled;
            }
        }
        #endregion
    }
}
#endif
