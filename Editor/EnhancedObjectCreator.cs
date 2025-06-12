// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor.Editor;
using EnhancedFramework.Core;
using EnhancedFramework.UI;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

namespace EnhancedFramework.Editor {
    /// <summary>
    /// Utility editor class used to create framework objects from the create menu.
    /// </summary>
    public static class EnhancedObjectCreator {
        #region Fading Group
        /// <summary>
        /// Creates a new <see cref="FadingGroupBehaviour"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Fading Object/Fading Group", false, CreateMenuOrder)]
        public static GameObject CreateFadingGroup(MenuCommand _command) {
            return CreateFadingObject("Fading-Group", _command, typeof(CanvasGroup), typeof(FadingGroupBehaviour));
        }

        /// <summary>
        /// Creates a new <see cref="TweeningFadingGroupBehaviour"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Fading Object/Fading Group [Tween]", false, CreateMenuOrder)]
        public static GameObject CreateTweeningFadingGroup(MenuCommand _command) {
            return CreateFadingObject("Fading-Group", _command, typeof(CanvasGroup), typeof(TweeningFadingGroupBehaviour));
        }

        /// <summary>
        /// Creates a new <see cref="TransitionFadingGroupBehaviour"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Fading Object/Fading Group [Transition]", false, CreateMenuOrder)]
        public static GameObject CreateTransitionFadingGroup(MenuCommand _command) {
            return CreateFadingObject("Fading-Group", _command, typeof(CanvasGroup), typeof(TransitionFadingGroupBehaviour));
        }

        /// <summary>
        /// Creates a new <see cref="ScreenTransitionFadingGroupBehaviour"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Fading Object/Fading Group [Screen Transition]", false, CreateMenuOrder)]
        public static GameObject CreateScreenTransitionFadingGroup(MenuCommand _command) {
            return CreateFadingObject("Fading-Group", _command, typeof(CanvasGroup), typeof(ScreenTransitionFadingGroupBehaviour));
        }

        // -----------------------

        private static GameObject CreateFadingObject(string _name, MenuCommand _command, params Type[] _components) {
            GameObject _object = CreateObject(_name, _command, _components);

            // Fading Object.
            if (_object.TryGetComponent(out FadingObjectBehaviour _fadingObject)) {
                ComponentUtility.MoveComponentUp(_fadingObject);
            }

            return _object;
        }
        #endregion

        #region Audio
        public const string AudioPrefix = "AD_";
        public static readonly int AudioLayer = LayerMask.NameToLayer("Audio");

        private static readonly GUIContent createAudioGUI       = new GUIContent(MenuPath + "Audio/Sound");
        private static readonly GUIContent createMusicGUI       = new GUIContent(MenuPath + "Audio/Music");
        private static readonly GUIContent createAmbientGUI     = new GUIContent(MenuPath + "Audio/Ambient");
        private static readonly GUIContent createSnapshotGUI    = new GUIContent(MenuPath + "Audio/Snapshot");

        // -----------------------

        /// <summary>
        /// Creates a new <see cref="AudioAssetController"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Audio/Sound", false, CreateMenuOrder)]
        public static GameObject CreateAudioControllerObject(MenuCommand _command) {
            return CreateAudio(AudioPrefix + "Sound", _command, typeof(AudioAssetController), typeof(BoxCollider));
        }

        /// <summary>
        /// Creates a new <see cref="AudioMusicController"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Audio/Music", false, CreateMenuOrder)]
        public static GameObject CreateMusicControllerObject(MenuCommand _command) {
            return CreateAudio(AudioPrefix + "Music", _command, typeof(AudioMusicController), typeof(BoxCollider));
        }

        /// <summary>
        /// Creates a new <see cref="AudioAmbientController"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Audio/Ambient", false, CreateMenuOrder)]
        public static GameObject CreateAmbientControllerObject(MenuCommand _command) {
            return CreateAudio(AudioPrefix + "Ambient", _command, typeof(AudioAmbientController), typeof(BoxCollider));
        }

        /// <summary>
        /// Creates a new <see cref="AudioSnapshotController"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Audio/Snapshot", false, CreateMenuOrder)]
        public static GameObject CreateSnapshotControllerObject(MenuCommand _command) {
            return CreateAudio(AudioPrefix + "Snapshot", _command, typeof(AudioSnapshotController), typeof(BoxCollider));
        }

        // -------------------------------------------
        // Scene View
        // -------------------------------------------

        [SceneViewContextMenuItem(Order = ContextMenuOrder)]
        private static void CreateAudioControllerObject(SceneView _, GenericMenu _menu, RaycastHit _hit) {
            CreateObjectAtPosition(createAudioGUI, _menu, _hit, CreateAudioControllerObject);
        }

        [SceneViewContextMenuItem(Order = ContextMenuOrder)]
        private static void CreateMusicControllerObject(SceneView _, GenericMenu _menu, RaycastHit _hit) {
            CreateObjectAtPosition(createMusicGUI, _menu, _hit, CreateMusicControllerObject);
        }

        [SceneViewContextMenuItem(Order = ContextMenuOrder)]
        private static void CreateAmbientControllerObject(SceneView _, GenericMenu _menu, RaycastHit _hit) {
            CreateObjectAtPosition(createAmbientGUI, _menu, _hit, CreateAmbientControllerObject);
        }

        [SceneViewContextMenuItem(Order = ContextMenuOrder)]
        private static void CreateSnapshotControllerObject(SceneView _, GenericMenu _menu, RaycastHit _hit) {
            CreateObjectAtPosition(createSnapshotGUI, _menu, _hit, CreateSnapshotControllerObject);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        private static GameObject CreateAudio(string _name, MenuCommand _command, params Type[] _components) {
            GameObject _object = CreateObject(_name, _command, _components);

            // Trigger.
            if (_object.TryGetComponent(out Collider _collider)) {
                _collider.isTrigger = true;
            }

            // Layer.
            if ((_object.layer == 0) && (AudioLayer != -1)) {
                _object.layer = AudioLayer;
            }

            return _object;
        }
        #endregion

        #region Flag
        public const string FlagPrefix = "FLG_";

        private static readonly GUIContent createFlagControllerGUI      = new GUIContent(MenuPath + "Flag/Flag Controller");
        private static readonly GUIContent createFlagDependantObjectGUI = new GUIContent(MenuPath + "Flag/Flag Dependant Object");

        // -----------------------

        /// <summary>
        /// Creates a new <see cref="FlagController"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Flag/Flag Controller", false, CreateMenuOrder)]
        public static GameObject CreateFlagController(MenuCommand _command) {
            return CreateTrigger(FlagPrefix + "Controller", _command, typeof(FlagController), typeof(BoxCollider));
        }

        /// <summary>
        /// Creates a new <see cref="FlagDependantObject"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Flag/Flag Dependant Object", false, CreateMenuOrder)]
        public static GameObject CreateFlagDependantObject(MenuCommand _command) {
            return CreateObject(FlagPrefix + "GameObject", _command, typeof(FlagDependantObject));
        }

        // -------------------------------------------
        // Scene View
        // -------------------------------------------

        [SceneViewContextMenuItem(Order = ContextMenuOrder)]
        private static void CreateFlagController(SceneView _, GenericMenu _menu, RaycastHit _hit) {
            CreateObjectAtPosition(createFlagControllerGUI, _menu, _hit, CreateFlagController);
        }

        [SceneViewContextMenuItem(Order = ContextMenuOrder)]
        private static void CreateFlagDependantObject(SceneView _, GenericMenu _menu, RaycastHit _hit) {
            CreateObjectAtPosition(createFlagDependantObjectGUI, _menu, _hit, CreateFlagDependantObject);
        }
        #endregion

        #region Player
        /// <summary>
        /// Creates a new <see cref="EnhancedPlayablePlayer"/> object for a timeline.
        /// </summary>
        [MenuItem(CreateMenuPath + "Player/Timeline", false, CreateMenuOrder)]
        public static GameObject CreateEnhancedTimeline(MenuCommand _command) {
            return CreateEnhancedPlayer("TMLN_NewTimeline", _command, typeof(PlayableDirector), typeof(EnhancedPlayablePlayer));
        }

        /// <summary>
        /// Creates a new <see cref="EnhancedVideoPlayer"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Player/Video", false, CreateMenuOrder)]
        public static GameObject CreateEnhancedVideo(MenuCommand _command) {
            return CreateEnhancedPlayer("VID_NewVideo", _command, typeof(VideoPlayer), typeof(EnhancedVideoPlayer));
        }

        /// <summary>
        /// Creates a new <see cref="EnhancedPlayablePlayer"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Player/Playable", false, CreateMenuOrder)]
        public static GameObject CreateEnhancedPlayable(MenuCommand _command) {
            return CreateEnhancedPlayer("PLY_NewPlayable", _command, typeof(PlayableDirector), typeof(EnhancedPlayablePlayer));
        }

        // -----------------------

        private static GameObject CreateEnhancedPlayer(string _name, MenuCommand _command, params Type[] _components) {
            GameObject _object = CreateObject(_name, _command, _components);

            // Player.
            if (_object.TryGetComponent(out EnhancedPlayer _player)) {
                ComponentUtility.MoveComponentUp(_player);
            }

            return _object;
        }
        #endregion

        #region Trigger
        public const string TriggerPrefix = "TRG_";
        public static readonly int TriggerLayer = LayerMask.NameToLayer("Trigger");

        private static readonly GUIContent createBoxTriggerGUI          = new GUIContent(MenuPath + "Trigger/Primitive/Box");
        private static readonly GUIContent createSphereTriggerGUI       = new GUIContent(MenuPath + "Trigger/Primitive/Sphere");
        private static readonly GUIContent createLevelTriggerGUI        = new GUIContent(MenuPath + "Trigger/Level Trigger");
        private static readonly GUIContent createLevelTriggerAreaGUI    = new GUIContent(MenuPath + "Trigger/Level Trigger Area");

        // -----------------------

        /// <summary>
        /// Creates a new box trigger object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Trigger/Primitive/Box", false, CreateMenuOrder)]
        public static GameObject CreateBoxTrigger(MenuCommand _command) {
            return CreateTrigger(TriggerPrefix + "Box", _command, typeof(BoxCollider));
        }

        /// <summary>
        /// Creates a new sphere trigger object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Trigger/Primitive/Sphere", false, CreateMenuOrder)]
        public static GameObject CreateSphereTrigger(MenuCommand _command) {
            return CreateTrigger(TriggerPrefix + "Sphere", _command, typeof(SphereCollider));
        }

        /// <summary>
        /// Creates a new <see cref="LevelTrigger"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Trigger/Level Trigger", false, CreateMenuOrder)]
        public static GameObject CreateLevelTrigger(MenuCommand _command) {
            return CreateLevelTrigger(TriggerPrefix + "Level", _command, typeof(BoxCollider), typeof(LevelTrigger));
        }

        /// <summary>
        /// Creates a new <see cref="LevelTriggerArea"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "Trigger/Level Trigger Area", false, CreateMenuOrder)]
        public static GameObject CreateLevelTriggerArea(MenuCommand _command) {
            GameObject _object = CreateLevelTrigger(TriggerPrefix + "LevelArea", _command, typeof(BoxCollider), typeof(LevelTriggerArea));

            // Area.
            if (_object.TryGetComponent(out LevelTriggerArea _trigger)) {
                _trigger.AreaVertices.AddRange(new Vector3[] { new Vector3(-.5f, 0f,  .5f),
                                                               new Vector3( .5f, 0f,  .5f),
                                                               new Vector3( .5f, 0f, -.5f),
                                                               new Vector3(-.5f, 0f, -.5f) });
            }

            return _object;
        }

        // -------------------------------------------
        // Scene View
        // -------------------------------------------

        [SceneViewContextMenuItem(Order = ContextMenuOrder)]
        private static void CreateBoxTrigger(SceneView _, GenericMenu _menu, RaycastHit _hit) {
            CreateObjectAtPosition(createBoxTriggerGUI, _menu, _hit, CreateBoxTrigger);
        }

        [SceneViewContextMenuItem(Order = ContextMenuOrder)]
        private static void CreateSphereTrigger(SceneView _, GenericMenu _menu, RaycastHit _hit) {
            CreateObjectAtPosition(createSphereTriggerGUI, _menu, _hit, CreateSphereTrigger);
        }

        [SceneViewContextMenuItem(Order = ContextMenuOrder)]
        private static void CreateLevelTrigger(SceneView _, GenericMenu _menu, RaycastHit _hit) {
            CreateObjectAtPosition(createLevelTriggerGUI, _menu, _hit, CreateLevelTrigger);
        }

        [SceneViewContextMenuItem(Order = ContextMenuOrder)]
        private static void CreateLevelTriggerArea(SceneView _, GenericMenu _menu, RaycastHit _hit) {
            CreateObjectAtPosition(createLevelTriggerAreaGUI, _menu, _hit, CreateLevelTriggerArea);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        private static GameObject CreateLevelTrigger(string _name, MenuCommand _command, params Type[] _components) {
            GameObject _object = CreateTrigger(_name, _command, _components);

            // Trigger.
            if (_object.TryGetComponent(out LevelTrigger _trigger)) {
                ComponentUtility.MoveComponentUp(_trigger);
            }

            return _object;
        }

        private static GameObject CreateTrigger(string _name, MenuCommand _command, params Type[] _components) {
            GameObject _object = CreateObject(_name, _command, _components);

            // Trigger.
            if (_object.TryGetComponent(out Collider _collider)) {
                _collider.isTrigger = true;
            }

            // Layer.
            if ((_object.layer == 0) && (TriggerLayer != -1)) {
                _object.layer = TriggerLayer;
            }

            return _object;
        }
        #endregion

        #region UI
        /// <summary>
        /// Creates a new <see cref="EnhancedButton"/> object.
        /// </summary>
        [MenuItem(CreateMenuPath + "UI/Enhanced Button", false, CreateMenuOrder)]
        public static GameObject CreateEnhancedButton(MenuCommand _command) {
            return CreateTrigger("Button", _command, typeof(RectTransform), typeof(EnhancedButton));
        }
        #endregion

        #region Creator
        public const string CreateMenuPath  = "GameObject/" + FrameworkUtility.MenuPath;
        public const string MenuPath        = SceneViewUtility.CreateGUI + FrameworkUtility.MenuPath;

        public const int ContextMenuOrder   = 9;
        public const int CreateMenuOrder    = -9;

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        private static void CreateObjectAtPosition(GUIContent _label, GenericMenu _menu, RaycastHit _hit, Func<MenuCommand, GameObject> _callback) {
            _menu.AddItem(_label, false, Create);

            // ----- Local Method ----- \\

            void Create() {
                MenuCommand _command = new MenuCommand(Selection.activeGameObject);
                GameObject _gameObject = _callback(_command);

                _gameObject.transform.position = _hit.point;
            }
        }

        private static GameObject CreateObject(string _name, MenuCommand _command, params Type[] _components) {
            return EnhancedEditorUtility.CreateObject(_name, _command.context as GameObject, _components);
        }
        #endregion
    }
}
