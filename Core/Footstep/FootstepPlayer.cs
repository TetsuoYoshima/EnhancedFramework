// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using EnhancedEditor;
using UnityEngine;

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Enum used to reference a foot.
    /// </summary>
    public enum Foot {
        Left    = 0,
        Right   = 1,
    }

    /// <summary>
    /// Utility class used to play various footstep feedbacks, including audio and FX.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Utility/Footstep Player"), DisallowMultipleComponent]
    public sealed class FootstepPlayer : EnhancedBehaviour, ILateUpdate {
        #region Mode
        /// <summary>
        /// Determines how this player events are triggered.
        /// </summary>
        public enum Mode {
            [Tooltip("Disables this footstep player")]
            Disabled = 0,

            [Separator(SeparatorPosition.Top)]

            [Tooltip("Uses animation events to play footstep effects")]
            AnimationEvent = 1,

            [Tooltip("Automatically plays events according to foot positions")]
            FootPosition   = 2,
        }
        #endregion

        public override UpdateRegistration UpdateRegistration {
            get {
                UpdateRegistration _value = base.UpdateRegistration | UpdateRegistration.Init;

                #if !UNITY_EDITOR
                if (!UseFootPositionMode)
                    return _value;
                #endif

                _value |= UpdateRegistration.Late;
                return _value;
            }
        }

        #region Global Members
        [Section("Footstep Player")]

        [Tooltip("Footsteps audio database to play")]
        [SerializeField, Enhanced, Required] private AudioMaterialDatabase audioDatabase = null;

        [Tooltip("Additional feedbacks to play on footstep")]
        [SerializeField] private EnhancedFeedbacks feedbacks = new EnhancedFeedbacks();

        [Space(10f)]

        [Tooltip("Transform reference of this object left foot (used to play audio and detect ground material)")]
        [SerializeField, Enhanced, Required] private Transform leftFoot = null;

        [Tooltip("Transform reference of this object right foot (used to play audio and detect ground material)")]
        [SerializeField, Enhanced, Required] private Transform rightFoot = null;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Mode used trigger this player footsteps")]
        [SerializeField] private Mode mode = Mode.FootPosition;

        [Tooltip("Collision mask used to detect ground material and play the associated audio")]
        [SerializeField] private LayerMask mask = new LayerMask();
        
        [Space(5f)]

        [Tooltip("If true, uses one cooldown per foot - otherwise, uses one cooldown for both")]
        [SerializeField] private bool oneCooldownPerFoot = true;

        [Tooltip("Whether this object is in 2D or 3D space (used for raycasting)")]
        [SerializeField, Enhanced, DisplayName("2D")] private bool is2D = false;

        [Space(10f)]

        [Tooltip("Minimum interval delay between footsteps occurrence")]
        [SerializeField, Enhanced, Range(0f, 2f)] private float minInterval = .2f;

        [Tooltip("Distances used to detect if a foot is on ground: first value for entering in contact, second for exiting contact")]
        [SerializeField, Enhanced, ShowIf(nameof(UseFootPositionMode)), MinMax(0f, .2f)] private Vector2 groundDetectionTolerance = new Vector2(.02f, .025f);

        // -----------------------

        /// <summary>
        /// Indicates if this player uses the <see cref="Mode.FootPosition"/>.
        /// </summary>
        public bool UseFootPositionMode {
            get { return mode == Mode.FootPosition; }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnInit() {
            base.OnInit();

            // Cooldown init.
            GetFootCooldown(Foot.Right).Reload(minInterval);
            GetFootCooldown(Foot.Left) .Reload(minInterval);
        }

        void ILateUpdate.Update() {

            #if UNITY_EDITOR
            if (!UseFootPositionMode) {
                return;
            }
            #endif

            UpdateFootsteps();
        }

        #if UNITY_EDITOR
        // -------------------------------------------
        // Editor
        // -------------------------------------------

        protected override void OnValidate() {
            base.OnValidate();

            // Feet.
            if (!rightFoot && transform.FindChildResursive("rightfoot", out Transform _rightFoot, false)) {
                rightFoot = _rightFoot;
            }

            if (!leftFoot  && transform.FindChildResursive("leftfoot",  out Transform _leftFoot,  false)) {
                leftFoot = _leftFoot;
            }
        }
#endif
        #endregion

        #region Mode Callbacks
        private bool isRightFootGrounded = true;
        private bool isLeftFootGrounded  = true;

        // -----------------------

        /// <summary>
        /// Called from a <see cref="FootstepAnimationEvent"/> to play a specific footstep feedback.
        /// </summary>
        /// <param name="_foot">Foot to play the associated feedback.</param>
        internal void OnAnimationEvent(Foot _foot) {

            if (mode != Mode.AnimationEvent) {
                return;
            }

            PlayFootstep(_foot);
        }

        /// <summary>
        /// Updates this player foot positions and play the associated feedbacks.
        /// </summary>
        private void UpdateFootsteps() {

            UpdateFoot(Foot.Right, ref isRightFootGrounded);
            UpdateFoot(Foot.Left,  ref isLeftFootGrounded);

            // ----- Local Method ----- \\

            void UpdateFoot(Foot _foot, ref bool _oldGrounded) {

                Transform _transform = GetFootTransform(_foot, out Vector3 _local);

                float _height = _oldGrounded ? groundDetectionTolerance.y : groundDetectionTolerance.x;
                bool _newGrounded = (_local.y - _height) <= 0f;

                // Contact with ground - play foostep.
                if (_newGrounded && !_oldGrounded) {
                    PlayFootstep(_foot, _transform, _local);
                }

                _oldGrounded = _newGrounded;
            }
        }
        #endregion

        #region Footsteps
        private const float CastDistanceOffset = .1f;

        private readonly Cooldown cooldown = new Cooldown();
        private readonly Cooldown secondCooldown = new Cooldown();

        /// <summary>
        /// Contact offset used for raycasting.
        /// </summary>
        public float ContactOffset {
            get {
                float _offset = is2D ? Physics2D.defaultContactOffset : Physics.defaultContactOffset;
                return (_offset * 2f) + CastDistanceOffset;
            }
        }

        // -----------------------

        /// <inheritdoc cref="PlayFootstep(Foot, Transform, Vector3)"/>
        public bool PlayFootstep(Foot _foot) {
            Transform _transform = GetFootTransform(_foot, out Vector3 _localPosition);
            return PlayFootstep(_foot, _transform, _localPosition);
        }

        /// <summary>
        /// Plays foostep feedbacks for a specific <see cref="Foot"/>.
        /// </summary>
        /// <param name="_foot"></param>
        /// <param name="_transform">Transform of this foot.</param>
        /// <param name="_localPosition">Local position of this foot.</param>
        /// <returns>True if the feedbacks could be played, false otherwise.</returns>
        private bool PlayFootstep(Foot _foot, Transform _transform, Vector3 _localPosition) {

            // Cooldown.
            Cooldown _cooldown = GetFootCooldown(_foot);
            if (!_cooldown.IsValid) {
                return false;
            }

            _cooldown.Reload(minInterval);

            // Audio.
            if (GetMaterial(_transform.position, _localPosition.y, out Material _material)) {

                AudioAsset _asset = audioDatabase.GetAsset(_material);

                _asset.Play(_transform, FeedbackPlayOptions.PlayAtPosition);
                _cooldown.Reload(minInterval);
            }

            // Feedbacks.
            feedbacks.Play(_transform, FeedbackPlayOptions.PlayAtPosition);

            return true;
        }

        /// <summary>
        /// Get the ground material of the surface located below a specific foot position.
        /// </summary>
        /// <param name="_position">Foot position to detect below surface.</param>
        /// <param name="_offset">Foot vertical offset.</param>
        /// <param name="_material">Material of the surface below this foot (null if none).</param>
        /// <returns>True if a surface could be successfully detected, false otherwise.</returns>
        private bool GetMaterial(Vector3 _position, float _offset, out Material _material) {
            
            // 2D.
            if (is2D) {

                this.LogErrorMessage("2D Detection not implemented");

                _material = null;
                return false;
            }

            // 3D.
            float _distance = _offset + ContactOffset;
            if (Physics.Raycast(_position, -transform.up, out RaycastHit _hit, _distance, mask.value, QueryTriggerInteraction.Ignore)) {
                return _hit.GetSharedMaterial(out _material);
            }

            _material = null;
            return false;
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get the <see cref="Cooldown"/> associated with a specific foot.
        /// </summary>
        /// <param name="_foot">Foot to get the associated cooldown.</param>
        /// <returns>Cooldown associated with this foot.</returns>
        public Cooldown GetFootCooldown(Foot _foot) {

            if (!oneCooldownPerFoot) {
                return cooldown;
            }

            switch (_foot) {

                case Foot.Left:
                    return cooldown;

                case Foot.Right:
                default:
                    return secondCooldown;
            }
        }

        /// <summary>
        /// Get the <see cref="Transform"/> of a specific foot.
        /// </summary>
        /// <param name="_foot">Foot to get the associated transform.</param>
        /// <param name="_localPosition">Local position of this foot.</param>
        /// <returns>This foot <see cref="Transform"/>.</returns>
        public Transform GetFootTransform(Foot _foot, out Vector3 _localPosition) {

            Transform _transform = transform;
            Transform _footTransform;

            switch (_foot) {

                case Foot.Left:
                    _footTransform = leftFoot;
                    break;

                case Foot.Right:
                default:
                    _footTransform = rightFoot;
                    break;
            }

            _localPosition = (_footTransform.position - _transform.position).RotateInverse(_transform.rotation);
            return _footTransform;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Setups this object reference foot using its <see cref="Animator"/> associated bones.
        /// </summary>
        [Button(SuperColor.Green, IsDrawnOnTop = false)]
        public void SetupFootBones() {

            if (!this.TryGetComponentInChildren(out Animator _animator)) {
                this.LogWarningMessage("No Animator could be found on this object - associated bones could be be retrieved");
                return;
            }

            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Setup Foot Bones");
            #endif

            rightFoot = _animator.GetBoneTransform(HumanBodyBones.RightFoot);
            leftFoot  = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        }
        #endregion
    }
}
