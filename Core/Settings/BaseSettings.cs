// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[assembly: InternalsVisibleTo("EnhancedFramework.Core")]
namespace EnhancedFramework.Core.Settings {
    /// <summary>
    /// Base class to inherit all game settings from.
    /// </summary>
    /// <typeparam name="T">This class type.</typeparam>
    public abstract class BaseSettings<T> : ScriptableSettings where T : BaseSettings<T> {
        #region Content
        public const string MenuPrefix  = "GS_";
        public const string MenuPath    = FrameworkUtility.MenuPath + "Game Settings/";
        public const int MenuOrder      = FrameworkUtility.MenuOrder;

        private static T instance = null;

        /// <summary>
        /// Global shared instance across the entiere game.
        /// </summary>
        public static T I {
            get {
                #if UNITY_EDITOR
                if (!Application.isPlaying && (instance == null) && AssetDatabase.FindAssets($"t:{typeof(T).Name}").SafeFirst(out string _guid)) {
                    instance = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_guid), typeof(T)) as T;
                }
                #endif

                return instance;
            }
            protected set {
                instance = value;
            }
        }

        // -----------------------

        internal protected override void Init() {
            I = this as T;
        }
        #endregion
    }

    /// <summary>
    /// Base class to inherit all game database from.
    /// </summary>
    /// <typeparam name="T">This class type.</typeparam>
    public abstract class BaseDatabase<T> : BaseSettings<T> where T : BaseDatabase<T> {
        #region Content
        public new const string MenuPrefix = "DT_";
        public new const string MenuPath   = FrameworkUtility.MenuPath + "Game Database/";
        #endregion
    }
}
