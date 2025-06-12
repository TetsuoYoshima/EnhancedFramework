// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core.GameStates;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

#if UNITY_EDITOR
using UnityEditor;

using ArrayUtility = EnhancedEditor.ArrayUtility;
#endif

using SnapshotController = EnhancedFramework.Core.AudioWeightController<EnhancedFramework.Core.AudioSnapshotAsset>;
using AmbientController  = EnhancedFramework.Core.AudioWeightController<EnhancedFramework.Core.AudioAmbientController>;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Singleton class managing the game global audio system, and all associated <see cref="EnhancedAudioPlayer"/>.
    /// </summary>
    [ExecuteInEditMode]
    [ScriptGizmos(false, true)]
    [DefaultExecutionOrder(-955)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Audio/Audio Manager"), DisallowMultipleComponent]
    public sealed class AudioManager : EnhancedSingleton<AudioManager>, IStableUpdate, IGameStateOverrideCallback,
                                       IObjectPoolManager<EnhancedAudioPlayer>, IObjectPoolManager<MusicPlayer>, 
                                       IObjectPoolManager<AmbientController>,   IObjectPoolManager<AmbientSoundPlayer>,
                                       IObjectPoolManager<SnapshotController> {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init | UpdateRegistration.Stable;

        #region Global Members
        [Section("Audio Manager")]

        [Tooltip("Reference of the listener in game")]
        [SerializeField, Enhanced, Required] private AudioListener listener = null;

        [Tooltip("Asset used for audio mixing in game")]
        [SerializeField, Enhanced, Required] private AudioMixer mixer = null;

        [Space(10f)]

        [Tooltip("Audio snapshot default asset")]
        [SerializeField, Enhanced, Required] private AudioSnapshotAsset defaultSnapshot = null;

        [Space(10f)]

        [Tooltip("Audio player used to play preview audios")]
        [SerializeField, Enhanced, Required] private EnhancedAudioPlayer previewPlayer = null;

        [Tooltip("Pool transform used for parenting audio objects")]
        [SerializeField, Enhanced, Required] private Transform poolRoot = null;

        [Space(10f)]

        [Tooltip("All mixer groups that should continue playing while the game is paused")]
        [SerializeField] private AudioMixerGroup[] ignorePauseMixers = new AudioMixerGroup[0];

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Tags used to detect if an audio-related trigger action should be performed or not (e.g. Snapshot & Ambiant Controllers)")]
        [SerializeField] private TagGroup triggerTags = new TagGroup();

        [Tooltip("If true, pauses all audio players when pausing audio")]
        [SerializeField] private bool pauseAudioPlayers = true;

        // -----------------------

        /// <summary>
        /// Is the game audio currently paused?
        /// </summary>
        public static bool IsPaused {
            get { return AudioListener.pause; }
            private set { AudioListener.pause = value; }
        }

        /// <summary>
        /// <see cref="AudioListener"/> of the game.
        /// </summary>
        public AudioListener Listener {
            get { return listener; }
        }

        /// <summary>
        /// Current position of the game <see cref="AudioListener"/>.
        /// </summary>
        public Vector3 ListenerPosition {
            get { return listener.transform.position; }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
            #endif

            // Registration.
            GameStateManager.Instance.RegisterOverrideCallback(this);
        }

        protected override void OnInit() {
            base.OnInit();

            // Initialization.
            ambientControllerPool.Initialize(this);
            ambientSoundPool     .Initialize(this);
            snapshotPool         .Initialize(this);
            audioPool            .Initialize(this);
            musicPool            .Initialize(this);

            ResetSnapshots();

            EnhancedSceneManager.OnStartLoading += OnStartLoading;
        }

        void IStableUpdate.Update() {

            Pause(ChronosManager.Instance.Chronos == 0f);

            UpdateAudioPlayers();
            UpdateMusic();
            UpdateAmbients();
            UpdateSnapshots();
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
            #endif

            // Unregistration.
            GameStateManager.Instance.UnregisterOverrideCallback(this);
        }

        // -------------------------------------------
        // Callback
        // -------------------------------------------

        private void OnStartLoading() {

            // Stop non persistent players.
            ref List<EnhancedAudioPlayer> _span = ref audioPlayers;

            for (int i = _span.Count; i-- > 0;) {
                EnhancedAudioPlayer _player = _span[i];

                if (!_player.IsPersistent) {
                    _player.Stop(false);
                }
            }
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------
        private readonly ManualCooldown editorCooldown = new ManualCooldown(.1f);

        protected override void OnEnable() {
            base.OnEnable();

            if (Application.isPlaying) {
                return;
            }

            EditorApplication.update += OnEditorUpdate;
        }

        protected override void OnDisable() {
            base.OnDisable();

            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate() {
            if (!previewPlayer) {
                return;
            }

            // Cooldown (editor coroutine may throw exceptions if called too often).
            editorCooldown.Update(ChronosUtility.RealDeltaTime);
            if (!editorCooldown.IsValid) {
                return;
            }

            editorCooldown.Reload();
            previewPlayer.AudioUpdate();
        }
        #endif
        #endregion

        #region Game State
        void IGameStateOverrideCallback.OnGameStateOverride(in GameStateOverride _state) {
            Pause(_state.IsPaused);
        }
        #endregion

        #region Pause
        /// <summary>
        /// Pauses / unpauses the game audio.
        /// </summary>
        /// <param name="_pause">True to pause audio, false to unpause.</param>
        [Button(ActivationMode.Play, SuperColor.Orange)]
        public void Pause(bool _pause) {
            if (IsPaused == _pause) {
                return;
            }

            IsPaused = _pause;

            // Player pause.
            if (pauseAudioPlayers) {

                ref List<EnhancedAudioPlayer> _span = ref audioPlayers;
                for (int i = _span.Count; i-- > 0;) {

                    EnhancedAudioPlayer _player = _span[i];

                    // Ignore.
                    if (_player.IgnorePause) {
                        continue;
                    }

                    // Pause / Resume.
                    if (_pause) {
                        _player.Pause(true);
                    } else if (_player.Status == EnhancedAudioPlayer.State.Paused) {
                        _player.Play();
                    }
                }
            }            
        }
        #endregion

        // ===== Pool, Players & Controllers ===== \\

        #region Audio Player
        private const int AudioPlayerPoolInitialCapacity = 10;

        private static readonly ObjectPool<EnhancedAudioPlayer> audioPool = new ObjectPool<EnhancedAudioPlayer>(AudioPlayerPoolInitialCapacity);
        private static          List<EnhancedAudioPlayer> audioPlayers    = new List<EnhancedAudioPlayer>();

        // -----------------------

        /// <inheritdoc cref="Play(AudioAsset, AudioAssetSettings, Vector3)"/>
        public AudioHandler Play(AudioAsset _audio, AudioAssetSettings _settings) {
            return Play(_audio, _settings, ListenerPosition);
        }

        /// <param name="_position">Position (in world space) where to play this audio.</param>
        /// <inheritdoc cref="Play(AudioAsset, AudioAssetSettings, Transform)"/>
        public AudioHandler Play(AudioAsset _audio, AudioAssetSettings _settings, Vector3 _position) {
            return GetAudioPlayerFromPool().Play(_audio, _settings, _position);
        }

        /// <summary>
        /// Plays a specific <see cref="AudioAsset"/> in game.
        /// </summary>
        /// <param name="_transform"><see cref="Transform"/> reference for this audio to follow.</param>
        /// <param name="_settings"><see cref="AudioAssetSettings"/> used to play this audio.</param>
        /// <returns><see cref="AudioHandler"/> wrapper of the current play operation.</returns>
        public AudioHandler Play(AudioAsset _audio, AudioAssetSettings _settings, Transform _transform) {
            return GetAudioPlayerFromPool().Play(_audio, _settings, _transform);
        }

        // -------------------------------------------
        // Update
        // -------------------------------------------

        /// <summary>
        /// Updates all active audio players.
        /// </summary>
        private void UpdateAudioPlayers() {
            ref List<EnhancedAudioPlayer> _span = ref audioPlayers;

            for (int i = _span.Count; i-- > 0;) {
                _span[i].AudioUpdate();
            }
        }

        // -------------------------------------------
        // Preview
        // -------------------------------------------

        /// <summary>
        /// Plays a 2D preview of a specific <see cref="AudioAsset"/> (mostly used in editor).
        /// </summary>
        /// <param name="_audio"><see cref="AudioAsset"/> to preview.</param>
        internal void PlayPreview(AudioAsset _audio) {

            EnhancedAudioPlayer _preveiw = previewPlayer;

            if (_preveiw.Status == EnhancedAudioPlayer.State.Paused) {
                _preveiw.Play();
                return;
            }

            Vector3 _position = listener.IsValid() ? ListenerPosition : Vector3.zero;
            _preveiw.Play(_audio, _audio.Settings, _position);
        }

        /// <summary>
        /// Pauses a specific <see cref="AudioAsset"/> preview.
        /// </summary>
        /// <param name="_"><see cref="AudioAsset"/> to pause.</param>
        internal void PausePreview(AudioAsset _) {
            previewPlayer.Pause(false);
        }

        /// <summary>
        /// Stops a specific <see cref="AudioAsset"/> preview.
        /// </summary>
        /// <param name="_"><see cref="AudioAsset"/> to pause.</param>
        internal void StopPreview(AudioAsset _) {
            previewPlayer.Stop(false);
        }

        // -------------------------------------------
        // Registration
        // -------------------------------------------

        /// <summary>
        /// Registers a specific active <see cref="EnhancedAudioPlayer"/> instance.
        /// </summary>
        /// <param name="_player"><see cref="EnhancedAudioPlayer"/> to register.</param>
        internal static void RegisterPlayer(EnhancedAudioPlayer _player) {
            audioPlayers.Add(_player);
        }

        /// <summary>
        /// Unregisters a specific <see cref="EnhancedAudioPlayer"/> instance.
        /// </summary>
        /// <param name="_player"><see cref="EnhancedAudioPlayer"/> to unregister.</param>
        internal static void UnregisterPlayer(EnhancedAudioPlayer _player) {
            audioPlayers.RemoveSwap(_player);
        }

        // -------------------------------------------
        // Pool
        // -------------------------------------------

        private EnhancedAudioPlayer GetAudioPlayerFromPool() {
            return audioPool.GetPoolInstance();
        }

        internal bool ReleaseAudioPlayerToPool(EnhancedAudioPlayer _player) {
            return audioPool.ReleasePoolInstance(_player);
        }

        private void ClearAudioPlayerPool() {
            audioPool.ClearPool(AudioPlayerPoolInitialCapacity);
        }

        // -----------------------

        EnhancedAudioPlayer IObjectPool<EnhancedAudioPlayer>.GetPoolInstance() {
            return GetAudioPlayerFromPool();
        }

        bool IObjectPool<EnhancedAudioPlayer>.ReleasePoolInstance(EnhancedAudioPlayer _instance) {
            return ReleaseAudioPlayerToPool(_instance);
        }

        EnhancedAudioPlayer IObjectPoolManager<EnhancedAudioPlayer>.CreateInstance() {
            GameObject _gameObject = new GameObject("ADP_NewPlayer");
            Transform _transform   = _gameObject.transform;

            _transform.SetParent(poolRoot);
            _transform.ResetLocal();

            return _gameObject.AddComponent<EnhancedAudioPlayer>();
        }

        void IObjectPoolManager<EnhancedAudioPlayer>.DestroyInstance(EnhancedAudioPlayer _instance) {
            Destroy(_instance.gameObject);
        }
        #endregion

        #region Music
        // -------------------------------------------
        // How it works:
        //
        //  Musics are stored in a buffer, which is sorted according to their respective layer.
        //  Only the music on the higher layer is the one actively played.
        //
        //  When a new music is played on the same layer than another music,
        //  the previous one is stopped and is replaced by the new music.
        //
        //  Other musics on a lower level are interrupted (paused or stopped),
        //  but remain active on their respective layer (they are only sleeping).
        //
        //  So when their layer become the active one, they can be resumed and played again.
        //
        //  Music start, interruptions, transitions and volume adjustments are all made in the update.
        // -------------------------------------------

        private const int MusicPlayerPoolInitialCapacity = 1;

        private static readonly ObjectPool<MusicPlayer> musicPool           = new ObjectPool<MusicPlayer>(MusicPlayerPoolInitialCapacity);
        private static readonly EnhancedCollection<MusicPlayer> musicBuffer = new EnhancedCollection<MusicPlayer>();

        // -----------------------

        /// <inheritdoc cref="PlayMusic(AudioMusicAsset, AudioLayer, MusicInterruption)"/>
        public MusicHandler PlayMusic(AudioMusicAsset _music) {
            return PlayMusic(_music, _music.Layer);
        }

        /// <inheritdoc cref="PlayMusic(AudioMusicAsset, AudioLayer, MusicInterruption)"/>
        public MusicHandler PlayMusic(AudioMusicAsset _music, AudioLayer _layer) {
            return PlayMusic(_music, _layer, _music.InterruptionMode);
        }

        /// <summary>
        /// Plays a specific <see cref="AudioMusicAsset"/>.
        /// <br/> Note that only the music on highest layer priority is the one actively played.
        /// </summary>
        /// <param name="_music"><see cref="AudioMusicAsset"/> to play.</param>
        /// <param name="_layer"><see cref="AudioLayer"/> on which to play this music.</param>
        /// <param name="_interruptionMode">Determines how to interrupt musics playing on a lower layer.</param>
        public MusicHandler PlayMusic(AudioMusicAsset _music, AudioLayer _layer, MusicInterruption _interruptionMode) {

            int _playerIndex = -1;
            int _index;

            ref List<MusicPlayer> _span = ref musicBuffer.collection;
            int _count = _span.Count;

            for (_index = 0; _index < _count; _index++) {

                MusicPlayer _musicPlayer = _span[_index];
                AudioLayer _musicLayer   = _musicPlayer.Layer;

                // Lower priority.
                if (_musicLayer < _layer) {
                    continue;
                }

                // Higher priority.
                if (_musicLayer > _layer) {
                    break;
                }

                if (_musicPlayer.Music == _music) {

                    // Use the same player.
                    _musicPlayer.SetOverridden(false);
                    _playerIndex = _index;

                    continue;
                }

                // Override musics on the same layer.
                _musicPlayer.SetOverridden(true);
            }

            MusicPlayer _player;

            // Insert in buffer.
            if (_playerIndex == -1) {

                _player = GetMusicPlayerFromPool().Setup(_music, _layer, _interruptionMode);
                _span.Insert(_index, _player);

            } else {

                _player = _span[_playerIndex];
                musicBuffer.Move(_playerIndex, _index);
            }

            this.LogMessage($"Play Music - {_music.name.Bold()}   ({_layer})");
            return new MusicHandler(_player);
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        /// <summary>
        /// Stops playing all instances of a specific music.
        /// </summary>
        /// <inheritdoc cref="StopMusic(AudioMusicAsset, AudioLayer, bool)"/>
        public void StopMusic(AudioMusicAsset _music, bool _instant = false) {

            ref List<MusicPlayer> _span = ref musicBuffer.collection;
            int _count = _span.Count;

            while (FindIndex(ref _span, _count, out int _index)) {
                StopMusic_Internal(_index, _instant);
            }

            // ----- Local Method ----- \\

            bool FindIndex(ref List<MusicPlayer> _players, int _count, out int _index) {

                for (_index = _count; _index-- > 0;) {
                    if (_players[_index].Music == _music) {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Stops playing all musics on a specific layer.
        /// </summary>
        /// <inheritdoc cref="StopMusic(AudioMusicAsset, AudioLayer, bool)"/>
        public void StopMusic(AudioLayer _layer, bool _instant = false) {

            ref List<MusicPlayer> _span = ref musicBuffer.collection;
            int _count = _span.Count;

            while (FindIndex(ref _span, _count, out int _index)) {
                StopMusic_Internal(_index, _instant);
            }

            // ----- Local Method ----- \\

            bool FindIndex(ref List<MusicPlayer> _players, int _count, out int _index) {

                for (_index = _count; _index-- > 0;) {
                    if (_players[_index].Layer == _layer) {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Stops playing a given music if playing on a specific layer.
        /// </summary>
        /// <param name="_music"><see cref="AudioMusicAsset"/> to stop.</param>
        /// <param name="_layer"><see cref="AudioLayer"/> on which to stop the music.</param>
        /// <param name="_instant">If true, instantly stops the music.</param>
        public void StopMusic(AudioMusicAsset _music, AudioLayer _layer, bool _instant = false) {
           
            if (FindIndex(out int _index)) {
                StopMusic_Internal(_index, _instant);
            }

            // ----- Local Method ----- \\

            bool FindIndex(out int _index) {

                ref List<MusicPlayer> _span = ref musicBuffer.collection;
                int _count = _span.Count;

                for (_index = _count; _index-- > 0;) {

                    MusicPlayer _player = _span[_index];
                    if ((_player.Music == _music) && (_player.Layer == _layer)) {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Stops from playing all active game musics.
        /// </summary>
        /// <param name="_instant">If true, instantly stops them.</param>
        public void ClearMusics(bool _instant = false) {

            for (int i = musicBuffer.Count; i-- > 0;) {
                StopMusic_Internal(i, _instant);
            }
        }

        // -------------------------------------------
        // Update
        // -------------------------------------------

        /// <summary>
        /// Updates all active musics in game.
        /// </summary>
        private void UpdateMusic() {

            MusicInterruption _interruption = MusicInterruption.None;
            bool _instant = false;
            float _volume = 1f;

            ref List<MusicPlayer> _span = ref musicBuffer.collection;

            for (int i = _span.Count; i-- > 0;) {
                _volume *= _span[i].Update(ref _interruption, ref _instant, _volume);
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get if any music is currently playing on a lower layer.
        /// </summary>
        /// <param name="_player"><see cref="MusicPlayer"/> to check for.</param>
        /// <returns>True if any music is playing on a lower layer, false otherwise.</returns>
        internal bool IsMusicPlayingOnLowerLayer(MusicPlayer _player) {

            ref List<MusicPlayer> _span = ref musicBuffer.collection;
            int _index = _span.IndexOf( _player);

            if (_index == -1) {
                return false;
            }

            for (int i = _index; i-- > 0;) {
                if (_span[i].IsPlaying) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Called when a <see cref="MusicPlayer"/> is definitely stopped.
        /// </summary>
        /// <param name="_music">Stopped <see cref="MusicPlayer"/>.</param>
        internal void OnMusicStopped(MusicPlayer _music) {
            musicBuffer.Remove(_music);
            ReleaseMusicPlayerToPool(_music);

            this.LogMessage($"Stop Music - {_music.Music.name.Bold()}   ({_music.Layer})");
        }

        /// <summary>
        /// Stops a music at a given index.
        /// </summary>
        private void StopMusic_Internal(int _index, bool _instant) {
            musicBuffer[_index].StopMusic(_instant);
        }

        // -------------------------------------------
        // Pool
        // -------------------------------------------

        private MusicPlayer GetMusicPlayerFromPool() {
            return musicPool.GetPoolInstance();
        }

        private bool ReleaseMusicPlayerToPool(MusicPlayer _player) {
            return musicPool.ReleasePoolInstance(_player);
        }

        private void ClearMusicPlayerPool() {
            musicPool.ClearPool(MusicPlayerPoolInitialCapacity);
        }

        // -----------------------

        MusicPlayer IObjectPool<MusicPlayer>.GetPoolInstance() {
            return GetMusicPlayerFromPool();
        }

        bool IObjectPool<MusicPlayer>.ReleasePoolInstance(MusicPlayer _instance) {
            return ReleaseMusicPlayerToPool(_instance);
        }

        MusicPlayer IObjectPoolManager<MusicPlayer>.CreateInstance() {
            return new MusicPlayer();
        }

        void IObjectPoolManager<MusicPlayer>.DestroyInstance(MusicPlayer _instance) {
            // Cannot destroy the instance, so simply ignore the object and wait for the garbage collector to pick it up.
        }
        #endregion

        #region Ambient
        private const int AmbientControllerPoolInitialCapacity  = 1;
        private const int AmbientSoundPoolInitialCapacity       = 5;

        private static readonly ObjectPool<AmbientController> ambientControllerPool = new ObjectPool<AmbientController>(AmbientControllerPoolInitialCapacity);
        private static readonly ObjectPool<AmbientSoundPlayer> ambientSoundPool     = new ObjectPool<AmbientSoundPlayer>(AmbientSoundPoolInitialCapacity);

        private static readonly EnhancedCollection<AmbientController> ambientBuffer = new EnhancedCollection<AmbientController>();

        // -----------------------

        /// <summary>
        /// Pushes and applies this <see cref="AudioAmbientController"/> into the audio game stack.
        /// <br/> Uses their respective weight and priority for mixing.
        /// </summary>
        /// <param name="_ambient"><see cref="AudioAmbientController"/> to push.</param>
        /// <param name="_instant">If true, instantly set this ambient weight.</param>
        public void PushAmbient(AudioAmbientController _ambient, bool _instant = false) {

            int _priority = _ambient.Priority;

            ref List<AmbientController> _span = ref ambientBuffer.collection;
            int _count = _span.Count;

            // Because the ambient might not instantly be removed from the buffer when unregistered (fade out duration),
            // check if its associated controller already exist.
            if (!GetPriorityController(ref _span, _count, _priority, out int _index)) {
                _index = _count;
            }

            // Insert in buffer.
            if (GetAmbientController(_ambient, out AmbientController _controller, out int _ambientIndex)) {

                _controller.SetWeight(_ambient.Weight, _instant ? 0f : _ambient.FadeInDuration);

                ambientBuffer.Move(_ambientIndex, _index);

            } else {

                _controller = GetAmbientFromPool();
                _controller.Initialize(_ambient, 0f, _priority, _instant);

                _ambient.SetAmbientWeight(0f);

                ambientBuffer.Insert(_index, _controller);
            }

            this.LogMessage($"Play Ambient - {_ambient.name.Bold()}   ({_priority})");

            // ----- Local Method ----- \\

            static bool GetPriorityController(ref List<AmbientController> _ambients, int _count, int _priority, out int _index) {

                for (_index = 0; _index < _count; _index++) {
                    if (_ambients[_index].Priority > _priority) {
                        return true;
                    }
                }

                _index = -1;
                return false;
            }
        }

        /// <summary>
        /// Pops and removes this <see cref="AudioAmbientController"/> from the audio game stack.
        /// </summary>
        /// <param name="_ambient"><see cref="AudioAmbientController"/> to pop.</param>
        /// <param name="_instant">If true, instantly fades out this ambient.</param>
        public void PopAmbient(AudioAmbientController _ambient, bool _instant = false) {

            if (!GetAmbientController(_ambient, out AmbientController _controller, out _))
                return;

            _controller.Stop(_instant, OnComplete);
            this.LogMessage($"Stop Ambient - {_ambient.name.Bold()}");

            // ----- Local Method ----- \\

            void OnComplete(bool _success) {

                if (_success && GetAmbientController(_ambient, out AmbientController _controller, out int _index)) {

                    ReleaseAmbientToPool(_controller);
                    ambientBuffer.RemoveAt(_index);
                }
            }
        }

        /// <summary>
        /// Updates the weight of a specific <see cref="AudioAmbientController"/>.
        /// </summary>
        /// <param name="_ambient"><see cref="AudioAmbientController"/> to update weight.</param>
        /// <param name="_weight">New weight of this ambient.</param>
        /// <param name="_duration">Transition duration from the current weight to its new value (in seconds).</param>
        public void SetAmbientWeight(AudioAmbientController _ambient, float _weight, float _duration) {

            if (!GetAmbientController(_ambient, out AmbientController _controller, out _))
                return;

            _controller.SetWeight(_weight, _duration);
        }

        // -----------------------

        private bool GetAmbientController(AudioAmbientController _ambient, out AmbientController _controller, out int _index) {
            ref List<AmbientController> _span = ref ambientBuffer.collection;
            int _count = _span.Count;

            for (_index = 0; _index < _count; _index++) {

                _controller = _span[_index];
                if (_controller.Object == _ambient) {
                    return true;
                }
            }

            _controller = null;
            return false;
        }

        // -------------------------------------------
        // Update
        // -------------------------------------------

        /// <summary>
        /// Updates all active audio ambients.
        /// </summary>
        private void UpdateAmbients() {

            ref List<AmbientController> _span = ref ambientBuffer.collection;
            int _count = _span.Count;

            if (_count == 0) {
                return;
            }

            float _priority = _span.Last().Priority;
            float _weightCoef = 1f;

            // Weight blend.
            for (int i = _count; i-- > 0;) {

                AmbientController _ambient = _span[i];
                if (_ambient.Priority != _priority) {

                    _weightCoef *= (_ambient.Priority == 0) ? 0f : (_ambient.Priority / _priority);
                    _priority = _ambient.Priority;
                }

                float _weight = Mathf.Max(0f, _ambient.Weight * _weightCoef);
                _ambient.Object.SetAmbientWeight(_weight);
            }
        }

        // -------------------------------------------
        // Pool
        // -------------------------------------------

        private AmbientController GetAmbientFromPool() {
            return ambientControllerPool.GetPoolInstance();
        }

        private bool ReleaseAmbientToPool(AmbientController _ambient) {
            return ambientControllerPool.ReleasePoolInstance(_ambient);
        }

        private void ClearAmbientPool() {
            ambientControllerPool.ClearPool(AmbientControllerPoolInitialCapacity);
        }

        // -----------------------

        AmbientController IObjectPool<AmbientController>.GetPoolInstance() {
            return GetAmbientFromPool();
        }

        bool IObjectPool<AmbientController>.ReleasePoolInstance(AmbientController _instance) {
            return ReleaseAmbientToPool(_instance);
        }

        AmbientController IObjectPoolManager<AmbientController>.CreateInstance() {
            return new AmbientController();
        }

        void IObjectPoolManager<AmbientController>.DestroyInstance(AmbientController _instance) {
            // Cannot destroy the instance, so simply ignore the object and wait for the garbage collector to pick it up.
        }

        // -------------------------------------------
        // Sound Pool
        // -------------------------------------------

        internal AmbientSoundPlayer GetAmbientSoundFromPool() {
            return ambientSoundPool.GetPoolInstance();
        }

        internal bool ReleaseAmbientSoundToPool(AmbientSoundPlayer _sound) {
            return ambientSoundPool.ReleasePoolInstance(_sound);
        }

        private void ClearAmbientSoundPool() {
            ambientSoundPool.ClearPool(AmbientSoundPoolInitialCapacity);
        }

        // -----------------------

        AmbientSoundPlayer IObjectPool<AmbientSoundPlayer>.GetPoolInstance() {
            return GetAmbientSoundFromPool();
        }

        bool IObjectPool<AmbientSoundPlayer>.ReleasePoolInstance(AmbientSoundPlayer _instance) {
            return ReleaseAmbientSoundToPool(_instance);
        }

        AmbientSoundPlayer IObjectPoolManager<AmbientSoundPlayer>.CreateInstance() {
            return new AmbientSoundPlayer();
        }

        void IObjectPoolManager<AmbientSoundPlayer>.DestroyInstance(AmbientSoundPlayer _instance) {
            // Cannot destroy the instance, so simply ignore the object and wait for the garbage collector to pick it up.
        }
        #endregion

        #region Snapshot
        private const int SnapshotPoolInitialCapacity = 1;

        private static readonly ObjectPool<SnapshotController> snapshotPool             = new ObjectPool<SnapshotController>(SnapshotPoolInitialCapacity);
        private static readonly EnhancedCollection<SnapshotController> snapshotBuffer   = new EnhancedCollection<SnapshotController>();

        private static AudioMixerSnapshot[] snapshotMixerParameters = new AudioMixerSnapshot[0];
        private static float[] snapshotWeightParameters             = new float[0];

        // -----------------------

        /// <inheritdoc cref="PushSnapshot(AudioSnapshotAsset, float, int, bool)"/>
        public void PushSnapshot(AudioSnapshotAsset _snapshot, bool _instant = false) {
            PushSnapshot(_snapshot, _snapshot.Weight, _snapshot.Priority, _instant);
        }

        /// <inheritdoc cref="PushSnapshot(AudioSnapshotAsset, float, int, bool)"/>
        public void PushSnapshot(AudioSnapshotAsset _snapshot, float _weight, bool _instant = false) {
            PushSnapshot(_snapshot, _weight, _snapshot.Priority, _instant);
        }

        /// <summary>
        /// Pushes and applies this <see cref="AudioSnapshotAsset"/> into the audio game stack.
        /// <br/> Uses their respective weight and priority for blending.
        /// </summary>
        /// <param name="_snapshot"><see cref="AudioSnapshotAsset"/> to push.</param>
        /// <param name="_weight">Weight of this snapshot (1 means fully active, 0 for inactive).</param>
        /// <param name="_priority">Priority of this snapshot. Snapshots with a higher priority can override others.</param>
        /// <param name="_instant">If true, instantly set this snapshot weight, ignoring any transition.</param>
        public void PushSnapshot(AudioSnapshotAsset _snapshot, float _weight, int _priority, bool _instant = false) {

            if (_weight == 0f) {
                PopSnapshot(_snapshot, _instant);
                return;
            }

            ref List<SnapshotController> _span = ref snapshotBuffer.collection;
            int _snapshotCount = _span.Count;

            // Find indexes.
            int _snapshotIndex = -1;
            int _index = -1;

            for (int i = 0; i < _snapshotCount; i++) {

                SnapshotController _temp = _span[i];

                if ((_snapshotIndex == -1) && (_temp.Object == _snapshot)) {
                    _snapshotIndex = i;

                    if (_index != -1)
                        break;
                }

                if ((_index == -1) && (_temp.Priority > _priority)) {
                    _index = i;

                    if (_snapshotIndex != -1)
                        break;
                }
            }

            if (_index == -1) {
                _index = _snapshotCount;
            }

            SnapshotController _controller;

            // Insert in buffer.
            if (_snapshotIndex == -1) {

                _controller = GetSnapshotFromPool();
                _controller.Initialize(_snapshot, _weight, _priority, _instant);

                snapshotBuffer.Insert(_index, _controller);

                this.LogMessage($"Apply Snapshot - {_snapshot.name.Bold()}   ({_priority})");
            } else {

                _controller = snapshotBuffer[_snapshotIndex];
                _controller.SetWeight(_weight, _instant ? 0f : _snapshot.UpdateDuration);

                snapshotBuffer.Move(_snapshotIndex, _index);
            }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        /// <summary>
        /// Pops and removes this <see cref="AudioSnapshotAsset"/> from the audio game stack.
        /// </summary>
        /// <param name="_snapshot"><see cref="AudioSnapshotAsset"/> to pop.</param>
        /// <param name="_instant">If true, instantly fades out this snapshot.</param>
        public void PopSnapshot(AudioSnapshotAsset _snapshot, bool _instant = false) {

            if (!GetSnapshotController(_snapshot, out SnapshotController _controller)) {
                return;
            }

            if (_controller.Object == defaultSnapshot) {
                this.LogWarningMessage("Cannot pop the default audio Snapshot");
                return;
            }

            _controller.Stop(_instant);
            this.LogMessage($"Pop Snapshot - {_snapshot.name.Bold()}");
        }

        /// <summary>
        /// Replaces all snapshots with a specific priority by a new snapshot.
        /// </summary>
        /// <param name="_priority">Priority of the existing snapshot to replace, and of the new snapshot to apply.</param>
        /// <inheritdoc cref="PushSnapshot(AudioSnapshotAsset, float, int, bool)"/>
        public void ReplaceSnapshot(AudioSnapshotAsset _snapshot, float _weight, int _priority, bool _instant = false) {

            ref List<SnapshotController> _span = ref snapshotBuffer.collection;

            // Remove snapshots with the same priority.
            for (int i = _span.Count; i-- > 0;) {

                SnapshotController _temp = _span[i];
                if ((_temp.Priority == _priority) && (_temp.Object != defaultSnapshot)) {

                    _temp.Stop(_instant);
                }
            }

            // Apply snapshot.
            PushSnapshot(_snapshot, _weight, _priority, _instant);
        }

        /// <summary>
        /// Resets the <see cref="AudioSnapshotAsset"/> buffer back to default.
        /// </summary>
        public void ResetSnapshots(bool _instant = false) {

            this.LogMessage($"Resets Snapshot ({snapshotBuffer.Count})");

            ref List<SnapshotController> _span = ref snapshotBuffer.collection;

            for (int i = _span.Count; i-- > 0;) {
                _span[i].Stop(_instant);
            }

            PushSnapshot(defaultSnapshot);
        }

        // -----------------------

        private bool GetSnapshotController(AudioSnapshotAsset _snapshot, out SnapshotController _controller) {

            ref List<SnapshotController> _span = ref snapshotBuffer.collection;
            int _snapshotCount = _span.Count;

            for (int i = 0; i < _snapshotCount; i++) {

                _controller = _span[i];
                if (_controller.Object == _snapshot)
                    return true;
            }

            _controller = null;
            return false;
        }

        // -------------------------------------------
        // Update
        // -------------------------------------------

        /// <summary>
        /// Updates all active audio snapshots.
        /// </summary>
        private void UpdateSnapshots() {

            ref List<SnapshotController> _span = ref snapshotBuffer.collection;
            int _count = _span.Count;

            float _totalWeight = 1f;
            bool _update = false;

            // Buffer resize.
            ref AudioMixerSnapshot[] _snapshots = ref snapshotMixerParameters;
            ref float[] _weights = ref snapshotWeightParameters;

            if (_snapshots.Length != _count) {

                Array.Resize(ref _snapshots, _count);
                Array.Resize(ref _weights, _count);

                _update = true;
            }

            // Blender.
            for (int i = _count; i-- > 0;) {

                SnapshotController _snapshot = _span[i];

                float _weight = Mathf.Max(0f, _snapshot.Weight * _totalWeight);
                _totalWeight  = Mathf.Max(0f, _totalWeight - _weight);

                // Check if any value changed.
                if (!_update && (_snapshots[i] != _snapshot.Object.MixerSnapshot) || !Mathf.Approximately(_weights[i], _weight)) {
                    _update = true;
                }

                _snapshots[i] = _snapshot.Object.MixerSnapshot;
                _weights[i]   = _weight;

                // Pop if not active.
                if ((_snapshot.Weight == 0f) && !_snapshot.IsFading) {

                    _span.RemoveAt(i);
                    ReleaseSnapshotToPool(_snapshot);
                }
            }

            if (_update) {
                mixer.TransitionToSnapshots(snapshotMixerParameters, _weights, 0f);
            }
        }

        // -------------------------------------------
        // Pool
        // -------------------------------------------

        private SnapshotController GetSnapshotFromPool() {
            return snapshotPool.GetPoolInstance();
        }

        private bool ReleaseSnapshotToPool(SnapshotController _player) {
            return snapshotPool.ReleasePoolInstance(_player);
        }

        private void ClearSnapshotPool() {
            snapshotPool.ClearPool(SnapshotPoolInitialCapacity);
        }

        // -----------------------

        SnapshotController IObjectPool<SnapshotController>.GetPoolInstance() {
            return GetSnapshotFromPool();
        }

        bool IObjectPool<SnapshotController>.ReleasePoolInstance(SnapshotController _instance) {
            return ReleaseSnapshotToPool(_instance);
        }

        SnapshotController IObjectPoolManager<SnapshotController>.CreateInstance() {
            return new SnapshotController();
        }

        void IObjectPoolManager<SnapshotController>.DestroyInstance(SnapshotController _instance) {
            // Cannot destroy the instance, so simply ignore the object and wait for the garbage collector to pick it up.
        }
        #endregion

        #region Pool
        IPoolableObject IObjectPool.GetPoolInstance() {
            return null;
        }

        bool IObjectPool.ReleasePoolInstance(IPoolableObject _instance) {

            if (_instance is EnhancedAudioPlayer _audioPlayer) {
                return ReleaseAudioPlayerToPool(_audioPlayer);
            }

            if (_instance is MusicPlayer _musicPlayer) {
                return ReleaseMusicPlayerToPool(_musicPlayer);
            }

            if (_instance is AmbientController _ambientController) {
                return ReleaseAmbientToPool(_ambientController);
            }

            if (_instance is AmbientSoundPlayer _soundPlayer) {
                return ReleaseAmbientSoundToPool(_soundPlayer);
            }

            if (_instance is SnapshotController _snapshotController) {
                return ReleaseSnapshotToPool(_snapshotController);
            }

            return false;
        }

        void IObjectPool.ClearPool(int _capacity) { }
        #endregion

        // ===== Other ===== \\

        #region Utility
        /// <summary>
        /// Get if a specific <see cref="AudioMixerGroup"/> should ignore pause, or continue playing.
        /// </summary>
        /// <param name="_mixer"><see cref="AudioMixerGroup"/> to check.</param>
        /// <returns>True if the specified mixer should ingore pause, false otherwise.</returns>
        public bool IgnorePause(AudioMixerGroup _mixer) {
            return ArrayUtility.Contains(ignorePauseMixers, _mixer);
        }

        /// <summary>
        /// Get if a specific <see cref="Component"/> has all the required audio-related triggers tags or not.
        /// <br/> These can especially be used to trigger Snapshot or Ambient controller.
        /// </summary>
        /// <param name="_object"><see cref="Component"/> to check tags.</param>
        /// <returns>True if the object has all the required trigger tags, false otherwise.</returns>
        public bool HasTriggerTags(Component _object) {
            return _object.HasTags(triggerTags);
        }

        /// <summary>
        /// Get if a specific <see cref="TagGroup"/> has all the required audio-related triggers tags or not.
        /// <br/> These can especially be used to trigger Snapshot or Ambient controller.
        /// </summary>
        /// <param name="_tags"><see cref="TagGroup"/> to check tags.</param>
        /// <returns>True if the group has all the required trigger tags, false otherwise.</returns>
        public bool HasTriggerTags(TagGroup _tags) {
            return _tags.ContainsAll(triggerTags);
        }
        #endregion

        #region Logger
        public override Color GetLogMessageColor(LogType _type) {
            return SuperColor.Pumpkin.Get();
        }
        #endregion
    }
}
