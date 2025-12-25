// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

namespace EnhancedFramework.Core.Settings {
    /// <summary>
    /// <see cref="EnhancedObjectID"/> related global settings.
    /// </summary>
    [CreateAssetMenu(fileName = MenuPrefix + "AudioSettings", menuName = MenuPath + "Audio", order = MenuOrder)]
    public sealed class ObjectIdSettings : BaseSettings<AudioSettings> {
        #region Global Members
        [Section("Object ID Settings")]

        [Tooltip("Ensure that dynamically instantiated objects keep a stable ID when instantiated again in the same scene")]
        [SerializeField] private bool dynamicObjectStableID = false;
        #endregion

        #region Initialization
        protected internal override void Init() {
            base.Init();

            bool _stableID = dynamicObjectStableID;
            EnhancedObjectID.DynamicObjectStableID = _stableID;

            if (_stableID) {
                EnhancedSceneManager.OnPreUnloadBundle += OnUnloadBundle;
            }
        }

        // -------------------------------------------
        // Callback(s)
        // -------------------------------------------

        private void OnUnloadBundle(SceneBundle _sceneBundle) {
            EnhancedObjectID.OnUnloadSceneBundle(_sceneBundle);
        }
        #endregion
    }
}
