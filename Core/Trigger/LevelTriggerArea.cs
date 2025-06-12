// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using ArrayUtility = EnhancedEditor.ArrayUtility;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Centralizes the behaviour of an area of multiple <see cref="EnhancedBehaviour"/>.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Trigger/Level Trigger Area")]
    public sealed class LevelTriggerArea : LevelTrigger, IDynamicUpdate {
        #region Handles Mode
        /// <summary>
        /// Mode used to determine which handles should be drawn.
        /// </summary>
        private enum HandlesMode {
            None    = 0,

            [Separator(SeparatorPosition.Top)]

            Area    = 1,
            Portals = 2,
        }
        #endregion

        #region Global Members
        #if UNITY_EDITOR
        [Tooltip("Select which handles should be drawn")]
        [SerializeField] private HandlesMode handles = HandlesMode.Area;

        [Space(5f)]

        [Tooltip("Edits this area vertices and positions")]
        [SerializeField, Enhanced, ToggleButton("EditCollider")] private bool editArea = false;


        [Tooltip("When active, always draw this trigger area in the scene view")]
        [SerializeField] private bool pinArea = false;

        [Space(10f)]
        #endif

        [Tooltip("All vertices of this area")]
        [SerializeField] private List<Vector3> areaVertices = new List<Vector3>();

        // -----------------------

        /// <summary>
        /// All vertices of this area.
        /// </summary>
        public List<Vector3> AreaVertices {
            get { return areaVertices; }
        }
        #endregion

        #region Enhanced Behaviour
        void IDynamicUpdate.Update() {
            AreaUpdate();
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        private static class Styles {
            public static readonly GUIStyle HandleStyle = new GUIStyle() {
                fontStyle = FontStyle.Bold,
                fontSize  = 18,
                normal    = new GUIStyleState() {
                    textColor = SuperColor.SmokyBlack.Get()
                },
            };
        }

        private const float AreaPointHandlesAlpha = .7f;
        private const float AreaPointHandlesSize  = .2f;

        private static Tool lastTool  = Tool.None;
        private static bool isEditing = false;
        private static int editingAreaID = -1;

        // -----------------------

        protected internal override void OnDrawHandles() {

            // Area edit.
            if (isEditing != editArea) {
                isEditing = editArea;

                // Tool setup.
                if (isEditing) {

                    lastTool = Tools.current;
                    editingAreaID = GetInstanceID();

                    Tools.current = Tool.Custom;

                } else {

                    Tools.current = lastTool;
                    SetupCollider();
                }
            }

            DrawArea(true);
        }

        internal void DrawArea(bool _forceDraw = false) {

            if (!_forceDraw && !pinArea) {
                return;
            }

            Transform _transform = transform;
            Quaternion _rotation = _transform.rotation;
            Vector3 _position    = _transform.position;

            if (isEditing && (editingAreaID == GetInstanceID())) {

                for (int i = 0; i < areaVertices.Count; i++) {

                    Vector3 _pos = areaVertices[i];
                    _pos = Handles.PositionHandle(_position + _pos.Rotate(_rotation), _rotation) - _position;

                    areaVertices[i] = _pos.RotateInverse(_rotation);
                }
            }

            // No enough vertices to draw an area.
            if ((areaVertices.Count < 2) || (handles == HandlesMode.None)) {
                return;
            }

            Vector3[] _vertices = areaVertices.ConvertAll(p => _position + p.Rotate(_rotation)).ToArray();
            ArrayUtility.Add(ref _vertices, _vertices.First());

            SuperColor _color = actors.ContainsValue(true) ? SuperColor.Green : SuperColor.Crimson;
            Color _wireframeColor = SuperColor.Black.Get();

            // Handles mangement.
            if (handles == HandlesMode.Portals) {
                base.OnDrawHandles();
                _wireframeColor = _color.Get();

            } else {
                using (var _scope = EnhancedGUI.HandlesColor.Scope(_color.Get(.4f))) {
                    Handles.DrawAAConvexPolygon(_vertices);
                }
            }

            // Area.
            using (var _scope = EnhancedGUI.HandlesColor.Scope(_wireframeColor)) {
                Handles.DrawAAPolyLine(_vertices);
            }

            using (var _scope = EnhancedGUI.HandlesColor.Scope(SuperColor.HarvestGold.Get(AreaPointHandlesAlpha))) {
                for (int i = 0; i < _vertices.Length - 1; i++) {

                    Vector3 _vertice = _vertices[i];
                    Handles.SphereHandleCap(0, _vertice, Quaternion.identity, AreaPointHandlesSize, EventType.Repaint);

                    if (isEditing) {
                        Handles.Label(_vertice + _transform.up * .5f, i.ToString(), Styles.HandleStyle);
                    }
                }
            }
        }
        #endif
        #endregion

        #region Trigger
        public const float UpdateCooldwonDefaultDuration = .05f;
        private readonly PairCollection<ITriggerActor, bool> actors = new PairCollection<ITriggerActor, bool>();

        /// <summary>
        /// Whether this trigger is currently active and updated or not.
        /// </summary>
        private bool IsActive {
            get { return actors.Count != 0; }
        }

        // -----------------------

        /// <summary>
        /// Updates this area.
        /// </summary>
        private void AreaUpdate() {

            Transform _transform = transform;
            Quaternion _rotation = _transform.rotation;
            Vector3 _position    = _transform.position;

            // Check if any actor entered / exited the area.
            ref List<Pair<ITriggerActor, bool>> _span = ref actors.collection;

            for (int i = _span.Count; i-- > 0;) {

                Pair<ITriggerActor, bool> _pair = _span[i];
                ITriggerActor _actor = _pair.First;

                bool _inArea = IsInArea(_actor);

                // State update.
                if (_inArea != _pair.Second) {

                    if (_inArea) {
                        OnEnterArea(_actor);
                    } else {
                        OnExitArea(_actor);
                    }

                    _pair.Second = _inArea;

                    // Security (happens when instantly exiting the trigger).
                    if ((i < _span.Count) && (_actor == _span[i].First)) {
                        _span[i] = _pair;
                    }
                }
            }

            // ----- Local Method ----- \\

            bool IsInArea(ITriggerActor _actor) {
                Vector3 _actorLocal = (_actor.Behaviour.Transform.position - _position).RotateInverse(_rotation);
                return EnhancedGeometryUtility.PointInPolygon(_actorLocal, areaVertices);
            }
        }

        // -------------------------------------------
        // Trigger
        // -------------------------------------------

        protected override void OnEnterTrigger(ITriggerActor _actor, EnhancedBehaviour _behaviour) {

            // Flag requirement.
            if (!requiredFlags.Valid) {
                return;
            }

            // Already registered.
            if (actors.ContainsKey(_actor)) {
                return;
            }

            // Registration.
            if (!IsActive) {
                UpdateManager.Instance.Register(this, UpdateRegistration.Dynamic);
            }

            // Register actor.
            actors.Add(_actor, false);
            AreaUpdate();
        }

        protected override void OnExitTrigger(ITriggerActor _actor, EnhancedBehaviour _behaviour) {

            // Invalid object.
            if (!actors.TryGetValue(_actor, out bool _active)) {
                return;
            }

            if (_active) {
                OnExitArea(_actor);
            }

            actors.Remove(_actor);

            // Unregistration.
            if (!IsActive) {
                UpdateManager.Instance.Unregister(this, UpdateRegistration.Dynamic);
            }
        }

        // -------------------------------------------
        // Area
        // -------------------------------------------

        private void OnEnterArea(ITriggerActor _actor) {
            base.OnEnterTrigger(_actor, _actor.Behaviour);
        }

        private void OnExitArea(ITriggerActor _actor) {
            base.OnExitTrigger(_actor, _actor.Behaviour);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Setups this trigger collider so that it encapsulates all vertices in the area.
        /// </summary>
        [ContextMenu("Setup Collider", false, 100)]
        private void SetupCollider() {

            if (collider == null) {
                return;
            }

            Transform _transform = collider.transform;
            Quaternion _rotation = _transform.rotation;

            _transform.rotation = Quaternion.identity;

            Vector3 _origin = _transform.position;

            if (areaVertices.SafeFirst(out Vector3 _offset)) {
                _origin += _offset;
            }

            Bounds _bounds = new Bounds(_origin, Vector3.up);

            foreach (Vector3 _vertice in areaVertices) {
                _bounds.Encapsulate(_transform.position + _vertice);
            }

            Vector3 _scale      = _transform.lossyScale;
            Vector3 _center     = _bounds.center - _transform.position;
            Vector3 _extents    = _bounds.extents;

            if (!_scale.IsNull()) {

                _center = _center.Divide(_scale);
                _extents = _extents.Divide(_scale);
            }

            _bounds.center  = _center;
            _bounds.extents = new Vector3(Mathf.Abs(_extents.x), Mathf.Abs(_extents.y), Mathf.Abs(_extents.z));

            _transform.rotation = _rotation;

            if (!_scale.IsNull()) {
                _bounds.center  = _bounds.center.Divide(_scale);
                _bounds.extents = _bounds.extents.Divide(_scale);
            }

            switch (collider) {
                case CapsuleCollider _caspule:
                    _caspule.center = _bounds.center;
                    _caspule.radius = _bounds.extents.Max();
                    break;

                case SphereCollider _sphere:
                    _sphere.center = _bounds.center;
                    _sphere.radius = _bounds.extents.Max();
                    break;

                case BoxCollider _box:
                    _box.center = _bounds.center;
                    _box.size   = _bounds.size;
                    break;

                default:
                    break;
            }
        }
        #endregion

        #region Play Mode Data
        public override bool CanSavePlayModeData {
            get { return true; }
        }

        // -----------------------

        public override void SavePlayModeData(PlayModeEnhancedObjectData _data) {

            // Save vertices.
            _data.Vectors.AddRange(areaVertices);
        }

        public override void LoadPlayModeData(PlayModeEnhancedObjectData _data) {

            // Load vertices.
            areaVertices = _data.Vectors;
            SetupCollider();
        }
        #endregion
    }
}
