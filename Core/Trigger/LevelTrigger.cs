// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedFramework.Core {
    /// <summary>
    /// Enum used to select one or multiple portal identifier(s).
    /// <br/> Uses cardinal directions.
    /// <para/>
    /// North - South - East - West
    /// </summary>
    [Flags]
    public enum PortalIdentifier {
        [Separator(SeparatorPosition.Bottom)]
        None        = 0,

        North       = 1 << 0,
        South       = 1 << 1,
        East        = 1 << 2,
        West        = 1 << 3,

        NorthEast   = 1 << 4,
        NorthWest   = 1 << 5,
        SouthEast   = 1 << 6,
        SouthWest   = 1 << 7,
    }

    /// <summary>
    /// Base class to inherit your own triggers from.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Trigger/Level Trigger"), DisallowMultipleComponent]
    public class LevelTrigger : EnhancedTrigger {
        /// <summary>
        /// Wrapper for a <see cref="LevelTrigger"/> entrance / exit portal.
        /// </summary>
        [Serializable]
        public sealed class Portal {
            #region Global Members
            [Tooltip("Identifier of this portal")]
            [SerializeField, Enhanced, FlagField(false), Duo(nameof(inverse), 95f)] private PortalIdentifier identifier = PortalIdentifier.None;

            [Tooltip("If true, inverses this portal forward direction")]
            [SerializeField, HideInInspector, Enhanced, Duo(nameof(color), 75f)] private bool inverse = false;

            [Tooltip("Previsualisation color of this portal")]
            [SerializeField, HideInInspector] internal Color color = Color.red;

            [Tooltip("Angle range of this portal")]
            [SerializeField, Enhanced, MinMax(-180f, 180f)] private Vector2Int angle = new Vector2Int(-15, 15);

            // -----------------------

            /// <summary>
            /// Identifier of this portal.
            /// </summary>
            public PortalIdentifier Identifier {
                get { return identifier; }
            }

            /// <summary>
            /// Angle detection range of this portal.
            /// </summary>
            public Vector2Int DirectionAngle {
                get {
                    Vector2Int _angle = angle;

                    if (inverse) {
                        _angle.x -= 180;
                        _angle.y -= 180;
                    }

                    return _angle;
                }
            }
            #endregion

            #region Utility
            /// <summary>
            /// Get if a specific angle is in the range of this portal.
            /// </summary>
            /// <param name="_angle">Angle to check (relative to this trigger forward direction).</param>
            /// <returns>True if the given angle is in this portal range, false otherwise.</returns>
            public bool IsInRange(float _angle) {
                if (inverse && (_angle > 0f)) {
                    _angle -= 360f;
                }

                return DirectionAngle.Contains(_angle);
            }
            #endregion
        }

        /// <summary>
        /// Wrapper for a <see cref="LevelTrigger"/> <see cref="ITrigger"/> callback.
        /// </summary>
        [Serializable]
        public sealed class Callback {
            #region Global Members
            [Tooltip("Callback object connected with this trigger")]
            [SerializeField] private SerializedInterface<ITrigger> trigger = null;

            [Tooltip("Identifier of all interacting portal(s) when entering this trigger")]
            [SerializeField] private PortalIdentifier enter = PortalIdentifier.None;

            [Tooltip("Identifier of all interacting portal(s) when exiting this trigger")]
            [SerializeField] private PortalIdentifier exit = PortalIdentifier.None;

            /// <summary>
            /// <see cref="ITrigger"/> callback object.
            /// </summary>
            public ITrigger Trigger {
                get { return trigger.Interface; }
            }

            /// <summary>
            /// Identifier of all interacting portal(s) when entering this trigger.
            /// </summary>
            public PortalIdentifier Enter {
                get { return enter; }
            }

            /// <summary>
            /// Identifier of all interacting portal(s) when exiting this trigger.
            /// </summary>
            public PortalIdentifier Exit {
                get { return exit; }
            }

            // -------------------------------------------
            // Constructor(s)
            // -------------------------------------------

            /// <inheritdoc cref="Callback(ITrigger, PortalIdentifier, PortalIdentifier)"/>
            public Callback() { }

            /// <param name="_trigger"><inheritdoc cref="Trigger" path="/summary"/></param>
            /// <param name="_enter"><inheritdoc cref="Enter" path="/summary"/></param>
            /// <param name="_exit"><inheritdoc cref="Exit" path="/summary"/></param>
            /// <inheritdoc cref="Callback"/>
            public Callback(ITrigger _trigger, PortalIdentifier _enter, PortalIdentifier _exit) {
                trigger = new SerializedInterface<ITrigger>(_trigger);
                enter   = _enter;
                exit    = _exit;
            }
            #endregion

            #region Trigger
            /// <param name="_portal">Identifier of the associated <see cref="LevelTrigger"/> entered portal.</param>
            /// <inheritdoc cref="ITrigger.OnEnterTrigger(ITriggerActor)"/>
            public void OnEnterTrigger(ITriggerActor _actor, PortalIdentifier _portal) {

                // Trigger activation.
                if (!TestIdentifier(enter, _portal)) {
                    return;
                }

                trigger.Interface.OnEnterTrigger(_actor);
            }

            /// <param name="_portal">Identifier of the associated <see cref="LevelTrigger"/> exit portal.</param>
            /// <inheritdoc cref="ITrigger.OnEnterTrigger(ITriggerActor)"/>
            public void OnExitTrigger(ITriggerActor _actor, PortalIdentifier _portal) {

                // Trigger activation.
                if (!TestIdentifier(exit, _portal)) {
                    return;
                }

                trigger.Interface.OnExitTrigger(_actor);
            }

            // -----------------------

            private bool TestIdentifier(PortalIdentifier _trigger, PortalIdentifier _portal) {
                return (_trigger == PortalIdentifier.None) || ((_portal != PortalIdentifier.None) && _trigger.HasFlagUnsafe(_portal));
            }
            #endregion
        }

        #region Global Members
        [Section("Level Trigger"), PropertyOrder(0)]

        [Tooltip("Required tags for other objects to interact with this trigger")]
        [SerializeField] protected TagGroup requiredTags = new TagGroup();

        [Tooltip("Required flags for this trigger to be interactable")]
        [SerializeField] protected FlagValueGroup requiredFlags = new FlagValueGroup();

        [Space(5f)]

        [Tooltip("If true, automatically disables this trigger collider after trigger exit")]
        [SerializeField] protected bool onlyOnce = false;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("All registered callbacks for this trigger")]
        [SerializeField] protected BlockList<Callback> callbacks = new BlockList<Callback>();

        [Space(10f)]

        [Tooltip("Entrance / exit portals used to detect how actor should interact with this trigger")]
        [SerializeField] protected BlockArray<Portal> portals = new BlockArray<Portal>();

        [Space(10f), HorizontalLine(SuperColor.Green, 1f), Space(10f)]

        [Tooltip("This object reference trigger collider")]
        [SerializeField, Enhanced, Required] protected new Collider collider = null;
        #endregion

        #region Enhanced Behaviour
        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        private const float InactiveHandlesAlpha    = .5f;
        private const float ActiveHandlesAlpha      = .7f;
        private const float ExitHandleDuration      = 5f;
        private const int RaycastInterval           = 2;

        private static readonly RaycastHit[] hitBuffer = new RaycastHit[16];
        private EnhancedBehaviour actor = null;

        private float exitActorTime = 0f;
        private float exitAngle     = 0f;

        // -----------------------

        protected override void OnValidate() {
            base.OnValidate();

            if (Application.isPlaying) {
                return;
            }

            // References.
            if (collider == null) {
                collider = GetComponent<Collider>();
            }
        }

        protected internal override void OnDrawHandles() {
            base.OnDrawHandles();

            if ((portals.Count == 0) || !collider) {
                return;
            }

            // Area and cast.
            Vector3 _center = collider.bounds.center;
            List<Vector3> _area = new List<Vector3>();

            const float noActorAngle = 720f;
            const float noExitTime = 2f;
            const float fadeExitCoef = 2f;

            // Actor portal.
            float _exitTime = (exitActorTime != 0f) ? ((Time.time - exitActorTime) / ExitHandleDuration) : noExitTime;
            float _actorAngle = (actor != null) ? GetActorAngle(actor) : noActorAngle;

            Portal _actorPortal = Array.Find(portals.Array, p => p.IsInRange(_actorAngle));
            bool _drawActorPortal = !Application.isPlaying;

            // Inverse loop to draw first portals on top.
            for (int i = portals.Count; i-- > 0;) {

                Portal _portal = portals[i];
                float _alpha = InactiveHandlesAlpha;

                // Actor portal.
                if (!_drawActorPortal) {

                    if (_portal == _actorPortal) {

                        // Highlight current actor portal.
                        _alpha = ActiveHandlesAlpha;
                        _drawActorPortal = true;

                    } else if ((_actorPortal == null) && (_exitTime < 1f) && _portal.IsInRange(exitAngle)) {

                        // Highlight last actor exit portal.
                        float _percent = (1f - _exitTime) * fadeExitCoef; // Fade after half time elapsed.
                        _alpha = Mathf.Lerp(_alpha, ActiveHandlesAlpha, _percent);

                        _drawActorPortal = true;
                    }
                }

                // Portal preview.
                using (var _scope = EnhancedGUI.HandlesColor.Scope(_portal.color.SetAlpha(_alpha))) {

                    int _maxAngle = _portal.DirectionAngle.y;
                    float _angle = _portal.DirectionAngle.x;

                    _area.Clear();
                    _area.Add(_center);

                    while (true) {

                        Vector3 _position = GetPoint(_angle);
                        if (_position != _center) {
                            _area.Add(_position);
                        }

                        if ((int)_angle == _maxAngle) {
                            break;
                        }

                        _angle = Mathf.MoveTowards(_angle, _maxAngle, RaycastInterval);
                    }

                    _area.Add(_center);
                    Vector3[] _vertices = _area.ToArray();

                    Handles.DrawAAConvexPolygon(_vertices);

                    using (var _wireframeScope = EnhancedGUI.HandlesColor.Scope(SuperColor.Black.Get())) {
                        Handles.DrawAAPolyLine(_vertices);
                    }

                    // ----- Local Method ----- \\

                    Vector3 GetPoint(float _angle) {

                        Vector3 _direction = Quaternion.AngleAxis(_angle, collider.transform.up) * collider.transform.forward;
                        float _length = collider.bounds.max.SetY(0f).sqrMagnitude;

                        int _count = Physics.RaycastNonAlloc(_center + _direction * _length, -_direction, hitBuffer, _length);
                        for (int i = 0; i < _count; i++) {

                            if (hitBuffer[i].collider == collider) {
                                return hitBuffer[i].point;
                            }
                        }

                        return _center;
                    }
                }
            }
        }
        #endif
        #endregion

        #region Registration
        /// <summary>
        /// Registers a specific <see cref="ITrigger"/> callback on this trigger.
        /// </summary>
        /// <param name="_callback"><see cref="ITrigger"/> instance to register.</param>
        /// <inheritdoc cref="Callback(ITrigger, PortalIdentifier, PortalIdentifier)"/>
        public void RegisterCallback(ITrigger _callback, PortalIdentifier _enter = PortalIdentifier.None, PortalIdentifier _exit = PortalIdentifier.None) {
            callbacks.Add(new Callback(_callback, _enter, _exit));
        }

        /// <summary>
        /// Unregisters a specific <see cref="ITrigger"/> callback from this trigger.
        /// </summary>
        /// <param name="_callback"><see cref="ITrigger"/> instance to unregister.</param>
        public void UnregisterCallback(ITrigger _callback) {

            ref List<Callback> _span = ref callbacks.List;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {

                if (_span[i].Trigger == _callback) {
                    _span.RemoveAt(i);
                    return;
                }
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get trigger callbacks from this object and all its parents.
        /// </summary>
        [ContextMenu("Get Callbacks", false, 100)]
        protected void GetObjectCallbacks() {

            ITrigger[] _triggers = GetComponentsInParent<ITrigger>();
            for (int i = 0; i < _triggers.Length; i++) {

                ITrigger _trigger = _triggers[i];
                if (!EqualityUtility.Equals(_trigger, this)) {
                    RegisterCallback(_trigger);
                }
            }
        }

        /// <summary>
        /// Removes trigger callbacks from this object and all its parents.
        /// </summary>
        [ContextMenu("Clear Callbacks", false, 100)]
        protected void ClearObjectCallbacks() {

            ITrigger[] _triggers = GetComponentsInParent<ITrigger>();
            for (int i = 0; i < _triggers.Length; i++) {

                ITrigger _trigger = _triggers[i];
                if (!EqualityUtility.Equals(_trigger, this)) {
                    UnregisterCallback(_trigger);
                }
            }
        }
        #endregion

        #region Trigger
        protected override void OnEnterTrigger(ITriggerActor _actor, EnhancedBehaviour _behaviour) {
            base.OnEnterTrigger(_actor, _behaviour);

            // Flag requirement.
            if (!requiredFlags.Valid) {
                return;
            }

            // Callbacks.
            PortalIdentifier _portal = GetInteractionPortal(_behaviour);

            ref List<Callback> _span = ref callbacks.List;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {
                _span[i].OnEnterTrigger(_actor, _portal);
            }

            #if UNITY_EDITOR
            // Editor utility.
            actor = _behaviour;
            #endif
        }

        protected override void OnExitTrigger(ITriggerActor _actor, EnhancedBehaviour _behaviour) {
            base.OnExitTrigger(_actor, _behaviour);

            // Callbacks.
            PortalIdentifier _portal = GetInteractionPortal(_behaviour);

            ref List < Callback > _span = ref callbacks.List;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {
                _span[i].OnExitTrigger(_actor, _portal);
            }

            #if UNITY_EDITOR
            // Editor utility.
            if (actor == _behaviour) {

                exitActorTime = Time.time;
                exitAngle = GetActorAngle(_behaviour);
                actor     = null;
            }
            #endif

            // Disable.
            if (onlyOnce) {
                collider.enabled = false;
                enabled = false;
            }
        }

        // -------------------------------------------
        // Interaction
        // -------------------------------------------

        protected override bool InteractWithTrigger(EnhancedBehaviour _behaviour) {
            return base.InteractWithTrigger(_behaviour) && _behaviour.HasTags(requiredTags);
        }

        /// <summary>
        /// Get this trigger interaction portal for this actor to interact with.
        /// </summary>
        /// <param name="_behaviour">Actor interacting with this trigger.</param>
        /// <returns><see cref="PortalIdentifier"/> identifier of the actor associated interaction portal.</returns>
        private PortalIdentifier GetInteractionPortal(EnhancedBehaviour _behaviour) {

            PortalIdentifier _direction = PortalIdentifier.None;

            // Interacting portal.
            ref Portal[] _span = ref portals.Array;
            int _count = _span.Length;

            if (_count != 0) {

                float _angle = GetActorAngle(_behaviour);
                for (int i = 0; i < _count; i++) {

                    Portal _portal = _span[i];
                    if (_portal.IsInRange(_angle)) {

                        _direction = _portal.Identifier;
                        break;
                    }
                }
            }

            return _direction;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get the angle between this trigger forward and its interacting actor.
        /// </summary>
        /// <param name="_behaviour">Actor interacting with this trigger.</param>
        /// <returns>Signed angle between this trigger forward and the actor.</returns>
        private float GetActorAngle(EnhancedBehaviour _behaviour) {
            Transform _transform = collider.transform;

            // Vector normalization not required - produces the same result.
            Vector3 _direction = _behaviour.transform.position - collider.bounds.center;
            return Vector3.SignedAngle(_transform.forward, _direction, _transform.up);
        }
        #endregion
    }
}
