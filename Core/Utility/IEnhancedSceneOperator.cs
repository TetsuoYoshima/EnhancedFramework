// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

namespace EnhancedFramework.Core {
    /// <summary>
    /// Interface used to get callback for editor scene-related operations, such as saving or entering play mode.
    /// </summary>
    public interface IEnhancedSceneOperator : IBaseUpdate {
        #region Content
        /// <summary>
        /// Get all associated resources in the scene.
        /// </summary>
        void GetSceneResources(bool _setDirty);
        #endregion
    }
}
