// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core.GameStates;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Game splash manager class.
    /// <para/>
    /// Should be loaded after and in a different Splash specific scene than Core.
    /// </summary>
    [ScriptGizmos(false, true)]
    [DefaultExecutionOrder(100)] // Execute late, after all other scripts are executed.
    [AddComponentMenu(FrameworkUtility.MenuPath + "General/Splash Manager"), DisallowMultipleComponent]
    public sealed class SplashManager : EnhancedSingleton<SplashManager>, ILoadingProcessor {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Play;

        #region Loading Processor
        public override bool IsLoadingProcessor => true;

        // True while playing any animation.
        public bool IsProcessing {
            get { return isPlaying; }
        }
        #endregion

        #region Global Members
        [Section("Splash Manager")]

        [SerializeField] private SerializedType<ISplashState> splashStateType = new SerializedType<ISplashState>(SerializedTypeConstraint.None, typeof(DefaultSplashGameState));
        [SerializeField, Enhanced, Required] private SceneBundle nextScene = null;

        [Space(10f)]

        [SerializeField] private PolymorphValue<SplashAnimation>[] animations = new PolymorphValue<SplashAnimation>[0];

        // This field is not meant to be used in any way.
        // It doesn't belong here, and it should be removed.
        // Yet, without hit, the game will crash on build when loading the Splash scene (even if the component is disabled).
        // Happens because of the List<PolymorphValue<SplashAnimation>> above.
        //
        // It needs to be a SerializeField PolymorphValue, its name doesn't matter.
        // I have absolutely no idea why with this field, the game doesn't crash. There isn't a single log for this.
        // This should require deeper investigation some day.
        //
        // See https://forum.unity.com/threads/unity-crashes-when-adding-an-element-to-a-list-of-serialized-interfaces.1100491/
        // for more informations.
        #pragma warning disable IDE0052
        [SerializeField, HideInInspector] private PolymorphValue<SplashAnimation> buildFix = new PolymorphValue<SplashAnimation>();

        // -----------------------

        private SceneBundle splashBundle = null;
        private bool isPlaying = false;
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Get this object scene bundle once fully loaded.
            EnhancedSceneManager.OnPostLoadBundle += OnBundleLoaded;
        }

        protected override void OnPlay() {
            base.OnPlay();
            StartCoroutine(PlaySplash());
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Unregister from callback if not yet done.
            if (splashBundle.IsNull()) {
                EnhancedSceneManager.OnPostLoadBundle -= OnBundleLoaded;
            }
        }

        // -------------------------------------------
        // Loading Callback
        // -------------------------------------------

        private void OnBundleLoaded(SceneBundle _bundle) {
            // Get this splash scene.
            if (_bundle.ContainScene(gameObject.scene, out _)) {

                splashBundle = _bundle;
                EnhancedSceneManager.OnPostLoadBundle -= OnBundleLoaded;
            }
        }
        #endregion

        #region Behaviour
        private const float TimeBeforeSplash = .6f;
        private GameState gameState = null;

        // -----------------------

        private IEnumerator PlaySplash() {
            // Wait some time before starting the splash to let the time for
            // all objects in the scene to be properly initialized.
            //
            // (The Update system make sure to not perform initializations and play for more than a fixed amount of time)
            yield return new WaitForSecondsRealtime(TimeBeforeSplash);

            gameState = GameState.CreateState(splashStateType);
            isPlaying = true;

            EnhancedSceneManager.Instance.LoadSceneBundle(nextScene, LoadSceneMode.Additive);

            // Play each animation.
            var _animations = animations;
            int _count = _animations.Length;

            for (int i = 0; i < _count; i++) {
                yield return _animations[i].Value.Play();
            }

            isPlaying = false;
            gameState.RemoveState();

            // Unload this splash scene and its content, as it will never be used again.
            this.Assert(splashBundle.IsValid());
            EnhancedSceneManager.Instance.UnloadSceneBundle(splashBundle, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        }
        #endregion
    }
}
