// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnhancedFramework.Editor {
    /// <summary>
    /// Editor class used to perform <see cref="LevelTriggerArea"/>-related editor operations, such as drawing handles.
    /// </summary>
    [InitializeOnLoad]
    internal static class LevelTriggerAreaDrawer {
        #region Content
        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        static LevelTriggerAreaDrawer() {

            EditorSceneManager.sceneOpened  += OnOpenedScene;
            EditorSceneManager.sceneSaved   += OnSavedScene;

            SceneManager.sceneLoaded        += OnLoadedScene;
            SceneView.duringSceneGui        += OnSceneGUI;

            GetTriggerAras();
        }

        // -------------------------------------------
        // Callbacks
        // -------------------------------------------

        private static void OnOpenedScene(Scene _scene, OpenSceneMode _mode) {
            GetTriggerAras();
        }

        private static void OnSavedScene(Scene _scene) {
            GetTriggerAras();
        }

        private static void OnLoadedScene(Scene _scene, LoadSceneMode _mode) {
            GetTriggerAras();
        }

        private static void OnSceneGUI(SceneView _sceneView) {

            ref List<LevelTriggerArea> _span = ref LevelTriggerArea.triggerAreas;
            for (int i = _span.Count; i-- > 0;) {

                if (_span[i] == null) {
                    _span.RemoveAt(i);
                    continue;
                }

                _span[i].DrawArea();
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        private static void GetTriggerAras() {
            LevelTriggerArea.triggerAreas.ReplaceBy(Object.FindObjectsByType<LevelTriggerArea>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        }
        #endregion
    }
}
