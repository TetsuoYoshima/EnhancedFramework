// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedFramework.Core {
    // ===== Base ===== \\
    
    /// <summary>
    /// Utility class for an easy-to use realtime cooldown.
    /// </summary>
    [Serializable]
    public class Cooldown {
        #region Global Members
        /// <summary>
        /// Duration of the cooldown, that is the time (in seconds) it takes to be fully reloaded.
        /// </summary>
        public float Duration = 0f;
        protected float time  = 0f;

        /// <summary>
        /// Is this cooldown valid again?
        /// </summary>
        public bool IsValid {
            get { return CurrentTime > Duration; }
        }

        /// <summary>
        /// Remaining time of this cooldown.
        /// </summary>
        public float Remain {
            get { return Mathf.Max(0f, Duration - CurrentTime); }
        }

        protected virtual float CurrentTime {
            get { return Time.time - time; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="Cooldown(float)"/>
        public Cooldown() { }

        /// <param name="_duration"><inheritdoc cref="Duration" path="/summary"/></param>
        /// <inheritdoc cref="Cooldown"/>
        public Cooldown(float _duration) {
            Duration = _duration;
        }
        #endregion

        #region Operator
        public static implicit operator bool(Cooldown _cooldown) {
            return _cooldown.IsValid;
        }
        #endregion

        #region Behaviour

        /// <param name="_duration"><inheritdoc cref="Duration" path="/summary"/></param>
        /// <inheritdoc cref="Reload"/>
        public void Reload(float _duration) {
            Duration = _duration;
            Reload();
        }

        /// <summary>
        /// Reloads this cooldown using its current duration.
        /// </summary>
        public virtual void Reload() {
            time = Time.time;
        }

        /// <summary>
        /// Resets this cooldown.
        /// </summary>
        public virtual void Reset() {
            time = -Duration;
        }
        #endregion
    }

    // ===== Derived ===== \\

    /// <summary>
    /// Utility class for an easy-to use unscaled-time cooldown.
    /// </summary>
    [Serializable]
    public sealed class UnscaledCooldown : Cooldown {
        #region Global Members
        protected override float CurrentTime {
            get { return Time.unscaledTime - time; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="UnscaledCooldown(float)"/>
        public UnscaledCooldown() { }

        /// <summary>
        /// <inheritdoc cref="UnscaledCooldown"/>
        /// </summary>
        /// <inheritdoc cref="Cooldown(float)"/>
        public UnscaledCooldown(float _duration) : base(_duration) { }
        #endregion

        #region Behaviour
        public override void Reload() {
            time = Time.unscaledTime;
        }
        #endregion
    }

    /// <summary>
    /// Utility class for an easy-to use manual update cooldown.
    /// </summary>
    [Serializable]
    public sealed class ManualCooldown : Cooldown {
        #region Global Members
        protected override float CurrentTime {
            get { return time; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="ManualCooldown(float)"/>
        public ManualCooldown() { }

        /// <summary>
        /// <inheritdoc cref="ManualCooldown"/>
        /// </summary>
        /// <inheritdoc cref="Cooldown(float)"/>
        public ManualCooldown(float _duration) : base(_duration) { }
        #endregion

        #region Behaviour
        /// <summary>
        /// Updates this cooldown value.
        /// </summary>
        /// <param name="_deltaTime">Time increase.</param>
        /// <returns>True if this cooldown is now valid, false otherwise.</returns>
        public bool Update(float _deltaTime) {
            time += _deltaTime;
            return IsValid;
        }

        public override void Reload() {
            time = 0f;
        }

        public override void Reset() {
            time = Duration + 1f;
        }
        #endregion
    }

    /// <summary>
    /// Utility class for an easy-to use frame cooldown.
    /// </summary>
    [Serializable]
    public sealed class FrameCooldown : Cooldown {
        #region Global Members
        protected override float CurrentTime {
            get { return Time.frameCount - time; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="FrameCooldown(float)"/>
        public FrameCooldown() { }

        /// <summary>
        /// <inheritdoc cref="FrameCooldown"/>
        /// </summary>
        /// <inheritdoc cref="FrameCooldown(float)"/>
        public FrameCooldown(float _duration) : base(_duration) { }
        #endregion

        #region Behaviour
        public override void Reload() {
            time = Time.frameCount;
        }
        #endregion
    }

    /// <summary>
    /// Utility class for an easy-to use cooldown with a callback.
    /// </summary>
    [Serializable]
    public sealed class CallbackCooldown<T> : ILateUpdate where T : Cooldown {
        #region Global Members
        /// <summary>
        /// <see cref="Core.Cooldown"/> wrapped in this object.
        /// </summary>
        public T Cooldown = null;

        /// <summary>
        /// Called once when this cooldown has elapsed.
        /// </summary>
        public Action OnComplete = null;

        // -----------------------

        /// <inheritdoc cref="Cooldown.IsValid"/>
        public bool IsValid {
            get { return Cooldown.IsValid; }
        }

        public float Remain {
            get { return Cooldown.Remain; }
        }

        Object IBaseUpdate.LogObject {
            get { return UpdateManager.Instance; }
        }

        public int InstanceID { get; private set; } = EnhancedUtility.GenerateGUID();

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="CallbackCooldown{T}"/>
        public CallbackCooldown() {
            Cooldown = Activator.CreateInstance<T>();
        }

        /// <param name="duration"><inheritdoc cref="Cooldown.Duration" path="/summary"/></param>
        /// <inheritdoc cref="CallbackCooldown()"/>
        public CallbackCooldown(float duration) : this() {
            Cooldown.Duration = duration;
        }
        #endregion

        #region Behaviour
        /// <inheritdoc cref="Cooldown.Reload"/>
        public void Reload(Action _onComplete = null) {
            Cooldown.Reload();
            Reload_Internal(_onComplete);
        }

        /// <inheritdoc cref="Cooldown.Reload(float)"/>
        public void Reload(float _duration, Action _onComplete = null) {
            Cooldown.Reload(_duration);
            Reload_Internal(_onComplete);
        }

        /// <inheritdoc cref="Cooldown.Reset"/>
        public void Cancel() {
            Cooldown.Reset();
            UnregisterUpdate();
        }

        // -----------------------

        private void Reload_Internal(Action _onComplete) {
            OnComplete = _onComplete;
            RegisterUpdate();
        }
        #endregion

        #region Update
        public const UpdateRegistration Registration = UpdateRegistration.Late;
        private bool isRegistered = false;

        // -----------------------

        void ILateUpdate.Update() {
            if (!Cooldown.IsValid)
                return;

            UnregisterUpdate();

            Action _onComplete = OnComplete;

            OnComplete = null;
            _onComplete?.Invoke();
        }

        private void RegisterUpdate() {
            if (isRegistered)
                return;

            UpdateManager.Instance.Register(this, Registration);
            isRegistered = true;
        }

        private void UnregisterUpdate() {
            if (!isRegistered)
                return;

            UpdateManager.Instance.Unregister(this, Registration);
            isRegistered = false;
        }
        #endregion
    }
}
