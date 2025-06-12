// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using UnityEngine;

using Range = EnhancedEditor.RangeAttribute;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedFramework.Core {
    /// <summary>
    /// <see cref="WeightAreaController"/>-related area settings wrapper.
    /// </summary>
    [Serializable]
    public sealed class WeightAreaSettings {
        #region Global Members
        [Space(10f)]

        [Tooltip("Axes of this area perimeter. Use all for a full 3D support, or only two for 2D")]
        public AxisConstraints Axes = AxisConstraints.X | AxisConstraints.Z;

        [Space(10f)]

        [Tooltip("X: The distance from the centerpoint that the area will have full effect at\n" +
                 "Y: The distance from the centerpoint that the area will not have any effect")]
        [Enhanced, MinMax(nameof(MinMaxRange))] public Vector2 MinMaxDistance = new Vector2(2f, 5f);

        [Tooltip("Area range max slider value")]
        [SerializeField, Enhanced, Range(5f, 99f)] private float maxRange = 10f;

        [Space(10f)]

        [Tooltip("If true, resets the weight back to 0 outside the area. Otherwise, uses the minimum range weight value")]
        public bool ResetWeightOutsideArea = true;

        [Tooltip("X: The weight of this area at its maximum distance\n" +
                 "Y: The weight of this area at its minimum distance")]
        [Enhanced, MinMax(0f, 1f)] public Vector2 RangeWeight = new Vector2(0f, 1f);

        // -----------------------

        /// <summary>
        /// This area range min max slider range.
        /// </summary>
        private Vector2 MinMaxRange {
            get { return new Vector2(0f, maxRange); }
        }
        #endregion
    }

    /// <summary>
    /// Base <see cref="EnhancedBehaviour"/> area weight controller, using an actor-related distance range.
    /// </summary>
    public abstract class WeightAreaController : EnhancedActivatorBehaviour, IDynamicUpdate {
        #region Global Members
        [PropertyOrder(1), Space(10f)]

        [Tooltip("If true, uses a range area to control this object weight")]
        [SerializeField] protected bool useRangeArea = false;
        [SerializeField, Enhanced, ShowIf(nameof(useRangeArea))] protected WeightAreaSettings areaSettings = new WeightAreaSettings();

        // -----------------------

        [PropertyOrder(9)]
        [Tooltip("Current actor instance within this trigger")]
        [SerializeField, Enhanced, ReadOnly] protected Transform actor = null;

        [Space(5f)]

        [Tooltip("Current weight of this object")]
        [SerializeField, Enhanced, ReadOnly, Range(0f, 1f)] protected float weight = 0f;

        // -----------------------

        [NonSerialized] protected bool isActor = false;

        // -----------------------

        /// <summary>
        /// Weight of this object.
        /// </summary>
        public float Weight {
            get { return weight; }
        }
        #endregion

        #region Enhanced Behaviour
        /// <summary>
        /// Color used to draw handles for this area center, small range.
        /// </summary>
        protected virtual SuperColor CenterRangeColor {
            get { return SuperColor.Green; }
        }

        /// <summary>
        /// Color used to draw handles for this area large range.
        /// </summary>
        protected virtual SuperColor LargeRangeColor {
            get { return SuperColor.Sapphire; }
        }

        // -----------------------

        protected override void OnBehaviourEnabled() {
            // Cooldown initialization.
            updateCooldown.Duration = UpdateCooldownDuration;

            base.OnBehaviourEnabled();
        }

        void IDynamicUpdate.Update() {
            ControllerUpdate();
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        private const float SphereHandlesAlphaCoef  = 1.5f;
        private const float InactiveHandlesAlpha    = .2f;
        private const float ActiveHandlesAlpha      = .5f;

        // -----------------------

        protected internal override void OnDrawHandles() {
            base.OnDrawHandles();

            if (!useRangeArea) {
                return;
            }

            Transform _transform = transform;
            Vector3 _position = _transform.position;

            AxisConstraints _axes = areaSettings.Axes;
            Vector3 _normal =  !_axes.HasFlagUnsafe(AxisConstraints.Y) ? _transform.up
                            : (!_axes.HasFlagUnsafe(AxisConstraints.X) ? _transform.right : _transform.forward);

            Vector2 _range = areaSettings.MinMaxDistance;
            bool _isActor = GetActorDistance(out float _actorDistance);
            bool _sphere = _axes == AxisConstraints.All;

            // Large area.
            using (var _scope = EnhancedGUI.HandlesColor.Scope(LargeRangeColor.Get(GetAlpha(_range.x, _range.y)))) {

                if (_sphere) {
                    Handles.SphereHandleCap(0, _position, Quaternion.identity, _range.y * 2f, EventType.Repaint);
                } else {
                    Handles.DrawSolidDisc(_position, _normal, _range.y);
                }
            }

            // Center area.
            using (var _scope = EnhancedGUI.HandlesColor.Scope(CenterRangeColor.Get(GetAlpha(0f, _range.x)))) {

                if (_sphere) {
                    Handles.SphereHandleCap(0, _position, Quaternion.identity, _range.x * 2f, EventType.Repaint);
                } else {
                    Handles.DrawSolidDisc(_position, _normal, _range.x);
                }
            }

            // Disc outlines.
            using (var _scope = EnhancedGUI.HandlesColor.Scope(SuperColor.Black.Get(ActiveHandlesAlpha))) {

                if (!_sphere) {
                    Handles.DrawWireDisc(_position, _normal, _range.x);
                    Handles.DrawWireDisc(_position, _normal, _range.y);
                }
            }

            // ----- Local Method ----- \\

            float GetAlpha(float _min, float _max) {

                float _alpha = (_isActor && Mathm.IsInRange(_actorDistance, _min, _max))
                             ? ActiveHandlesAlpha
                             : InactiveHandlesAlpha;

                if (_sphere) {
                    _alpha *= SphereHandlesAlphaCoef;
                }

                return _alpha;
            }
        }
#endif
        #endregion

        #region Behaviour
        private const UpdateRegistration UpdateType = UpdateRegistration.Dynamic;
        private const float UpdateCooldwonDefaultDuration = .2f;

        protected readonly ManualCooldown updateCooldown = new ManualCooldown(UpdateCooldwonDefaultDuration);

        /// <summary>
        /// Duration of this controller update cooldown.
        /// <br/> Used to only update this controller every X seconds.
        /// </summary>
        protected virtual float UpdateCooldownDuration {
            get { return UpdateCooldwonDefaultDuration; }
        }

        // -------------------------------------------
        // Controller
        // -------------------------------------------

        /// <summary>
        /// Called on update while this controller is active.
        /// </summary>
        protected virtual void ControllerUpdate() {

            // Update cooldown.
            float _delta = unscaledTime ? UnscaledDeltaTime : DeltaTime;

            if (!updateCooldown.Update(_delta)) {
                return;
            }

            updateCooldown.Reload();

            // Non-area behaviour.
            if (!useRangeArea) {

                UpdateWeight(1f);
                return;
            }

            Vector2 _weightRange = areaSettings.RangeWeight;
            float _weight;

            // Outside area.
            if (!GetActorDistance(out float _actorDistance)) {

                _weight = areaSettings.ResetWeightOutsideArea ? 0f : _weightRange.x;
                UpdateWeight(_weight);

                return;
            }

            // Area weight.
            Vector2 _range = areaSettings.MinMaxDistance;
            float _normalizedDistance = Mathm.NormalizedValue(_actorDistance, _range.y, _range.x);

            _weight = Mathf.Lerp(_weightRange.x, _weightRange.y, _normalizedDistance);
            UpdateWeight(_weight);

            // ----- Local Method ----- \\

            void UpdateWeight(float _weight) {

                if (Mathf.Approximately(_weight, weight)) {
                    return;
                }

                SetWeight(_weight);
            }
        }

        /// <summary>
        /// Sets the weight of this object.
        /// </summary>
        /// <param name="_weight">Weight of this object.</param>
        protected virtual void SetWeight(float _weight) {
            weight = _weight;
        }

        // -------------------------------------------
        // Activation
        // -------------------------------------------

        protected override void OnActivation() {
            base.OnActivation();

            UpdateManager.Instance.Register(this, UpdateType);
            SetWeight(0f);
        }

        protected override void OnDeactivation() {
            base.OnDeactivation();

            UpdateManager.Instance.Unregister(this, UpdateType);
        }
        #endregion

        #region Trigger
        protected override void OnEnterTrigger(ITriggerActor _actor, EnhancedBehaviour _behaviour) {

            // Actor.
            if (isActor) {
                this.LogWarningMessage($"Actor '{actor.name}' already registered in this area");
                return;
            }

            SetActor(true, _behaviour.transform);
            base.OnEnterTrigger(_actor, _behaviour);
        }

        protected override void OnExitTrigger(ITriggerActor _actor, EnhancedBehaviour _behaviour) {

            // Actor.
            if (!isActor || (_behaviour.transform != actor)) {
                return;
            }

            SetActor(false, null);
            base.OnExitTrigger(_actor, _behaviour);
        }
        #endregion

        #region Area
        /// <summary>
        /// Get the distance between this trigger area current actor and the area center.
        /// </summary>
        /// <param name="_distance">Distance between the current actor and this area center (0 if there is no actor).</param>
        /// <returns>True if an actor is currently in the area, false otherwise.</returns>
        public bool GetActorDistance(out float _distance) {

            if (!isActor) {
                _distance = 0f;
                return false;
            }

            Vector3 _actorPosition = actor.position;
            Vector3 _position      = transform.position;

            AxisConstraints _axes  = areaSettings.Axes;

            if (!_axes.HasFlagUnsafe(AxisConstraints.X)) {
                _actorPosition = _actorPosition.SetX(_position.x);
            }

            if (!_axes.HasFlagUnsafe(AxisConstraints.Y)) {
                _actorPosition = _actorPosition.SetY(_position.y);
            }

            if (!_axes.HasFlagUnsafe(AxisConstraints.Z)) {
                _actorPosition = _actorPosition.SetZ(_position.z);
            }

            _distance = Vector3.Distance(_actorPosition, _position);
            return true;
        }

        /// <summary>
        /// Sets this trigger area actor.
        /// </summary>
        /// <param name="_isActor">Whether there is an actor in this area or not.</param>
        /// <param name="_actor">This area actor.</param>
        protected virtual void SetActor(bool _isActor, Transform _actor) {
            isActor = _isActor;
            actor   = _actor;
        }
        #endregion
    }
}
