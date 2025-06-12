// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="Component"/> wrapper for a <see cref="TagGroup"/>.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Tag/Multi Tags")]
    public sealed class MultiTagsBehaviour : EnhancedBehaviour {
        #region Global Members
        [Section("Mutli-Tags")]

        public TagGroup Tags = new TagGroup();

        // -----------------------

        /// <summary>
        /// The total amount of tags in this object.
        /// </summary>
        public int Count {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Tags.Count; }
        }
        #endregion

        #region Operator
        public static implicit operator TagGroup(MultiTagsBehaviour _behaviour) {
            return _behaviour.Tags;
        }

        public Tag this[int _index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Tags[_index]; }
        }
        #endregion
    }
}
