// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using EnhancedFramework.Core;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.SceneManagement;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.PlayMaker {
    /// <summary>
    /// <see cref="FsmStateAction"/> used to load a <see cref="EnhancedEditor.SceneBundle"/>.
    /// </summary>
    [Tooltip("Loads a Scene Bundle")]
    [ActionCategory("SceneBundle")]
    public sealed class LoadSceneBundle : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - Mode
        // -------------------------------------------

        [Tooltip("The Scene Bundle to load.")]
        [RequiredField, ObjectType(typeof(SceneBundle))]
        public FsmObject SceneBundle = null;

        [Tooltip("Allow you to specify whether or not to load the bundle additively.")]
        [RequiredField, ObjectType(typeof(LoadSceneMode))]
        public FsmEnum LoadSceneMode = null;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            LoadSceneMode = null;
            SceneBundle   = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (SceneBundle.Value is SceneBundle _bundle) {
                EnhancedSceneManager.Instance.LoadSceneBundle(_bundle, (LoadSceneMode)LoadSceneMode.Value);
            }

            Finish();
        }
        #endregion
    }
}
