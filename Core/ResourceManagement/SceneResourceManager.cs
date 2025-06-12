// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object  = UnityEngine.Object;
using RLoader = EnhancedEditor.Reference<EnhancedFramework.Core.ResourceLoader>;

[assembly: InternalsVisibleTo("EnhancedFramework.Editor")]
namespace EnhancedFramework.Core {
    /// <summary>
    /// Base <see cref="ResourceLoader{T}"/>-related resource interface.
    /// <br/> Always prefer using <see cref="IResourceBehaviour{T}"/> instead of this interface when possible.
    /// <para/>
    /// This only exist to serialize all behaviours using a single non-generic type.
    /// </summary>
    public interface IResourceBehaviour {
        #region Content
        /// <summary>
        /// This resource instance object to use for logging messages to the console.
        /// </summary>
        Object LogObject { get; }
        #endregion
    }

    /// <summary>
    /// Generic interface to inherit your own <see cref="ResourceLoader{T}"/>-related resources from.
    /// </summary>
    /// <typeparam name="T">This behaviour related <see cref="ResourceLoader{T}"/> type.</typeparam>
    public interface IResourceBehaviour<T> : IResourceBehaviour where T : ResourceLoader, new() {
        #region Content
        /// <summary>
        /// Fills a specific <see cref="ResourceLoader"/> with this behaviour.
        /// </summary>
        /// <param name="_resource">The <see cref="ResourceLoader"/> to fill with this behaviour.</param>
        void FillResource(T _resource);
        #endregion
    }

    /// <summary>
    /// Base loading resource class.
    /// <br/> Always inherit from <see cref="ResourceLoader{T}"/> instead of this class.
    /// <para/>
    /// This only exist to serialize all loaders using a single non-generic type.
    /// </summary>
    [Serializable]
    public abstract class ResourceLoader {
        #region Global Members
        /// <summary>
        /// The total count of <see cref="IResourceBehaviour"/> in this loader.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Indicates whether this loader is currently loading or unloading any resource.
        /// </summary>
        public abstract bool IsProcessing { get; }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Prevents inheriting from this class in other assemblies.
        /// </summary>
        private protected ResourceLoader() { }
        #endregion

        #region Setup
        /// <summary>
        /// Registers a new resource behaviour instance in this loader.
        /// </summary>
        /// <param name="_behaviour">The <see cref="IResourceBehaviour"/> to register.</param>
        /// <returns>True if the behaviour could be successfully registered, false otherwise.</returns>
        internal abstract bool RegisterBehaviour(IResourceBehaviour _behaviour);

        /// <summary>
        /// Fills this loader with all its resources before loading.
        /// </summary>
        internal protected abstract void Setup();
        #endregion

        #region Behaviour
        /// <summary>
        /// Starts loading all resources in this loader.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Starts unloading all resources in this loader.
        /// </summary>
        public abstract void Unload();
        #endregion
    }

    /// <summary>
    /// Base class to inherit your own resource loaders from.
    /// <para/>
    /// Use this to dynamically load and unload resources from a scene.
    /// </summary>
    /// <typeparam name="T">This <see cref="ResourceLoader"/> class type.</typeparam>
    [Serializable]
    public abstract class ResourceLoader<T> : ResourceLoader where T : ResourceLoader, new() {
        #region Global Members
        /// <summary>
        /// All registered behaviours in this loader.
        /// </summary>
        public List<SerializedInterface<IResourceBehaviour<T>>> Behaviours = new List<SerializedInterface<IResourceBehaviour<T>>>();

        // -----------------------

        public override int Count {
            get { return Behaviours.Count; }
        }
        #endregion

        #region Setup
        internal override bool RegisterBehaviour(IResourceBehaviour _behaviour) {
            if (_behaviour is IResourceBehaviour<T> _resourceBehaviour) {
                Behaviours.Add(new SerializedInterface<IResourceBehaviour<T>>(_resourceBehaviour));
                return true;
            }

            return false;
        }

        internal protected override void Setup() {

            ref List<SerializedInterface<IResourceBehaviour<T>>> _span = ref Behaviours;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {
                _span[i].Interface.FillResource(this as T);
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get this loader <see cref="IResourceBehaviour{T}"/> at the given index.
        /// <para/>
        /// You can use <see cref="Count"/> to get the total amount of behaviours in this loader.
        /// </summary>
        /// <param name="_index">The index at which to get the desired behaviour.</param>
        /// <returns>The <see cref="IResourceBehaviour{T}"/> instance of this loader at the given index.</returns>
        public IResourceBehaviour<T> GetBehaviourAt(int _index) {
            return Behaviours[_index].Interface;
        }
        #endregion
    }

    // ===== Manager ===== \\

    /// <summary>
    /// Scene-depending manager class, used to automatically load and unload
    /// <br/> all resources associated with it following its lifetime.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "General/Scene Resource Manager"), DisallowMultipleComponent]
    public sealed class SceneResourceManager : EnhancedBehaviour, ILoadingProcessor {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init;

        #region Loading Processor
        public override bool IsLoadingProcessor => true;

        public bool IsProcessing {
            get {
                ref List<RLoader> _span = ref resources;
                for (int i = _span.Count; i-- > 0;) {
                    if (_span[i].Value.IsProcessing)
                        return true;
                }

                return false;
            }
        }
        #endregion

        #region Global Members
        [Section("Scene Resource Manager")]

        [SerializeField, Enhanced, ReadOnly] private List<RLoader> resources = new List<RLoader>();
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Registration.
            EnhancedSceneManager.OnPreUnloadBundle += OnPreUnloadBundle;
        }

        protected override void OnInit() {
            base.OnInit();

            // Get and load all resources during initialization.
            ref List<RLoader> _span = ref resources;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {
                ResourceLoader _resource = (ResourceLoader)_span[i];
                _resource.Setup();
                _resource.Load();
            }
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Unregistration.
            EnhancedSceneManager.OnPreUnloadBundle -= OnPreUnloadBundle;
        }

        // -------------------------------------------
        // Unloading Callback
        // -------------------------------------------

        private void OnPreUnloadBundle(SceneBundle _bundle) {
            // When this object scene is about to be unloaded, unload its associated resources.
            if (!IsSceneLoaded(ref _bundle.Scenes, gameObject.scene)) {
                return;
            }

            ref List<RLoader> _span = ref resources;
            for (int i = _span.Count; i-- > 0;) {
                _span[i].Value.Unload();
            }

            // ----- Local Method ----- \\

            static bool IsSceneLoaded(ref SceneAsset[] _scenes, Scene _scene) {

                for (int i = _scenes.Length; i-- > 0;) {
                    if (_scenes[i].Scene == _scene)
                        return true;
                }

                return false;
            }
        }
        #endregion

        #region Editor
        #if UNITY_EDITOR
        /// <summary>
        /// Called from an editor class to serialize all resources in the scene.
        /// </summary>
        internal void GetSceneResources() {
            // Each time this object scene is saved, register all resources in it.
            GameObject[] _objects = gameObject.scene.GetRootGameObjects();
            resources.Clear();

            foreach (GameObject _object in _objects) {
                IResourceBehaviour[] _behaviours = _object.GetComponentsInChildren<IResourceBehaviour>();

                foreach (IResourceBehaviour _behaviour in _behaviours) {
                    RegisterBehaviour(_behaviour);
                }
            }

            // ----- Local Method ----- \\

            void RegisterBehaviour(IResourceBehaviour _behaviour) {
                foreach (ResourceLoader _resource in resources) {
                    if (_resource.RegisterBehaviour(_behaviour)) {
                        return;
                    }
                }

                // If no appropriate loader could be found,
                // find the behaviour interface and create a new instance of its loader.
                Type[] _interfaceTypes = _behaviour.GetType().GetInterfaces();
                foreach (Type _type in _interfaceTypes) {

                    if (_type.IsGenericType && _type.GetGenericTypeDefinition() == typeof(IResourceBehaviour<>)) {

                        Type _resourceType = _type.GetGenericArguments()[0];
                        ResourceLoader _loader = Activator.CreateInstance(_resourceType) as ResourceLoader;

                        if (_loader.RegisterBehaviour(_behaviour)) {
                            resources.Add(_loader);
                            return;
                        }
                    }
                }
            }
        }
        #endif
        #endregion
    }
}
