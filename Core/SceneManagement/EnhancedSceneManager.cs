// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core.GameStates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace EnhancedFramework.Core {
    /// <summary>
    /// All <see cref="EnhancedSceneManager"/> loading-related possible states.
    /// </summary>
    public enum LoadingState {
        Inactive                = 0,

        Request                 = 1,
        Prepare                 = 2,
        Start                   = 3,

        Loading                 = 20,
        Unloading               = 21,
        FreeMemory              = 22,
        MinimumDuration         = 23,
        WaitForInitialization   = 24,
        Ready                   = 25,

        Complete                = 99,
    }

    /// <summary>
    /// All <see cref="EnhancedSceneManager"/> unloading-related possible states.
    /// </summary>
    public enum UnloadingState {
        Inactive                = 0,

        Request                 = 1,
        Prepare                 = 2,
        Start                   = 3,

        Unloading               = 20,
        FreeMemory              = 21,
        WaitForInitialization   = 22,
        Ready                   = 23,

        Complete                = 99
    }

    /// <summary>
    /// Interface used to indicate when an object has been fully initialized during a loading state.
    /// <br/> After loading scenes, the <see cref="EnhancedSceneManager"/>
    /// waits for all <see cref="ILoadingProcessor"/> to be initialized before moving to the next phase.
    /// <para/>
    /// You can register an object from the <see cref="EnhancedSceneManager"/> using
    /// <br/> <see cref="EnhancedSceneManager.RegisterProcessor(ILoadingProcessor)"/>
    /// and <see cref="EnhancedSceneManager.UnregisterProcessor(ILoadingProcessor)"/>.
    /// </summary>
    public interface ILoadingProcessor {
        #region Content
        /// <summary>
        /// This processor <see cref="Object"/> used for log.
        /// </summary>
        Object LogObject { get; }

        /// <summary>
        /// Indicates if the object has been fully initialized or is still processing.
        /// </summary>
        bool IsProcessing { get; }
        #endregion
    }

    /// <summary>
    /// Default <see cref="SceneManagerBehaviour"/>,
    /// allowing to pause the game when entering in a loading state.
    /// </summary>
    [Serializable, DisplayName("<Default>")]
    public sealed class DefaultSceneManagerBehaviour : SceneManagerBehaviour {
        #region Global Members
        [SerializeField] private bool pauseGameOnLoading = true;
        #endregion

        #region Loading
        private const int ChronosPriority = 999;
        private readonly int chronosID = EnhancedUtility.GenerateGUID();

        // -----------------------

        public override void OnStartLoading() {
            base.OnStartLoading();

            // Pauses the game when entering in a loading state.
            if (pauseGameOnLoading) {
                ChronosManager.Instance.PushOverride(chronosID, 0f, ChronosPriority);
            }
        }

        public override void OnStopLoading() {
            base.OnStopLoading();

            // Unpause when exiting the loading state.
            if (pauseGameOnLoading) {
                ChronosManager.Instance.PopOverride(chronosID);
            }
        }
        #endregion
    }

    // ===== Manager ===== \\

    /// <summary>
    /// Singleton instance managing the loading and unloading of scenes in the game.
    /// <br/> Deals with <see cref="SceneBundle"/> and the game core scene.
    /// <para/>
    /// To create and implement you own behaviours, simply create
    /// a new <see cref="SceneManagerBehaviour"/> class and assign it to this instance.
    /// <br/> Then, get access to it using the <see cref="Behaviour"/>
    /// property to call your own functions and receive multiple callbacks.
    /// </summary>
    [ScriptGizmos(false, true)]
    [DefaultExecutionOrder(-950)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "General/Scene Manager"), DisallowMultipleComponent]
    public sealed class EnhancedSceneManager : EnhancedSingleton<EnhancedSceneManager>, IGameStateLifetimeCallback {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init;

        #region Global Members
        [Section("Enhanced Scene Manager")]

        [SerializeField] private SerializedType<ILoadingState> loadingStateType     = new SerializedType<ILoadingState>(SerializedTypeConstraint.None,
                                                                                                                        typeof(DefaultLoadingGameState));

        [SerializeField] private SerializedType<IUnloadingState> unloadingStateType = new SerializedType<IUnloadingState>(SerializedTypeConstraint.None,
                                                                                                                          typeof(DefaultUnloadingGameState));

        [Space(10f)]

        [Tooltip("If true, unload unused reosurces after loading and unloading (this can be an expensive operation)")]
        [SerializeField] private bool unloadResources = true;

        [Tooltip("The first scene to load when starting the game")]
        [SerializeField, Enhanced, Required] private SceneBundle firstScene = null;

        [Space(10f)]

        [SerializeField, Enhanced, ReadOnly] private LoadingState loadingState      = LoadingState.Inactive;
        [SerializeField, Enhanced, ReadOnly] private UnloadingState unloadingState  = UnloadingState.Inactive;

        [Space(10f)]

        [SerializeField, Enhanced, ReadOnly] private Set<SceneBundle> loadedBundles = new Set<SceneBundle>();

        // -------------------------------------------
        // Settings & Behaviour
        // -------------------------------------------

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField] public PolymorphValue<LoadSceneSettings> LoadingSettings       = new PolymorphValue<LoadSceneSettings>(SerializedTypeConstraint.BaseType,
                                                                                                                                typeof(LoadSceneSettings));

        [SerializeField] public PolymorphValue<UnloadSceneSettings> UnloadingSettings   = new PolymorphValue<UnloadSceneSettings>(SerializedTypeConstraint.BaseType,
                                                                                                                                  typeof(UnloadSceneSettings));

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField] private PolymorphValue<SceneManagerBehaviour> behaviour = new PolymorphValue<SceneManagerBehaviour>(SerializedTypeConstraint.None,
                                                                                                                             typeof(DefaultSceneManagerBehaviour));

        // -------------------------------------------
        // Accessors
        // -------------------------------------------

        /// <summary>
        /// The current <see cref="Core.LoadingState"/> of this scene manager.
        /// </summary>
        public LoadingState LoadingState {
            get { return loadingState; }
        }

        /// <summary>
        /// The current <see cref="Core.UnloadingState"/> of this scene manager.
        /// </summary>
        public UnloadingState UnloadingState {
            get { return unloadingState; }
        }

        /// <summary>
        /// This scene manager <see cref="SceneManagerBehaviour"/> instance.
        /// <br/> Use this to call your own specific behaviours.
        /// </summary>
        public SceneManagerBehaviour Behaviour {
            get { return behaviour.Value; }
        }

        /// <summary>
        /// Total count of currently loaded <see cref="SceneBundle"/>.
        /// </summary>
        public int LoadedBundleCount {
            get { return loadedBundles.Count; }
        }

        // -------------------------------------------
        // Events
        // -------------------------------------------

        /// <summary>
        /// Called when starting a new loading operation.
        /// </summary>
        public static event Action OnStartLoading   = null;

        /// <summary>
        /// Called when stoping the current loading operation.
        /// </summary>
        public static event Action OnStopLoading    = null;

        /// <summary>
        /// Called when starting a new unloading operation.
        /// </summary>
        public static event Action OnStartUnloading = null;

        /// <summary>
        /// Called when stoping the current unloading operation.
        /// </summary>
        public static event Action OnStopUnloading  = null;

        /// <summary>
        /// Called when the state for the current loading operation changes.
        /// </summary>
        public static event Action<LoadingState> OnLoadingState     = null;

        /// <summary>
        /// Called when the state for the current unloading operation changes.
        /// </summary>
        public static event Action<UnloadingState> OnUnloadingState = null;

        /// <summary>
        /// Called whenever before loading a <see cref="SceneBundle"/>.
        /// </summary>
        public static event Action<SceneBundle> OnPreLoadBundle     = null;

        /// <summary>
        /// Called whenever after a <see cref="SceneBundle"/> has been loaded.
        /// </summary>
        public static event Action<SceneBundle> OnPostLoadBundle    = null;

        /// <summary>
        /// Called whenever before unloading a <see cref="SceneBundle"/>.
        /// </summary>
        public static event Action<SceneBundle> OnPreUnloadBundle   = null;

        /// <summary>
        /// Called whenever after a <see cref="SceneBundle"/> has been unloaded.
        /// </summary>
        public static event Action<SceneBundle> OnPostUnloadBundle  = null;
        #endregion

        #region Enhanced Behaviour
        protected override void OnInit() {
            base.OnInit();

            // Get all loaded bundles.
            for (int i = SceneManager.sceneCount; i-- > 0;) {

                Scene _scene = SceneManager.GetSceneAt(i);
                if (!_scene.IsCoreScene() && SceneBundle.GetSceneBundle(_scene, out SceneBundle _bundle)) {
                    loadedBundles.Add(_bundle);
                }
            }

            #if UNITY_EDITOR
            if (SceneManager.sceneCount == 1) {
                // Load the first scene if only core is loaded.
                LoadFirstScene();
            } else {
                // If all scenes are already loaded, enters in a loading state as soon as possible.
                // Used to prepare the scene and wait for all objects to be fully initialized.
                BuildSceneDatabase _database = BuildSceneDatabase.Database;
                int _sceneBundleCount = _database.SceneBundleCount;

                for (int i = 0; i < _sceneBundleCount; i++) {
                    SceneBundle _bundle = _database.GetSceneBundleAt(i);

                    // Simulate scene loading; a scene cannot be loaded twice anyway.
                    if (_bundle.IsLoaded) {
                        LoadSceneBundle(_bundle, LoadSceneMode.Additive);
                    }
                }

                // Avoid any Play callback before end of loading by suspending the UpdateManager.
                UpdateManager.Instance.IsSuspended = true;

                // Default loading.
                var _loadingBundles = loadingBundles;
                int _count = _loadingBundles.Count;

                if (_count == 0) {
                    DoPerformLoading();
                }

                // Simulate game loading if another scene than core or than the first game scene is loaded.
                if ((_count != 2) || !IsFirstSceneLoaded(ref _loadingBundles.collection, firstScene)) {
                    Behaviour.OnEnterPlayModeEditor(_loadingBundles, LoadingSettings);
                }
            }
            #else
            LoadFirstScene();
            #endif

            // ----- Local Methods ----- \\

            static bool IsFirstSceneLoaded(ref List<Pair<SceneBundle, LoadSceneMode>> _loadingBundles, SceneBundle _firstScene) {

                for (int i = _loadingBundles.Count; i-- > 0;) {
                    if (_loadingBundles[i].First == _firstScene)
                        return true;
                }

                return false;
            }

            void LoadFirstScene() {
                if (firstScene.IsValid()) {
                    Instance.LoadSceneBundle(firstScene, LoadSceneMode.Additive);
                }
            }
        }
        #endregion

        #region Registration
        private List<ILoadingProcessor> processors = new List<ILoadingProcessor>();

        // -----------------------
        
        /// <summary>
        /// Registers a new <see cref="ILoadingProcessor"/> instance.
        /// <para/>
        /// Should be called on object initialization. Keep in mind to unregister it on deactivation.
        /// </summary>
        /// <param name="_processor">The <see cref="ILoadingProcessor"/> to register.</param>
        public void RegisterProcessor(ILoadingProcessor _processor) {
            processors.Add(_processor);
        }

        /// <summary>
        /// Unregisters a specific <see cref="ILoadingProcessor"/> instance.
        /// <para/>
        /// Should be called when the object is deactivated or destroyed.
        /// </summary>
        /// <param name="_processor">The <see cref="ILoadingProcessor"/> to unregister.</param>
        public void UnregisterProcessor(ILoadingProcessor _processor) {
            processors.RemoveSwap(_processor);
        }
        #endregion

        #region Game State
        void IGameStateLifetimeCallback.OnInit(GameState _state) {
            switch (_state) {
                // Start loading.
                case ILoadingState:
                    StartLoading(_state);
                    break;

                // Start unloading.
                case IUnloadingState:
                    StartUnloading(_state);
                    break;

                default:
                    break;
            }
        }

        void IGameStateLifetimeCallback.OnTerminate(GameState _state) {
            switch (_state) {
                // Stop loading.
                case ILoadingState:
                    StopLoading();
                    break;

                // Stop unloading.
                case IUnloadingState:
                    StopUnloading();
                    break;

                default:
                    break;
            }
        }
        #endregion

        #region Loading
        private const float StartLoadDelay      = .2f;
        private const float LoadBundleInterval  = .01f;
        private const float LoadCompletionDelay = .25f;

        private readonly PairCollection<SceneBundle, LoadSceneMode> loadingBundles = new PairCollection<SceneBundle, LoadSceneMode>();

        private Coroutine loadingCoroutine = null;
        private GameState loadingGameState = null;

        /// <summary>
        /// The <see cref="GameState"/> of the current loading operation.
        /// </summary>
        public GameState LoadingGameState {
            get { return loadingGameState; }
        }

        // -------------------------------------------
        // Requests
        // -------------------------------------------

        /// <param name="_settings">The settings to use to perform this loading.</param>
        /// <inheritdoc cref="LoadSceneBundle(SceneBundle, LoadSceneMode)"/>
        public void LoadSceneBundle(SceneBundle _bundle, LoadSceneMode _mode, LoadSceneSettings _settings) {
            LoadingSettings.Value = _settings;
            LoadSceneBundle(_bundle, _mode);
        }

        /// <summary>
        /// Requests to load a specific <see cref="SceneBundle"/>.
        /// </summary>
        /// <param name="_bundle">The <see cref="SceneBundle"/> to load.</param>
        /// <param name="_mode">The mode used to load the bundle.</param>
        public void LoadSceneBundle(SceneBundle _bundle, LoadSceneMode _mode) {
            loadingBundles.Add(_bundle, _mode);
            DoPerformLoading();
        }

        private void DoPerformLoading() {
            // Create a new loading state if none.
            // Loading will only start when the state ask for it.
            if (!loadingGameState.IsActive()) {

                SetLoadingState(LoadingState.Request);
                loadingGameState = GameState.CreateState(loadingStateType);
            }
        }

        // -------------------------------------------
        // Game State Callbacks
        // -------------------------------------------

        /// <summary>
        /// Starts loading all previously requested <see cref="SceneBundle"/>.
        /// <para/>
        /// Used as a <see cref="LoadingGameState{T}"/> callback.
        /// </summary>
        private void StartLoading(GameState _gameState) {
            loadingGameState = _gameState;

            #if DEVELOPMENT
            // This case should never happen. If it does, log an error.
            if (loadingCoroutine != null) {
                this.LogErrorMessage("Starting a new loading operation while another is still running\nStoping the previous coroutine");
                StopLoading();
            }
            #endif

            // Start loading.
            loadingCoroutine = StartCoroutine(PerformLoading());
        }

        /// <summary>
        /// Stops all active loading operation(s).
        /// <para/>
        /// Used as a <see cref="LoadingGameState{T}"/> callback.
        /// </summary>
        private void StopLoading() {
            switch (loadingState) {
                // Ignore when inactive.
                case LoadingState.Inactive:
                    return;

                // Valid.
                case LoadingState.Complete:
                    break;

                // Prematurely stopped.
                default:
                    this.LogWarningMessage("Loading prematurely canceled before completion");
                    StopCoroutine(loadingCoroutine);

                    Behaviour.OnCancelLoading();
                    break;
            }

            SetLoadingState(LoadingState.Inactive);
            Behaviour.OnStopLoading();

            OnStopLoading?.Invoke();
            loadingCoroutine = null;
        }

        // -------------------------------------------
        // Loading
        // -------------------------------------------

        private IEnumerator PerformLoading() {
            yield return null;

            SceneManagerBehaviour _behaviour = Behaviour;

            // Preparation.
            _behaviour.PrepareLoading(loadingBundles, LoadingSettings);
            SetLoadingState(LoadingState.Prepare);
            
            // Wait for all pending operations to be compelete.
            while ((unloadingState != UnloadingState.Inactive) || !_behaviour.StartLoading) {
                yield return null;
            }

            // Initialization.
            _behaviour.OnStartLoading();
            OnStartLoading?.Invoke();

            SetLoadingState(LoadingState.Start);
            double _startTime = Time.realtimeSinceStartup;

            yield return null;

            // Terminates all gameplay states.
            GameStateManager.Instance.PopNonPersistentStates();

            yield return new WaitForSecondsRealtime(StartLoadDelay);

            SetLoadingState(LoadingState.Loading);

            // Cache the yield instruction to avoid garbage.
            var _interval = new WaitForSecondsRealtime(LoadBundleInterval);
            var _loadingBundles = loadingBundles;

            for (int i = 0; i < _loadingBundles.Count; i++) {

                var _pair = _loadingBundles[i];
                SceneBundle _toLoad = _pair.First;
                LoadSceneMode _mode = _pair.Second;

                _behaviour.OnPreLoadBundle(_toLoad, _mode);
                OnPreLoadBundle?.Invoke(_toLoad);

                // When wanting to load a bundle all alone,
                // simply unload all scenes except for core, then load it additively.
                if (_mode == LoadSceneMode.Single) {
                    SetLoadingState(LoadingState.Unloading);

                    for (int j = loadedBundles.Count; j-- > 0;) {

                        SceneBundle _toUnload = loadedBundles[j];
                        if (!_toUnload.IsCoreBundle()) {
                            UnloadSceneBundle(_toUnload, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
                        }
                    }

                    // Wait for unloading completion.
                    while (unloadingState != UnloadingState.Inactive) {
                        yield return null;
                    }

                    SetLoadingState(LoadingState.Loading);
                }

                // Loading operation.
                if (!_toLoad.IsLoaded) {
                    // Loading operation.
                    var _operation = _toLoad.LoadAsync(LoadSceneMode.Additive);
                    _behaviour.OnLoadBundle(_operation, i, _loadingBundles.Count);

                    yield return _operation;

                    if (i < (_loadingBundles.Count - 1)) {
                        yield return _interval;
                    }

                    if (!_toLoad.IsCoreBundle()) {
                        loadedBundles.Add(_toLoad);
                    }
                }

                _behaviour.OnPostLoadBundle(_toLoad, _mode);
                OnPostLoadBundle?.Invoke(_toLoad);
            }

            _loadingBundles.Clear();

            // Free memory.
            if (FreeMemory(out AsyncOperation _freeMemory)) {

                SetLoadingState(LoadingState.FreeMemory);
                yield return _freeMemory;
            }

            SetLoadingState(LoadingState.WaitForInitialization);

            // Once all scenes have been loaded, wait before checking initialized behaviours,
            // as some may not be awaken yet.
            yield return new WaitForSecondsRealtime(LoadCompletionDelay);

            // Wait for all behaviours to be fully loaded and initialized.
            while (IsAnyProcessorActive()) {
                yield return null;
            }

            SetLoadingState(LoadingState.MinimumDuration);

            // Minimum duration.
            while ((Time.realtimeSinceStartup - _startTime) < _behaviour.LoadingMinimumDuration) {
                yield return null;
            }

            SetLoadingState(LoadingState.Ready);
            _behaviour.OnLoadingReady();

            // Before completion, let the behaviour complete its requested operations (like a press for any key).
            while (!_behaviour.CompleteLoading) {
                yield return null;
            }

            SetLoadingState(LoadingState.Complete);

            // Terminates the state.
            loadingGameState.RemoveState();
        }

        /// <summary>
        /// Sets the loading state of the object, and perform all related operations.
        /// </summary>
        private void SetLoadingState(LoadingState _state) {
            loadingState = _state;
            Behaviour.OnLoadingState(_state);

            OnLoadingState?.Invoke(_state);
        }
        #endregion

        #region Unloading
        private const float StartUnloadDelay        = .1f;
        private const float UnloadBundleInterval    = .01f;
        private const float UnloadCompletionDelay   = .05f;

        private readonly PairCollection<SceneBundle, UnloadSceneOptions> unloadingBundles = new PairCollection<SceneBundle, UnloadSceneOptions>();

        private Coroutine unloadingCoroutine = null;
        private GameState unloadingGameState = null;

        /// <summary>
        /// The <see cref="GameState"/> of the current unloading operation.
        /// </summary>
        public GameState UnloadingGameState {
            get { return unloadingGameState; }
        }

        // -------------------------------------------
        // Requests
        // -------------------------------------------

        /// <param name="_settings">The settings to use to perform this unloading.</param>
        /// <inheritdoc cref="UnloadSceneBundle(SceneBundle, UnloadSceneOptions)"/>
        public void UnloadSceneBundle(SceneBundle _bundle, UnloadSceneOptions _options, UnloadSceneSettings _settings) {
            UnloadingSettings.Value = _settings;
            UnloadSceneBundle(_bundle, _options);
        }

        /// <summary>
        /// Requests to unload a specific <see cref="SceneBundle"/>.
        /// </summary>
        /// <param name="_bundle">The <see cref="SceneBundle"/> to unload.</param>
        /// <param name="_options">The options used to unload the bundle.</param>
        public void UnloadSceneBundle(SceneBundle _bundle, UnloadSceneOptions _options = UnloadSceneOptions.None) {
            unloadingBundles.Add(_bundle, _options);
            DoPerformUnloading();
        }

        private void DoPerformUnloading() {
            // Create a new unloading state if none.
            // Unloading will only start when the state ask for it.
            if (!unloadingGameState.IsActive()) {

                SetUnloadingState(UnloadingState.Request);
                unloadingGameState = GameState.CreateState(unloadingStateType);
            }
        }

        // -------------------------------------------
        // Game State Callbacks
        // -------------------------------------------

        /// <summary>
        /// Starts unloading all previously requested <see cref="SceneBundle"/>.
        /// <para/>
        /// Used as a <see cref="UnloadingGameState{T}"/> callback.
        /// </summary>
        private void StartUnloading(GameState _gameState) {
            unloadingGameState = _gameState;

            #if DEVELOPMENT
            // This case should never happen. If it does, log an error.
            if (unloadingCoroutine != null) {
                this.LogErrorMessage("Starting a new unloading operation while another is still running\nStoping the previous coroutine");
                StopUnloading();
            }
            #endif

            // Start unloading.
            unloadingCoroutine = StartCoroutine(PerformUnloading());
        }

        /// <summary>
        /// Stops all active unloading operation(s).
        /// <para/>
        /// Used as a <see cref="UnloadingGameState{T}"/> callback.
        /// </summary>
        private void StopUnloading() {
            switch (unloadingState) {
                // Ignore when inactive.
                case UnloadingState.Inactive:
                    return;

                // Valid.
                case UnloadingState.Complete:
                    break;

                // Prematurely stopped.
                default:
                    this.LogWarningMessage("Unloading prematurely canceled before completion");
                    StopCoroutine(unloadingCoroutine);

                    Behaviour.OnCancelUnloading();
                    break;
            }

            SetUnloadingState(UnloadingState.Inactive);
            Behaviour.OnStopUnloading();

            OnStopUnloading?.Invoke();
            unloadingCoroutine = null;
        }

        // -------------------------------------------
        // Unloading
        // -------------------------------------------

        private IEnumerator PerformUnloading() {
            yield return null;

            SceneManagerBehaviour _behaviour = Behaviour;

            // Preparation.
            _behaviour.PrepareUnloading(unloadingBundles, UnloadingSettings);
            SetUnloadingState(UnloadingState.Prepare);

            // Wait while a loading operation is in process.
            while ((loadingState == LoadingState.Loading) || !_behaviour.StartUnloading) {
                yield return null;
            }

            // Initialization.
            _behaviour.OnStartUnloading();
            OnStartUnloading?.Invoke();

            SetUnloadingState(UnloadingState.Start);
            yield return new WaitForSecondsRealtime(StartUnloadDelay);

            SetUnloadingState(UnloadingState.Unloading);

            // Cache the yield instruction to avoid garbage.
            var _interval = new WaitForSecondsRealtime(UnloadBundleInterval);
            var _unloadingBundles = unloadingBundles;

            // Unload bundles.
            for (int i = 0; i < _unloadingBundles.Count; i++) {

                var _pair = _unloadingBundles[i];
                SceneBundle _bundle = _pair.First;
                UnloadSceneOptions _options = _pair.Second;

                // Before unloading the bundle, call the associated event
                // and wait for all processors to be ready.
                // This can be used to perform additional operations before destroying an object.
                _behaviour.OnPreUnloadBundle(_bundle, _options);
                OnPreUnloadBundle?.Invoke(_bundle);

                while (IsAnyProcessorActive()) {
                    yield return null;
                }

                // Unloading operation.
                if (_bundle.IsLoaded) {
                    var _operation = _bundle.UnloadAsync(_options);
                    _behaviour.OnUnloadBundle(_operation, i, _unloadingBundles.Count);

                    yield return _operation;

                    if (i < (_unloadingBundles.Count - 1)) {
                        yield return _interval;
                    }

                    loadedBundles.Remove(_bundle);
                }

                _behaviour.OnPostUnloadBundle(_bundle, _options);
                OnPostUnloadBundle?.Invoke(_bundle);
            }

            _unloadingBundles.Clear();

            // Free memory.
            if (FreeMemory(out AsyncOperation _freeMemory)) {

                SetUnloadingState(UnloadingState.FreeMemory);
                yield return _freeMemory;
            }

            yield return new WaitForSecondsRealtime(UnloadCompletionDelay);

            SetUnloadingState(UnloadingState.Ready);
            _behaviour.OnUnloadingReady();

            // Wait for the behaviour to complete its associated operations.
            while (!_behaviour.CompleteUnloading) {
                yield return null;
            }

            SetUnloadingState(UnloadingState.Complete);

            // Terminates the state.
            unloadingGameState.RemoveState();
        }

        /// <summary>
        /// Sets the unloading state of the object, and perform all related operations.
        /// </summary>
        private void SetUnloadingState(UnloadingState _state) {
            unloadingState = _state;
            Behaviour.OnUnloadingState(_state);

            OnUnloadingState?.Invoke(_state);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get the current <see cref="LoadSceneSettings"/> as the desired type.
        /// </summary>
        /// <typeparam name="T">The type to convert the settings in.</typeparam>
        /// <param name="_settings">The current <see cref="LoadSceneSettings"/> object.</param>
        /// <returns>True if the settings could be converted to the desired type, false otherwise.</returns>
        public bool GetLoadingSettings<T>(out T _settings) where T : LoadSceneSettings {
            if (LoadingSettings.Value is T _temp) {
                _settings = _temp;
                return true;
            }

            _settings = null;
            return false;
        }

        /// <summary>
        /// Get the current <see cref="UnloadSceneSettings"/> as the desired type.
        /// </summary>
        /// <typeparam name="T">The type to convert the settings in.</typeparam>
        /// <param name="_settings">The current <see cref="UnloadSceneSettings"/> object.</param>
        /// <returns>True if the settings could be converted to the desired type, false otherwise.</returns>
        public bool GetUnloadingSettings<T>(out T _settings) where T : UnloadSceneSettings {
            if (UnloadingSettings.Value is T _temp) {
                _settings = _temp;
                return true;
            }

            _settings = null;
            return false;
        }

        /// <summary>
        /// Get the loaded <see cref="SceneBundle"/> at the given index.
        /// </summary>
        /// <param name="_index">Index to get the loaded <see cref="SceneBundle"/> at.</param>
        /// <returns>The loaded <see cref="SceneBundle"/> at the given index.</returns>
        public SceneBundle GetLoadedBundleAt(int _index) {
            return loadedBundles[_index];
        }

        /// <summary>
        /// Indicates if any <see cref="ILoadingProcessor"/> is currently processing or not.
        /// </summary>
        public bool IsAnyProcessorActive() {

            ref List<ILoadingProcessor> _span = ref processors;
            for (int i = _span.Count; i-- > 0;) {

                if (_span[i].IsProcessing) {
                    //this.Log("Waiting for " + processors[_index].LogObject.name);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Moves a given <see cref="GameObject"/> instance to the core scene.
        /// </summary>
        /// <param name="_gameObject">The <see cref="GameObject"/> to move.</param>
        public void MoveGameObjectToCoreScene(GameObject _gameObject) {
            GameObject _thisGameObject = gameObject;

            _thisGameObject.transform.SetParent(null); // Security.
            SceneManager.MoveGameObjectToScene(_gameObject, _thisGameObject.scene);
        }

        /// <summary>
        /// Free some memory space.
        /// <br/> Called once unloading operations have been complete.
        /// </summary>
        private bool FreeMemory(out AsyncOperation _operation) {

            if (!unloadResources) {
                _operation = null;
                return false;
            }

            GC.Collect();
            _operation = Resources.UnloadUnusedAssets();

            return true;
        }
        #endregion
    }
}
