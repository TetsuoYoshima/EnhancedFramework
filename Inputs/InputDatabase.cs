// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if ENABLE_INPUT_SYSTEM
#define NEW_INPUT_SYSTEM
#endif

using EnhancedEditor;
using EnhancedFramework.Core.Settings;
using UnityEngine;

#if NEW_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EnhancedFramework.Inputs {
    /// <summary>
    /// Base class for any Enhanced input <see cref="ScriptableObject"/> asset.
    /// </summary>
    [CreateAssetMenu(fileName = FileName, menuName = MenuPath + "Input", order = MenuOrder)]
    public class InputDatabase : BaseDatabase<InputDatabase> {
        public const string FileName = MenuPrefix + "InputDatabase";

        #region Global Members
        [Section("Input Database")]

        #if NEW_INPUT_SYSTEM
        [SerializeField, Enhanced, Required] private InputActionAsset database = null;

        [Space(10f)]
        #endif

        [SerializeField] private InputActionEnhancedAsset[] inputs = new InputActionEnhancedAsset[0];
        [SerializeField] private InputMapEnhancedAsset[] maps      = new InputMapEnhancedAsset[0];

        // -----------------------

        #if NEW_INPUT_SYSTEM
        /// <summary>
        /// The new input system database object.
        /// </summary>
        public InputActionAsset ActionDatabase {
            get { return database; }
            internal set { database = value; }
        }
        #endif

        /// <summary>
        /// The total amount of <see cref="InputActionEnhancedAsset"/> in the game.
        /// </summary>
        public int InputCount {
            get { return inputs.Length; }
        }

        /// <summary>
        /// The total amount of <see cref="InputMapEnhancedAsset"/> in the game.
        /// </summary>
        public int InputMapCount {
            get { return maps.Length; }
        }
        #endregion

        #region Initialization
        protected override void Init() {
            base.Init();

            // Inputs may not be serialized with a consistant reference,
            // so use the database to retrieve their target input within.

            ref InputActionEnhancedAsset[] _inputSpan = ref inputs;
            for (int i = _inputSpan.Length; i-- > 0;) {
                _inputSpan[i].Initialize(this);
            }

            ref InputMapEnhancedAsset[] _mapSpan = ref maps;
            for (int i = _mapSpan.Length; i-- > 0;) {
                _mapSpan[i].Initialize(this);
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get the <see cref="InputActionEnhancedAsset"/> at a specific index.
        /// <para/>
        /// Use <see cref="InputCount"/> to get the total amount of input actions.
        /// </summary>
        /// <param name="_index">The index to get the input at.</param>
        /// <returns>The <see cref="InputActionEnhancedAsset"/> at the specified index.</returns>
        public InputActionEnhancedAsset GetActionAt(int _index) {
            return inputs[_index];
        }

        /// <summary>
        /// Get the <see cref="InputMapEnhancedAsset"/> at a specific index.
        /// <para/>
        /// Use <see cref="InputMapCount"/> to get the total amount of input action maps.
        /// </summary>
        /// <param name="_index">The index to get the input map at.</param>
        /// <returns>The <see cref="InputMapEnhancedAsset"/> at the specified index.</returns>
        public InputMapEnhancedAsset GetMapAt(int _index) {
            return maps[_index];
        }

        // -------------------------------------------
        // Editor Tool(s)
        // -------------------------------------------

        /// <summary>
        /// Seupts this database <see cref="InputActionEnhancedAsset"/> and <see cref="InputMapEnhancedAsset"/>.
        /// </summary>
        internal void Setup(InputActionEnhancedAsset[] _actions, InputMapEnhancedAsset[] _maps) {
            inputs = _actions;
            maps   = _maps;
        }
        #endregion
    }
}
