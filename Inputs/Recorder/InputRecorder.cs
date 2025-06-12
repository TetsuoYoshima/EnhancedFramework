// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if ENABLE_INPUT_SYSTEM
#define NEW_INPUT_SYSTEM
#endif

#if NEW_INPUT_SYSTEM
using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Inputs {
    /// <summary>
    /// <see cref="EnhancedRecorderDevice"/>-related <see cref="IInputStateTypeInfo"/>.
    /// </summary>
    internal struct EnhancedRecorderDeviceState : IInputStateTypeInfo {
        #region Global Members
        public readonly FourCC format {
            get { return new FourCC('E', 'H', 'C', 'D'); }
        }

        /// <summary>
        /// Dummy field used to compile this struct.
        /// </summary>
        [InputControl(name = "button", layout = "Button", bit = 3)]
        public ushort button;
        #endregion
    }

    /// <summary>
    /// <see cref="InputRecorder"/>-related <see cref="InputDevice"/>, used to send events about the current capture.
    /// </summary>
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    [InputControlLayout(displayName = "Enhanced Recorder", stateType = typeof(EnhancedRecorderDeviceState))]
    internal sealed class EnhancedRecorderDevice : InputDevice {
        #region Global Members
        /// <summary>
        /// Instance of this <see cref="InputDevice"/>.
        /// </summary>
        public static readonly EnhancedRecorderDevice Instance = InputSystem.AddDevice<EnhancedRecorderDevice>();

        /// <summary>
        /// Dummy button.
        /// </summary>
        public ButtonControl Button { get; private set; }

        static EnhancedRecorderDevice() {
            InputSystem.RegisterLayout<EnhancedRecorderDevice>("Enhanced Device");
        }

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeInPlayer() { }

        // -----------------------

        protected override void FinishSetup() {
            base.FinishSetup();

            Button = GetChildControl<ButtonControl>("button");
        }
        #endregion
    }

    /// <summary>
    /// Wrapper for a single input capture data value.
    /// </summary>
    [Serializable]
    public struct InputCaptureSingleDataObject {
        #region Content
        public Quaternion Quaternion;
        public Vector3    Vector;
        public float      Float;
        public string     String;
        #endregion
    }

    /// <summary>
    /// Wrapper for a custom input data.
    /// </summary>
    [Serializable]
    public sealed class InputCaptureData {
        #region Global Members
        /// <summary>
        /// Unique identifier of this input data.
        /// </summary>
        public int ID = 0;

        /// <summary>
        /// Index of the current capture replay data.
        /// </summary>
        public int ReplayIndex = 0;

        /// <summary>
        /// All registered data in this wrapper.
        /// </summary>
        public PairCollection<double, InputCaptureSingleDataObject> Data = new PairCollection<double, InputCaptureSingleDataObject>();

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <param name="_id"><inheritdoc cref="ID" path="/summary"/></param>
        /// <inheritdoc cref="InputCaptureData"/>
        public InputCaptureData(int _id) {
            ID = _id;
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Records a entry for this data.
        /// </summary>
        /// <param name="_time">Current record time.</param>
        /// <param name="_id">ID of the data to record.</param>
        /// <param name="_data">Data value.</param>
        /// <returns>True if the data was recorded, false otherwise.</returns>
        public bool Record(double _time, int _id, InputCaptureSingleDataObject _data) {

            if (ID != _id) {
                return false;
            }

            Data.Add(_time, _data);
            return true;
        }

        /// <summary>
        /// Replays recorded data for this frame.
        /// </summary>
        /// <param name="_recorderData"><see cref="IInputRecorderData"/> to replay.</param>
        /// <param name="_time">Current game time.</param>
        /// <param name="_timeRuntime">Record runtime.</param>
        /// <param name="_timeStartEvent">Record first event time.</param>
        /// <returns>True if the data was replayed, false otherwise.</returns>
        public bool Replay(IInputRecorderData _recorderData, double _time, double _timeRuntime, double _timeStartEvent) {

            if (_recorderData.InputDataID != ID) {
                return false;
            }

            //m_AllEventsByTime[m_AllEventsByTimeIndex + 1].internalTime > m_StartTimeAsPerFirstEvent + (InputRuntime.s_Instance.currentTime - m_StartTimeAsPerRuntime)

            while ((ReplayIndex < Data.Count) && (Data[ReplayIndex].First < (_timeStartEvent + (_time - _timeRuntime)))) {

                _recorderData.Replay(Data[ReplayIndex].Second);
                ReplayIndex++;
            }

            return true;
        }
        #endregion
    }

    /// <summary>
    /// Holder for all input data from a single capture.
    /// </summary>
    [Serializable]
    public sealed class InputCaptureDataHolder {
        #region Global Members
        /// <summary>
        /// All data from this input capture.
        /// </summary>
        [SerializeField] public EnhancedCollection<InputCaptureData> Data = new EnhancedCollection<InputCaptureData>();
        #endregion

        #region Behaviour
        /// <summary>
        /// Records a specific input data.
        /// </summary>
        /// <param name="_time">Current record time.</param>
        /// <param name="_id">ID of the data to record.</param>
        /// <param name="_data">Data value.</param>
        public void RecordData(double _time, int _id, InputCaptureSingleDataObject _data) {

            List<InputCaptureData> _dataSpan = Data.collection;
            int _dataCount = _dataSpan.Count;

            for (int i = 0; i < _dataCount; i++) {

                if (_dataSpan[i].Record(_time, _id, _data)) {
                    return;
                }
            }

            InputCaptureData _capture = new InputCaptureData(_id);
            _capture.Record(_time, _id, _data);

            Data.Add(_capture);
        }

        /// <summary>
        /// Replays recorded data for this frame.
        /// </summary>
        /// <param name="_recorderData"><see cref="IInputRecorderData"/> to replay.</param>
        /// <param name="_time">Current game time.</param>
        /// <param name="_timeRuntime">Record runtime.</param>
        /// <param name="_timeStartEvent">Record first event time.</param>
        public void ReplayData(IInputRecorderData _recorderData, double _time, double _timeRuntime, double _timeStartEvent) {

            List<InputCaptureData> _dataSpan = Data.collection;
            int _dataCount = _dataSpan.Count;

            for (int i = 0; i < _dataCount; i++) {

                if (_dataSpan[i].Replay(_recorderData, _time, _timeRuntime, _timeStartEvent)) {
                    return;
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Inherit from this interface to record and replay your own data bind to input captures.
    /// </summary>
    public interface IInputRecorderData {
        #region Content
        /// <summary>
        /// Unique identifier of this object related data.
        /// </summary>
        int InputDataID { get; }

        /// <summary>
        /// Records an input data for this frame.
        /// </summary>
        /// <param name="_data">This frame input data.</param>
        /// <returns>True if a data should be recorded, false otherwise.</returns>
        bool Record(out InputCaptureSingleDataObject _data);

        /// <summary>
        /// Replays a previously recorded input data.
        /// </summary>
        /// <param name="_data">Input data to replay.</param>
        void Replay(InputCaptureSingleDataObject _data);
        #endregion
    }

    /// <summary>
    /// <see cref="InputEventTrace"/> wrapper used to record and replay game inputs.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Input/Input Recorder"), DisallowMultipleComponent]
    #pragma warning disable
    public sealed class InputRecorder : EnhancedBehaviour, IStableUpdate {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Stable;

        #region State
        /// <summary>
        /// All available states of this object.
        /// </summary>
        public enum State {
            Inactive = 0,

            Record   = 1,
            Replay   = 2
        }
        #endregion

        #region Global Members
        public const string InputRecordFileExtension = "inputtrace";

        [Section("Input Recorder")]

        [Tooltip("If enabled, additional events will be recorded that demarcate frame boundaries. When replaying, this allows " +
                 "spacing out input events across frames corresponding to the original distribution across frames when input was " +
                 "recorded. If this is turned off, all input events will be queued in one block when replaying the trace.")]
        [SerializeField] private bool recordFrames = true;

        [Tooltip("If enabled, only StateEvents and DeltaStateEvents will be captured")]
        [SerializeField] private bool recordStateEventsOnly = false;

        [Space(5f)]

        [Tooltip("If enabled, new devices will be created for captured events when replaying them\n" +
                 "If disabled (default), events will be queued as is and thus keep their original device ID")]
        [SerializeField] private bool replayOnNewDevices = false;

        [Tooltip("If enabled, the system will try to simulate the original event timing on replay. This differs from replaying frame " +
                 "by frame in that replay will try to compensate for differences in frame timings and redistribute events to frames that " +
                 "more closely match the original timing. Note that this is not perfect and will not necessarily create a 1:1 match.")]
        [SerializeField] private bool simulateOriginalTimingOnReplay = true;

        [Space(5f)]

        [Tooltip("Toggles meta data recording")]
        [SerializeField] private bool recordMetaData = true;

        [Space(10f)]

        [Tooltip("Group displayed on screen while recording")]
        [SerializeField, Enhanced, Required] private FadingObjectBehaviour recordGroup = null;

        [Tooltip("Group displayed on screen while replaying a record")]
        [SerializeField, Enhanced, Required] private FadingObjectBehaviour replayGroup = null;

        [Space(5f)]

        [Tooltip("Maximum input frame adjustement when replaying a capture")]
        [SerializeField, Enhanced, Range(0f, 99f)] private int replayAdjustment = 0;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Current state of this object")]
        [SerializeField, Enhanced, ReadOnly] private State state = State.Inactive;

        [Tooltip("Whether any input is currently enabled, which also indicates if the capture is currently active")]
        [SerializeField, Enhanced, ReadOnly] private bool isActive = true;

        [Tooltip("Whether the recorder is currently paused or not")]
        [SerializeField, Enhanced, ReadOnly] private bool isPaused = false;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Memory allocate for capture by default (will automatically grow up to max memory)")]
        [SerializeField, Enhanced, DisplayName("Default Size"), Range(nameof(RecordMemorySizeRange))] private int recordMemoryDefaultSize = 2;

        [Tooltip("Maximum memory allocated for capture. Once a capture reaches this limit, new events will start overwriting old ones")]
        [SerializeField, Enhanced, DisplayName("Max Size"), Range(1, 500)] private int recordMemoryMaxSize = 10;

        // -----------------------

        private InputEventTrace eventTrace = null;
        private InputEventTrace.ReplayController replayController = null;

        // -----------------------

        /// <summary>
        /// Default memory size used to store input recording data (in MB).
        /// </summary>
        public int RecordMemoryDefaultSize {
            get { return recordMemoryDefaultSize * 1024 * 1024; }
        }

        /// <summary>
        /// Default memory size used to store input recording data (in MB).
        /// </summary>
        public int RecordMemoryMaxSize {
            get { return recordMemoryMaxSize * 1024 * 1024; }
        }

        public Vector2 RecordMemorySizeRange {
            get { return new Vector2(1f, RecordMemoryMaxSize); }
        }

        /// <summary>
        /// Indicates if a record is currently in progress.
        /// </summary>
        public bool IsRecording {
            get { return GetEventTrace(out var _eventTrace) && _eventTrace.enabled; }
        }

        /// <summary>
        /// Indicates if a replay is currently in progress.
        /// </summary>
        public bool IsReplaying {
            get { return GetReplayController(out var _controller) && !_controller.finished; }
        }

        /// <summary>
        /// Total number of captured events.
        /// </summary>
        public long EventCount {
            get {
                if (GetEventTrace(out var _eventTrace)) {
                    return _eventTrace.eventCount;
                }

                return 0L;
            }
        }

        /// <summary>
        /// Total size of captured events (in bytes).
        /// </summary>
        public long TotalEventSizeInBytes {
            get {
                if (GetEventTrace(out var _eventTrace)) {
                    return _eventTrace.totalEventSizeInBytes;
                }

                return 0L;
            }
        }

        /// <summary>
        /// Total size of capture memory currently allocated.
        /// </summary>
        public long AllocatedSizeInBytes {
            get {
                if (GetEventTrace(out var _eventTrace)) {
                    return _eventTrace.allocatedSizeInBytes;
                }

                return 0L;
            }
        }

        /// <summary>
        /// The underlying event trace that contains the captured input events (null if there is no capture).
        /// </summary>
        public InputEventTrace Capture {
            get { return eventTrace; }
        }

        /// <summary>
        /// The replay controller for when a replay is running.
        /// </summary>
        public InputEventTrace.ReplayController Replay {
            get { return replayController; }
        }

        /// <summary>
        /// Position of the current replay operation.
        /// </summary>
        public int ReplayPosition {
            get {
                if (GetReplayController(out var _controller)) {
                    return _controller.position;
                }

                return 0;
            }
        }
        #endregion

        #region Enhanced Behaviour
        void IStableUpdate.Update() {

            // Updates inputs during replay.
            if (IsReplaying) {
                UpdateInputState();
            }

            switch (state) {

                // Record.
                case State.Record:

                    if (IsRecording) {
                        RecordInputData();
                    } else {
                        StopRecord();
                    }

                    break;

                // Replay.
                case State.Replay:

                    if (IsReplaying) {
                        ReplayInputData();
                    } else {
                        StopReplay();
                    }

                    break;

                case State.Inactive:
                default:
                    break;
            }
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Capture.
            StopRecord();
            StopReplay();
        }

        private void OnDestroy() {

            // Clear cache.
            if (GetEventTrace(out var _eventTrace)) {

                _eventTrace.Dispose();
                eventTrace = null;
            }
        }
        #endregion

        #region Record File
        private const string MetaFileExtension = ".meta";

        // -----------------------

        /// <summary>
        /// Saves the current record to a file on disk.
        /// </summary>
        /// <param name="_filePath">Full path of the file to save on disk.</param>
        /// <returns>True if the record could be successfully saved, false otherwise.</returns>
        public bool SaveRecordToFile(string _filePath) {

            if (eventTrace == null) {
                return false;
            }

            // Security.
            if (string.IsNullOrEmpty(_filePath)) {

                this.LogErrorMessage("Save file path cannot be empty");
                return false;
            }

            // Save.
            eventTrace.WriteTo(_filePath);

            // Meta.
            string _metaFilePath = $"{_filePath}{MetaFileExtension}";
            string _metaData = JsonUtility.ToJson(dataHolder);

            File.WriteAllText(_metaFilePath, _metaData);

            this.LogMessage($"Successfully saved record at path \'{_filePath}\'");
            return true;
        }

        /// <summary>
        /// Loads a specific record from a file on disk.
        /// </summary>
        /// <param name="_filePath">Full path of the file to load from disk.</param>
        /// <returns>True if the record could be successfully loaded, false otherwise.</returns>
        public bool LoadRecordFromFile(string _filePath) {

            // Security.
            if (string.IsNullOrEmpty(_filePath)) {

                this.LogErrorMessage("Load file path cannot be empty");
                return false;
            }

            // Load.
            CreateEventTrace();
            eventTrace.ReadFrom(_filePath);

            // Meta.
            string _metaFilePath = $"{_filePath}{MetaFileExtension}";
            if (File.Exists(_metaFilePath)) {

                string _json = File.ReadAllText(_metaFilePath);
                if (!string.IsNullOrEmpty(_json)) {

                    InputCaptureDataHolder _data = new InputCaptureDataHolder();
                    JsonUtility.FromJsonOverwrite(_json, _data);

                    if (_data != null) {
                        dataHolder = _data;
                    }
                }
            }

            this.LogMessage($"Successfully loaded record from path \'{_filePath}\'");
            return true;
        }
        #endregion

        #region Capture
        private const BindingFlags StaticFlags      = BindingFlags.Static   | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags InstanceFlags    = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Type inputRuntimeType           = typeof(InputEventTrace).Assembly.GetType("UnityEngine.InputSystem.LowLevel.InputRuntime");
        private static readonly FieldInfo inputRuntimeInstance  = inputRuntimeType.GetField("s_Instance", StaticFlags);

        private static readonly FieldInfo timeRuntimeField      = typeof(InputEventTrace.ReplayController).GetField("m_StartTimeAsPerRuntime", InstanceFlags);
        private static readonly FieldInfo timeFirstEventField   = typeof(InputEventTrace.ReplayController).GetField("m_StartTimeAsPerFirstEvent", InstanceFlags);
        private static readonly FieldInfo allEventsField        = typeof(InputEventTrace.ReplayController).GetField("m_AllEventsByTime", InstanceFlags);
        private static readonly FieldInfo eventIndexField       = typeof(InputEventTrace.ReplayController).GetField("m_AllEventsByTimeIndex", InstanceFlags);

        [NonSerialized] private double replayTime = 0d;

        [NonSerialized] private int firstCaptureEventID = 0;
        [NonSerialized] private int eventDeviceID = 0;

        /// <summary>
        /// ID of the first marker from the current capture.
        /// </summary>
        public int FirstCaptureEventID {
            get { return firstCaptureEventID; }
            internal set { firstCaptureEventID = value; }
        }

        /// <summary>
        /// Current <see cref="InputSystem"/>-related time.
        /// </summary>
        public double InputCurrentTime {
            get {
                var _runtime = inputRuntimeInstance.GetValue(null);
                PropertyInfo _property = _runtime.GetType().GetProperty("currentTime", InstanceFlags);

                double _time = (double)_property.GetValue(_runtime);
                return _time;
            }
        }

        /// <summary>
        /// Time offset of the current capture.
        /// </summary>
        public double GetCaptureTimeOffset {
            get {

                // Get all capture events.
                List<InputEventPtr> _events = (List<InputEventPtr>)allEventsField.GetValue(replayController);
                int _eventCount = _events.Count;

                if ((_events == null) || (_eventCount < 3)) {
                    return 0d;
                }

                // Retrieve recorder device id (not the same as during record).
                if ((eventDeviceID == 0) && (firstCaptureEventID != 0)) {

                    int _firstEventIndex = -1;
                    for (int i = 0; i < _eventCount; i++) {

                        InputEventPtr _ptr = _events[i];
                        if (_ptr.id == firstCaptureEventID) {

                            _firstEventIndex = _ptr.deviceId;
                            break;
                        }
                    }

                    eventDeviceID = _firstEventIndex;
                    //this.LogError("Delog ID => " + firstCaptureEventID + " - " + eventDeviceID + " | " + _firstEventIndex);
                }

                // Get marked event time offset.
                int _index = (int)eventIndexField.GetValue(replayController);
                int _previous = _index - 1;

                int _adjustment = Mathf.Clamp(replayAdjustment, 0, _eventCount - 1);

                // Detect marker index.
                int _min = Mathf.Max(0, _index - replayAdjustment);
                int _max = Mathf.Min(_eventCount - 1, _index + replayAdjustment);

                for (int i = _min; i < Mathf.Max(0, _max - _min); i++) {
                    if (_events[i].deviceId == eventDeviceID) {
                        _index = i;
                        break;
                    }
                }

                // Security.
                _index    = Mathf.Clamp(_index, 0, _eventCount - 1);
                _previous = Mathf.Clamp(_previous, 0, _eventCount - 1);

                return _events[_index].time - _events[_previous].time;
            }
        }

        // -------------------------------------------
        // Record
        // -------------------------------------------

        /// <summary>
        /// Starts input recording.
        /// </summary>
        public void StartRecord() {

            if (IsRecording) {

                // Resume.
                isPaused = false;

                foreach (var item in InputSystem.devices) {
                    InputSystem.ResetDevice(item);
                }

                // Send an event used, used as a marker for the capture.
                InputSystem.QueueConfigChangeEvent(EnhancedRecorderDevice.Instance);

                return;
            }

            // Start record.
            StopReplay();
            CreateEventTrace().Enable();

            // State.
            SetState(State.Record);
            isPaused = false;

            // Pause.
            if (!isActive) {
                PauseRecord();
            }

            // Group.
            recordGroup.Show();
        }

        /// <summary>
        /// Pauses input recording.
        /// </summary>
        public void PauseRecord() {

            if (!IsRecording) {
                return;
            }

            isPaused = true;
        }

        /// <summary>
        /// Stops input recording.
        /// </summary>
        public void StopRecord() {

            if (GetEventTrace(out InputEventTrace _event)) {
                eventTrace.Disable();
            }

            // State.
            if (state == State.Record) {
                SetState(State.Inactive);
            }

            // Group.
            recordGroup.Hide();
        }

        // -------------------------------------------
        // Replay
        // -------------------------------------------

        /// <summary>
        /// Starts input replay from recording.
        /// </summary>
        public void StartReplay() {

            // No capture.
            if (!GetEventTrace(out var _eventTrace)) {
                return;
            }

            // Pause.
            if (IsReplaying && isPaused) {

                replayController.paused = false;
                isPaused = false;

                foreach (var item in InputSystem.devices) {
                    InputSystem.ResetDevice(item);
                }

                // Capture time offset.
                double _time = (double)timeRuntimeField.GetValue(replayController);
                double _elapsed = InputCurrentTime - replayTime;

                double _captureOffset = GetCaptureTimeOffset;
                double _realElapsed = _elapsed - _captureOffset;

                //this.LogMessage("Delog Elapsed => " + _time + " - " + _elapsed + " | " + _captureOffset + " - " + _realElapsed + " | " + EventIndex);

                timeRuntimeField.SetValue(replayController, _time + _realElapsed);
                return;
            }

            StopRecord();

            // Configuration.
            replayController = eventTrace.Replay().OnFinished(StopReplay);

            if (replayOnNewDevices) {
                replayController.WithAllDevicesMappedToNewInstances();
            }

            // Start replay.
            if (simulateOriginalTimingOnReplay) {
                replayController.PlayAllEventsAccordingToTimestamps();
            } else {
                replayController.PlayAllFramesOneByOne();
            }

            // State.
            SetState(State.Replay);
            isPaused = false;

            // Pause.
            if (!isActive) {
                PauseReplay();
            }

            // Group.
            replayGroup.Show();
        }

        /// <summary>
        /// Pauses input replay.
        /// </summary>
        public void PauseReplay() {

            if (!GetReplayController(out var _controller)) {
                return;
            }

            _controller.paused = true;
            isPaused = true;

            replayTime = InputCurrentTime;
        }

        /// <summary>
        /// Stops input replay from recording.
        /// </summary>
        public void StopReplay() {

            if (GetReplayController(out var _controller)) {

                _controller.Dispose();
                replayController = null;
            }

            // State.
            if (state == State.Replay) {
                SetState(State.Inactive);
            }

            // Group.
            replayGroup.Hide();
        }
        #endregion

        #region Input
        /// <summary>
        /// Filters input events according to the recorder configuration.
        /// </summary>
        /// <param name="_event">Current input event.</param>
        /// <param name="_device">Current input device.</param>
        /// <returns>True if the event was successfully filtered and should be recorded, false otherwise.</returns>
        private bool OnFilterInputEvent(InputEventPtr _event, InputDevice _device) {

            UpdateInputState();

            // Don't register inputs while paused.
            if (isPaused || !isActive) {
                return false;
            }

            if (_device == EnhancedRecorderDevice.Instance) {
                //this.LogErrorMessage("DEVICE => " + _event.deviceId + " - " + _event.type + " - " + _event.id);

                if (firstCaptureEventID == 0) {
                    firstCaptureEventID = _event.id;
                }
            }

            // Filter out non-state events, if enabled.
            if (recordStateEventsOnly && !_event.IsA<StateEvent>() && !_event.IsA<DeltaStateEvent>()) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks enabled inputs and pauses / resumes capture in consequence.
        /// </summary>
        private void UpdateInputState() {

            // Check if any input is currently active.
            // If not, pauses the current record / replay.
            //
            // Used to consistently disable capture during loading or any sequence that doesn't require input.
            // Reduces potential machine-dependant issues (like loading times).

            InputDatabase _database = InputDatabase.I;
            bool _active = false;

            for (int i = 0; i < _database.InputMapCount; i++) {

                if (_database.GetMapAt(i).IsEnabled) {
                    _active = true;
                }
            }

            _active &= Application.isFocused;

            // Pause update.
            if (_active != isActive) {

                isActive = _active;

                switch (state) {

                    // Record.
                    case State.Record:

                        if (_active) {
                            StartRecord();
                        } else {
                            PauseRecord();
                        }

                        break;

                    // Replay.
                    case State.Replay:

                        if (_active) {
                            StartReplay();
                        } else {
                            PauseReplay();
                        }

                        break;

                    case State.Inactive:
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Pauses / unpauses all in-game orphan inputs.
        /// <br/> As these inputs are not referenced in action maps,
        /// this is used to record and replay the most accurate behaviours during capture.
        /// </summary>
        /// <param name="_pause"></param>
        private void PauseOrphanInputs(bool _pause) {

            InputDatabase _database = InputDatabase.I;
            bool _active = false;

            for (int i = _database.InputCount; i-- > 0;) {

                var _input = _database.GetActionAt(i);

                if (_input.IsOrphan) {
                    _input.IsPaused = _pause;
                }
            }
        }
        #endregion

        #region Data
        private static List<IInputRecorderData> dataBuffer = new List<IInputRecorderData>();
        private static InputCaptureDataHolder dataHolder = new InputCaptureDataHolder();

        // -----------------------

        /// <summary>
        /// Records all registered input data.
        /// </summary>
        private void RecordInputData() {

            if (isPaused || !recordMetaData) {
                return;
            }

            double _time = InputCurrentTime;
            int _bufferSize = dataBuffer.Count;

            for (int i = 0; i < _bufferSize; i++) {

                IInputRecorderData _recordData = dataBuffer[i];

                if (_recordData.Record(out InputCaptureSingleDataObject _data)) {
                    dataHolder.RecordData(_time, _recordData.InputDataID, _data);
                }
            }
        }

        /// <summary>
        /// Replays all registered input data.
        /// </summary>
        private void ReplayInputData() {

            if (isPaused || !recordMetaData) {
                return;
            }

            double _time = InputCurrentTime;
            double _timeRuntime = (double)timeRuntimeField.GetValue(replayController);
            double _timeFirstEvent = (double)timeFirstEventField.GetValue(replayController);

            if (_timeFirstEvent < 0d) {
                return;
            }

            int _bufferSize = dataBuffer.Count;

            for (int i = 0; i < _bufferSize; i++) {

                IInputRecorderData _recordData = dataBuffer[i];
                dataHolder.ReplayData(_recordData, _time, _timeRuntime, _timeFirstEvent);
            }
        }

        // -------------------------------------------
        // Rehistration
        // -------------------------------------------

        /// <summary>
        /// Registers a specific <see cref="IInputRecorderData"/> instance.
        /// </summary>
        /// <param name="_data"><see cref="IInputRecorderData"/> to register.</param>
        public static void RegisterInputData(IInputRecorderData _data) {
            dataBuffer.Add(_data);
        }

        /// <summary>
        /// Unregisters a specific <see cref="IInputRecorderData"/> instance.
        /// </summary>
        /// <param name="_data"><see cref="IInputRecorderData"/> to unregister.</param>
        public static void UnregisterInputData(IInputRecorderData _data) {
            dataBuffer.Remove(_data);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Clears this recording current capture.
        /// </summary>
        public void ClearCapture() {

            if (eventTrace == null) {
                return;
            }

            eventTrace.Clear();
        }

        /// <summary>
        /// Sets the current state of this object.
        /// </summary>
        /// <param name="_state">New state of this object.</param>
        private void SetState(State _state) {

            state = _state;
            PauseOrphanInputs(_state != State.Inactive);

            #if UNITY_EDITOR
            // Focus game window.
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            #endif
        }

        /// <summary>
        /// Creates this record event trace.
        /// </summary>
        /// <returns>New capture capture <see cref="InputEventTrace"/>.</returns>
        private InputEventTrace CreateEventTrace() {

            // Dispose previous event.
            if (eventTrace != null) {
                eventTrace.Dispose();
            }

            // Create event.
            eventTrace = new InputEventTrace(RecordMemoryDefaultSize, true, RecordMemoryMaxSize) {
                recordFrameMarkers = recordFrames,
            };

            eventTrace.onFilterEvent += OnFilterInputEvent;
            return eventTrace;
        }

        // -------------------------------------------
        // Getter
        // -------------------------------------------

        /// <summary>
        /// Get this recorder current capture <see cref="InputEventTrace"/>.
        /// </summary>
        /// <param name="_eventTrace">This recorder <see cref="InputEventTrace"/>.</param>
        /// <returns>True if an invent trace is currently active, false otherwise.</returns>
        public bool GetEventTrace(out InputEventTrace _eventTrace) {
            _eventTrace = eventTrace;
            return _eventTrace != null;
        }

        /// <summary>
        /// Get this recorder current replay controller.
        /// </summary>
        /// <param name="_replayController">This recorder current replay controller.</param>
        /// <returns>True if an input controller is currently active, false otherwise.</returns>
        public bool GetReplayController(out InputEventTrace.ReplayController _replayController) {
            _replayController = replayController;
            return _replayController != null;
        }
        #endregion
    }
}
#endif
