// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using Object = UnityEngine.Object;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Serializable <see cref="EnhancedBehaviour"/> data.
    /// </summary>
    [Serializable]
    public sealed class PlayModeEnhancedObjectData : PlayModeObjectData {
        #region Global Members
        [SerializeField] public List<Vector3> Vectors   = new List<Vector3>();
        [SerializeField] public List<string> Strings    = new List<string>();
        [SerializeField] public List<float> Floats      = new List<float>();
        [SerializeField] public List<bool> Bools        = new List<bool>();
        [SerializeField] public List<int> Ints          = new List<int>();

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        public PlayModeEnhancedObjectData() : base() { }
        #endregion

        #region Behaviour
        public override void Save(Object _object) {

            Vectors.Clear();
            Strings.Clear();
            Floats .Clear();
            Bools  .Clear();
            Ints   .Clear();

            if (_object is EnhancedBehaviour _behaviour) {
                _behaviour.SavePlayModeData(this);
            }

            base.Save(_object);
        }

        public override bool Load(Object _object) {

            if (_object is EnhancedBehaviour _behaviour) {

                _behaviour.LoadPlayModeData(this);
                return true;
            }

            return false;
        }
        #endregion
    }

    /// <summary>
    /// Base class to derive every <see cref="MonoBehaviour"/> of the project from.
    /// <para/>
    /// The <see cref="EnhancedBehaviour"/> uses component activation callbacks to automatically
    /// register/unregister itself from updates, avoiding using Unity update callbacks for a better result at a lower cost.
    /// <br/> You can register to a variety of callbacks using the <see cref="UpdateRegistration"/> enum flag.
    /// <para/>
    /// Can manage the object initialization from the OnInit method when using the <see cref="UpdateRegistration.Init"/> flag,
    /// which is more efficient and controlled to prevent framedrops when multiple objects initialization occurs.
    /// <para/>
    /// Comes also with basic activation/deactivation callbacks that ensure being called
    /// when its state really changes.
    /// <br/> You can use the OnPaused callback to implement specific behaviours when the object gets paused/unpaused,
    /// which happens when its local time scale factor reach 0.
    /// </summary>
    public abstract class EnhancedBehaviour : MonoBehaviour, IBaseUpdate, IInitUpdate, IPlayUpdate, ISaveable, IMessageLogger {
        #region Update Registration
        /// <summary>
        /// Override this to specify this object update registration.
        /// <para/>
        /// Use "base.UpdateRegistration | <see cref="Core.UpdateRegistration"/>.value"
        /// to add a new registration or override its value
        /// </summary>
        public virtual UpdateRegistration UpdateRegistration => 0;

        /// <summary>
        /// Override this to automatically register / unregister this behaviour as a <see cref="ILoadingProcessor"/>.
        /// </summary>
        public virtual bool IsLoadingProcessor => false;

        /// <summary>
        /// Override this to specify if this object data should be automatically saved.
        /// <para/>
        /// Use <see cref="OnSerialize(Core.SaveData)"/> and <see cref="OnDeserialize(Core.SaveData)"/> to save and load data.
        /// </summary>
        public virtual bool SaveData => false;

        /// <summary>
        /// Object used for console logging, using <see cref="UpdateManager"/> update system.
        /// </summary>
        public Object LogObject {
            get { return this; }
        }

        /// <summary>
        /// Indicates whether this object is initialized or not.
        /// </summary>
        bool IInitUpdate.IsInitialized { get; set; } = false;

        /// <summary>
        /// Indicates whether this object is currently "playing" or still holding its execution.
        /// </summary>
        bool IPlayUpdate.IsPlaying { get; set; } = false;
        #endregion

        #region Global Members
        [PropertyOrder(int.MinValue + 999)]
        [SerializeField, Enhanced, ReadOnly] protected float chronos = 1f;
        [SerializeField, HideInInspector] private EnhancedObjectID objectID = EnhancedObjectID.Default;

        /// <summary>
        /// This object local time scale factor.
        /// <para/>
        /// Affects the values of both <see cref="DeltaTime"/> and <see cref="SmoothDeltaTime"/> properties.
        /// </summary>
        public float Chronos {
            get {
                return chronos;
            } internal set {
                bool _doPause = (chronos == 0f) != (value == 0f);
                chronos = value;

                if (_doPause) {
                    OnPaused(value == 0f);
                }
            }
        }

        /// <summary>
        /// Object-related version of <see cref="Time.deltaTime"/>.
        /// </summary>
        public float DeltaTime {
            get {
                return Time.deltaTime * chronos;
            }
        }

        /// <summary>
        /// A smooth verison of this object-related version of <see cref="Time.deltaTime"/>.
        /// </summary>
        public float SmoothDeltaTime {
            get {
                return Time.smoothDeltaTime * chronos;
            }
        }

        /// <summary>
        /// Object-related version of <see cref="Time.unscaledDeltaTime"/>.
        /// </summary>
        public float UnscaledDeltaTime {
            get {
                return Time.unscaledDeltaTime * chronos;
            }
        }

        /// <summary>
        /// <inheritdoc cref="Component.transform"/>
        /// Uses an overridable property for optimization purpose.
        /// </summary>
        public virtual Transform Transform {
            get { return transform; }
        }

        /// <summary>
        /// The unique identifier of this object.
        /// </summary>
        public EnhancedObjectID ID {
            get { return objectID; }
        }

        /// <summary>
        /// The instance id of this object.
        /// </summary>
        public int InstanceID {
            get { return GetInstanceID(); }
        }
        #endregion

        #region Operator
        public static implicit operator EnhancedObjectID(EnhancedBehaviour _behaviour) {
            return _behaviour.ID;
        }

        public override bool Equals(object _object) {
            if (_object is EnhancedBehaviour _behaviour) {
                return Equals(_behaviour);
            }

            return base.Equals(_object);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
        #endregion

        #region Enhanced Behaviour
        /// <summary>
        /// Base unity message when this behaviour is enabled.
        /// <br/> Should not be overridden, always prefer using <see cref="OnBehaviourEnabled"/> instead.
        /// </summary>
        protected virtual void OnEnable() {
            OnBehaviourEnabled();
        }

        /// <summary>
        /// Base unity message when this behaviour is disabled.
        /// <br/> Should not be overridden, always prefer using <see cref="OnBehaviourDisabled"/> instead.
        /// </summary>
        protected virtual void OnDisable() {
            if (!GameManager.IsQuittingApplication) {
                OnBehaviourDisabled();
            }
        }

        void IInitUpdate.Init() {
            OnInit();
        }

        void IPlayUpdate.Play() {
            OnPlay();
        }

        // -----------------------

        /// <summary>
        /// Called when this behaviour is being enabled.
        /// </summary>
        protected virtual void OnBehaviourEnabled() {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
            #endif

            // Assign ID for new instantiated objects.
            GetObjectID();

            // Registration.
            UpdateRegistration _registraiton = UpdateRegistration;

            if (_registraiton != 0) {
                UpdateManager.Instance.Register(this, _registraiton);
            }

            if (IsLoadingProcessor && (this is ILoadingProcessor _processor)) {
                EnhancedSceneManager.Instance.RegisterProcessor(_processor);
            }

            if (SaveData) {
                SaveManager.Instance.Register(this);
            }
        }

        /// <summary>
        /// Called on object initialization (during loading).
        /// <br/> Requires to set the flag <see cref="UpdateRegistration.Init"/> on this object <see cref="UpdateRegistration"/> property.
        /// <para/>
        /// Initialization is only called once after
        /// <br/> <see cref="OnBehaviourEnabled"/>, but before any local update.
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// Called once after exiting this object scene loading. Use this to start its behaviour execution.
        /// <br/> Requires to set the flag <see cref="UpdateRegistration.Play"/> on this object <see cref="UpdateRegistration"/> property.
        /// <para/>
        /// Play is only called once after <see cref="OnInit"/>
        /// <br/> and exiting loading, but before any local update.
        /// </summary>
        protected virtual void OnPlay() { }

        /// <summary>
        /// Called when this object is being paused or unpaused.
        /// <para/>
        /// Pause happens when this object local <see cref="chronos"/> value is set to 0.
        /// </summary>
        protected virtual void OnPaused(bool _isPaused) { }

        /// <summary>
        /// Called when this behaviour is being disabled.
        /// </summary>
        protected virtual void OnBehaviourDisabled() {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
            #endif

            // Trigger release.
            triggerHandler = triggerHandler.Release();

            // Unregistration.
            UpdateRegistration _registraiton = UpdateRegistration;

            if (_registraiton != 0) {
                UpdateManager.Instance.Unregister(this, _registraiton);
            }

            if (IsLoadingProcessor && (this is ILoadingProcessor _processor)) {
                EnhancedSceneManager.Instance.UnregisterProcessor(_processor);
            }

            if (SaveData) {
                SaveManager.Instance.Unregister(this);
            }
        }

        // -------------------------------------------
        // Editor
        // -------------------------------------------

        [Conditional("UNITY_EDITOR")]
        protected virtual void OnValidate() {
            #if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode) {
                GetObjectID();
            }
            #endif
        }

        /// <summary>
        /// Override this method to draw your own Handles when the object is selected (only in editor).
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        internal protected virtual void OnDrawHandles() { }
        #endregion

        // --- Logic --- \\

        #region Object ID
        /// <summary>
        /// If true, automatically regenerates this object ID on play if required.
        /// </summary>
        public virtual bool AutoRegenerateID {
            get { return true; }
        }

        // -----------------------

        /// <summary>
        /// Get this object unique ID.
        /// </summary>
        [ContextMenu("Get Object ID", false, 10)]
        private void GetObjectID() {

            #if UNITY_EDITOR
            if (!Application.isPlaying) {

                // Prefab objects always have a null scene id.
                if (!gameObject.scene.IsValid() || (StageUtility.GetCurrentStage() is PrefabStage)) {

                    if (objectID.IsValid) {
                        SetID(EnhancedObjectID.Default);
                    }

                    return;
                }

                if (true/*!objectID.IsValid*/) {

                    EnhancedObjectID _objectID = new EnhancedObjectID(this);
                    SetID(_objectID);
                }

                return;
            }

            // ----- Local Method ----- \\

            void SetID(EnhancedObjectID _id) {

                if (objectID == _id) {
                    return;
                }

                Undo.RecordObject(this, "Assigning ID");
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);

                objectID = _id;
                EditorUtility.SetDirty(this);
            }
            #endif

            // Runtime assignement.
            if (AutoRegenerateID) {
                objectID.InitSceneObject(this);
            }
        }
        #endregion

        #region Saveable
        void ISaveable.Serialize(SaveData _data) {
            OnSerialize(_data);
        }

        void ISaveable.Deserialize(SaveData _data) {
            OnDeserialize(_data);
        }

        // -----------------------

        /// <inheritdoc cref="ISaveable.Serialize(SaveData)"/>
        protected virtual void OnSerialize(SaveData _data) { }

        /// <inheritdoc cref="ISaveable.Deserialize(SaveData)"/>
        protected virtual void OnDeserialize(SaveData _data) { }
        #endregion

        #region Events
        /// <summary>
        /// Calls an <see cref="EnhancedAnimationEvent"/> on this behaviour.
        /// <br/> This is an alternative to direct method calls from animation events,
        /// allowing more control and reference management.
        /// <para/>
        /// Note that this method should only be called from an animation event.
        /// </summary>
        /// <param name="_event">Event to call.</param>
        public void AnimationEvent(ScriptableObject _event) {

            if (_event is not IEnhancedAnimationEvent _animationEvent) {
                this.LogWarningMessage($"{_event.name} is not of AnimationEvent type", _event);
                return;
            }

            _animationEvent.Invoke(this);
        }
        #endregion

        #region Trigger
        protected TriggerInfoHandler triggerHandler = default;

        /// <inheritdoc cref="TriggerInfoHandler.ActorCount"/>
        public int TriggerActorCount {
            get { return triggerHandler.ActorCount; }
        }

        // -----------------------

        /// <inheritdoc cref="ITrigger.OnEnterTrigger(ITriggerActor)"/>
        public void OnEnterTrigger(ITriggerActor _actor) {

            // Security.
            if (this is not ITrigger _trigger) {
                this.LogWarningMessage("Object is no Trigger");
                return;
            }

            triggerHandler = triggerHandler.RegisterActor(_trigger, _actor);
            EnhancedBehaviour _behaviour = _actor.Behaviour;

            if (InteractWithTrigger(_behaviour)) {
                OnEnterTrigger(_actor, _behaviour);
            }
        }

        /// <inheritdoc cref="ITrigger.OnExitTrigger(ITriggerActor)"/>
        public void OnExitTrigger(ITriggerActor _actor) {

            triggerHandler = triggerHandler.UnregisterActor(_actor);
            EnhancedBehaviour _behaviour = _actor.Behaviour;

            if (InteractWithTrigger(_behaviour)) {
                OnExitTrigger(_actor, _behaviour);
            }
        }

        // -------------------------------------------
        // Callback
        // -------------------------------------------

        /// <summary>
        /// <inheritdoc cref="ITrigger.OnEnterTrigger(ITriggerActor)"/>
        /// <para/>
        /// Requires this object to inherit from <see cref="ITrigger"/>.
        /// </summary>
        /// <param name="_behaviour"><see cref="EnhancedBehaviour"/> entering this trigger.</param>
        /// <inheritdoc cref="ITrigger.OnEnterTrigger(ITriggerActor)"/>
        protected virtual void OnEnterTrigger(ITriggerActor _actor, EnhancedBehaviour _behaviour) { }

        /// <summary>
        /// <inheritdoc cref="ITrigger.OnExitTrigger(ITriggerActor)"/>
        /// <para/>
        /// Requires this object to inherit from <see cref="ITrigger"/>.
        /// <param name="_behaviour"><see cref="EnhancedBehaviour"/> exiting this trigger.</param>
        /// <inheritdoc cref="ITrigger.OnExitTrigger(ITriggerActor)"/>
        protected virtual void OnExitTrigger(ITriggerActor _actor, EnhancedBehaviour _behaviour) { }

        // -----------------------

        /// <summary>
        /// Called when an actor tries to interact with this trigger.
        /// </summary>
        /// <param name="_actor"><see cref="EnhancedBehaviour"/> interacting with trigger.</param>
        /// <returns>True if this <see cref="EnhancedBehaviour"/> should interact with this trigger, false otherwise.</returns>
        protected virtual bool InteractWithTrigger(EnhancedBehaviour _actor) {
            return true;
        }
        #endregion

        // --- Miscs --- \\

        #region Utility
        /// <summary>
        /// Compare two <see cref="EnhancedBehaviour"/> instances.
        /// </summary>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(EnhancedBehaviour _other) {
            return _other.IsValid() && (ID.Equals(_other.ID));
        }

        // -------------------------------------------
        // Vector
        // -------------------------------------------

        /// <inheritdoc cref="TransformExtensions.RelativeVector(Transform, Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetRelativeVector(Vector3 _vector) {
            return Transform.RelativeVector(_vector);
        }

        /// <inheritdoc cref="TransformExtensions.WorldVector(Transform, Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetWorldVector(Vector3 _vector) {
            return Transform.WorldVector(_vector);
        }

        // -----------------------

        /// <summary>
        /// <inheritdoc cref="GetRelativeVector(Vector3)"/>
        /// <para/>
        /// This method overload can be used when this transform rotation is already cached in a value.
        /// <br/> This only differs from <see cref="VectorExtensions.RotateInverse(Vector3, Quaternion)"/>
        /// in its signature, to maintain a coherent architecture.
        /// </summary>
        /// <param name="_rotation">Rotation of the object.</param>
        /// <inheritdoc cref="GetRelativeVector(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetRelativeVector(Vector3 _vector, Quaternion _rotation) {
            return _vector.RotateInverse(_rotation);
        }

        /// <summary>
        /// <inheritdoc cref="GetWorldVector(Vector3)"/>
        /// <para/>
        /// This method overload can be used when this transform rotation is already cached in a value.
        /// <br/> This only differs from <see cref="VectorExtensions.Rotate(Vector3, Quaternion)"/>
        /// in its signature, to maintain a coherent architecture.
        /// </summary>
        /// <param name="_rotation">Rotation of the object.</param>
        /// <inheritdoc cref="GetWorldVector(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetWorldVector(Vector3 _vector, Quaternion _rotation) {
            return _vector.Rotate(_rotation);
        }
        #endregion

        #region Logger
        /// <inheritdoc/>
        public virtual string GetLogMessageFormat(LogType _type) {
            return EnhancedLogger.GetMessageFormat(GetType(), GetLogMessageColor(_type));
        }

        /// <summary>
        /// Get the <see cref="Color"/> used to identify a log message of a specific type..
        /// </summary>
        /// <param name="_type">The type of the log message to get the associated color.</param>
        /// <returns>The color to use for the given <see cref="LogType"/> message.</returns>
        public virtual Color GetLogMessageColor(LogType _type) {
            return SuperColor.White.Get();
        }
        #endregion

        #region Play Mode Data
        /// <summary>
        /// Indicates if this object data can be saved during Play Mode.
        /// </summary>
        public virtual bool CanSavePlayModeData {
            get { return false; }
        }

        // -----------------------

        /// <summary>
        /// Saves this object data.
        /// </summary>
        /// <param name="_data">Wrapper in which to save this object data.</param>
        [Conditional("UNITY_EDITOR")]
        public virtual void SavePlayModeData(PlayModeEnhancedObjectData _data) { }

        /// <summary>
        /// Loads this object data.
        /// </summary>
        /// <param name="_data">Wrapper from which to load this object data.</param>
        [Conditional("UNITY_EDITOR")]
        public virtual void LoadPlayModeData(PlayModeEnhancedObjectData _data) { }
        #endregion
    }
}
