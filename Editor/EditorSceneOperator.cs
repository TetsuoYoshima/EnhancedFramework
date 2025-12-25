// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace EnhancedFramework.Editor {
    /// <summary>
    /// Editor class used to perform additional scene-related operations.
    /// </summary>
    [InitializeOnLoad]
    internal static class EditorSceneOperator {
        #region Content
        private static readonly List<Type> sceneOperatorTypes = new List<Type>();

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        static EditorSceneOperator() {
            // Registration.
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneSaving         += OnSavingScene;

            // Get inherited types.
            Type _baseType = typeof(IEnhancedSceneOperator);
            sceneOperatorTypes.Clear();

            Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var _assembly in _assemblies) {
                try {
                    Type[] _types = _assembly.GetTypes();

                    foreach (Type _type in _types) {

                        // Register.
                        if ((_type != _baseType) && _baseType.IsAssignableFrom(_type) && _type.IsClass && !_type.IsAbstract) {
                            sceneOperatorTypes.Add(_type);
                        }
                    }
                } catch { }
            }
        }

        // -------------------------------------------
        // Callback(s)
        // -------------------------------------------

        private static void OnSavingScene(Scene _scene, string _path) {

            // Get resources when saving a scene.
            foreach (Type _type in sceneOperatorTypes) {

                IEnhancedSceneOperator[] _instances = FindInstances<IEnhancedSceneOperator>(_type);
                int _instanceCount = 0;

                foreach (IEnhancedSceneOperator _instance in _instances) {
                    if (_scene != (_instance.LogObject as Component).gameObject.scene) {
                        continue;
                    }

                    _instance.GetSceneResources(true);
                    _instanceCount++;

                    // Log for when multiple instances are found in a scene.
                    if (_instanceCount != 1) {
                        _instance.LogObject.LogWarning($"Multiple \'{_instance.GetType().Name}\' instances ({_instanceCount}) found in the scene \'{_scene.name}\'. " +
                                                       $"Duplicate instances should be removed from the scene.");
                    }
                }
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _state) {
            if (_state != PlayModeStateChange.ExitingEditMode) {
                return;
            }

            // Get resources before entering play mode.
            foreach (Type _type in sceneOperatorTypes) {
                IEnhancedSceneOperator[] _instances = FindInstances<IEnhancedSceneOperator>(_type);
                foreach (IEnhancedSceneOperator _instance in _instances) {
                    _instance.GetSceneResources(true);
                }
            }
        }

        // -----------------------

        private static T[] FindInstances<T>(Type _type) {
            return Object.FindObjectsByType(_type, FindObjectsInactive.Include, FindObjectsSortMode.None) as T[];
        }
        #endregion
    }
}
