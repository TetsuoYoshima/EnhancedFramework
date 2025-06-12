// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[assembly: InternalsVisibleTo("EnhancedFramework.Editor")]
namespace EnhancedFramework.Timeline {
    /// <summary>
    /// Interface to inherit any <see cref="PlayableAsset"/> using a binding object from.
    /// </summary>
    public interface IBindingPlayableAsset {
        #region Content
        /// <summary>
        /// The binding <see cref="Object"/> of this playable asset track.
        /// </summary>
        Object BindingObject { get; set; }
        #endregion
    }

    /// <summary>
    /// Base non-generic Enhanced <see cref="PlayableAsset"/> class.
    /// </summary>
    public abstract class EnhancedPlayableAsset : PlayableAsset {
        #region Utility
        /// <summary>
        /// Default name of any created clip of this <see cref="PlayableAsset"/> type.
        /// </summary>
        public virtual string ClipDefaultName {
            get { return string.Empty; }
        }

        /// <summary>
        /// If true, automatically serializes this asset binding object
        /// in a <see cref="EnhancedPlayableBindingData"/> instance, on the same <see cref="PlayableDirector"/> <see cref="GameObject"/>.
        /// </summary>
        public virtual bool SerializeBindingInComponent {
            get { return false; }
        }

        // -----------------------

        /// <summary>
        /// Called the first time this clip is created in the editor.
        /// <br/> Use this to initialize this clip default values.
        /// </summary>
        internal protected virtual void OnCreated(TimelineClip _clip) {
            string _name = ClipDefaultName;

            if (!string.IsNullOrEmpty(_name)) {
                _clip.displayName = _name;
            }
        }
        #endregion
    }

    /// <summary>
    /// Base generic class for every Enhanced <see cref="PlayableAsset"/>.
    /// </summary>
    /// <typeparam name="T"><see cref="EnhancedPlayableBehaviour"/> playable type for this asset.</typeparam>
    public abstract class EnhancedPlayableAsset<T> : EnhancedPlayableAsset where T : EnhancedPlayableBehaviour, new() {
        #region Global Members
        /// <summary>
        /// The <see cref="EnhancedPlayableBehaviour"/> template of this asset.
        /// </summary>
        [Enhanced, Block] public T Template = default;
        #endregion

        #region Behaviour
        public override Playable CreatePlayable(PlayableGraph _graph, GameObject _owner) {
            return ScriptPlayable<T>.Create(_graph, Template);
        }
        #endregion
    }

    /// <summary>
    /// Base generic class for every Enhanced <see cref="PlayableAsset"/> with a bound <see cref="Object"/>.
    /// </summary>
    /// <typeparam name="U">Clip bound <see cref="Object"/> type.</typeparam>
    /// <inheritdoc cref="EnhancedPlayableAsset{T}"/>
    public abstract class EnhancedPlayableAsset<T, U> : EnhancedPlayableAsset<T>, IBindingPlayableAsset where T : EnhancedPlayableBehaviour<U>, new()
                                                                                                        where U : Object {
        #region Global Members
        /// <summary>
        /// The <see cref="U"/> bound object of this playable.
        /// </summary>
        [SerializeField, HideInInspector] internal protected U bindingObject = null;

        // -----------------------

        /// <inheritdoc cref="IBindingPlayableAsset.BindingObject"/>
        public Object BindingObject {
            get { return bindingObject; }
            set { bindingObject = value as U; }
        }
        #endregion

        #region Behaviour
        public override Playable CreatePlayable(PlayableGraph _graph, GameObject _owner) {

            // Set bound object.
            Template.bindingObject = bindingObject;
            return base.CreatePlayable(_graph, _owner);
        }
        #endregion
    }
}
