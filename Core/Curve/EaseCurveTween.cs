// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

#if DOTWEEN_ENABLED
using DG.Tweening;
using DG.Tweening.Core;
using EnhancedEditor;
using System;
using UnityEngine;

using Min = EnhancedEditor.MinAttribute;

namespace EnhancedFramework.Core {
    // ===== Base ===== \\
    
    /// <summary>
    /// Base class for ease/curve values.
    /// <br/> Prefer using <see cref="EaseValue"/>/<see cref="CurveValue"/> instead of this.
    /// </summary>
    [Serializable]
    public abstract class EaseCurveTween<T> {
        #region Content
        /// <summary>
        /// Target value of the tween.
        /// </summary>
        public T Value = default;

        /// <summary>
        /// Duration of the tween.
        /// </summary>
        [Enhanced, Min(.001f)] public float Duration = 1f;
        [Enhanced, Min(0f)]    public float Delay    = 0f;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        protected EaseCurveTween() { }

        protected EaseCurveTween(T _value, float _duration, float _delay) {
            Duration = _duration;
            Value    = _value;
            Delay    = _delay;
        }

        // -------------------------------------------
        // Core
        // -------------------------------------------

        public abstract Tween SetEase(Tween _tween);

        private Tween ApplySettings(Tween _tween) {
            return SetEase(_tween.SetDelay(Delay));
        }
        #endregion

        #region Transform
        public Tween Move(Transform _transform, bool _snapping = false) {
            if (Value is Vector3 _value) {
                return ApplySettings(_transform.DOMove(_value, Duration, _snapping));
            }

            throw new InvalidTweenException("\"Move\" action can only be performed on Vector3 type tween objects");
        }

        public Tween Rotate(Transform _transform, RotateMode _mode = RotateMode.Fast) {
            if (Value is Vector3 _euler) {
                return ApplySettings(_transform.DORotate(_euler, Duration, _mode));
            }

            throw new InvalidTweenException("\"Rotate\" action can only be performed on Vector3 type tween objects.");
        }
        
        public Tween RotateQuaternion(Transform _transform) {
            if (Value is Quaternion _quaternion) {
                return ApplySettings(_transform.DORotateQuaternion(_quaternion, Duration));
            }

            throw new InvalidTweenException("\"Rotate Quaternion\" action can only be performed on Quaternion type tween objects.");
        }

        public Tween Scale(Transform _transform) {
            if (Value is Vector3 _scale) {
                return ApplySettings(_transform.DOScale(_scale, Duration));
            } else if (Value is float _scalef) {
                return ApplySettings(_transform.DOScale(_scalef, Duration));
            }

            throw new InvalidTweenException("\"Scale\" action can only be performed on Vector3 type tween objects.");
        }

        // -------------------------------------------
        // Local
        // -------------------------------------------

        public Tween LocalMove(Transform _transform, bool _snapping = false) {
            if (Value is Vector3 _value) {
                return ApplySettings(_transform.DOLocalMove(_value, Duration, _snapping));
            }

            throw new InvalidTweenException("\"Local Move\" action can only be performed on Vector3 type tween objects.");
        }

        public Tween LocalRotate(Transform _transform, RotateMode _mode = RotateMode.Fast) {
            if (Value is Vector3 _euler) {
                return ApplySettings(_transform.DOLocalRotate(_euler, Duration, _mode));
            }

            throw new InvalidTweenException("\"Local Rotate\" action can only be performed on Vector3 type tween objects.");
        }

        public Tween LocalRotateQuaternion(Transform _transform) {
            if (Value is Quaternion _quaternion) {
                return ApplySettings(_transform.DOLocalRotateQuaternion(_quaternion, Duration));
            }

            throw new InvalidTweenException("\"Local Rotate Quaternion\" action can only be performed on Quaternion type tween objects.");
        }
        #endregion

        #region Rect Transform
        public Tween AnchorPosition(RectTransform _rectTransform, bool _snapping = false) {
            if (Value is Vector3 _vector3) {
                return ApplySettings(_rectTransform.DOAnchorPos3D(_vector3, Duration, _snapping));
            }

            if (Value is Vector2 _vector2) {
                return ApplySettings(_rectTransform.DOAnchorPos(_vector2, Duration, _snapping));
            }

            throw new InvalidTweenException("\"AnchorPosition\" action can only be performed on Vector3 or Vector2 type tween objects");
        }

        public Tween AnchorMin(RectTransform _rectTransform) {
            if (Value is Vector2 _vector2) {
                return ApplySettings(_rectTransform.DOAnchorMin(_vector2, Duration));
            }

            throw new InvalidTweenException("\"AnchorMin\" action can only be performed or Vector2 type tween objects");
        }

        public Tween AnchorMax(RectTransform _rectTransform) {
            if (Value is Vector2 _vector2) {
                return ApplySettings(_rectTransform.DOAnchorMax(_vector2, Duration));
            }

            throw new InvalidTweenException("\"AnchorMax\" action can only be performed or Vector2 type tween objects");
        }

        public Tween SizeDelta(RectTransform _rectTransform, bool _snapping = false) {
            if (Value is Vector2 _value) {
                return ApplySettings(_rectTransform.DOSizeDelta(_value, Duration, _snapping));
            }

            throw new InvalidTweenException("\"SizeDelta\" action can only be performed or Vector2 type tween objects");
        }
        #endregion

        #region Vector
        public Tween To(DOGetter<Vector2> _getter, DOSetter<Vector2> _setter) {
            if (Value is Vector2 _value) {
                return ApplySettings(DOTween.To(_getter, _setter, _value, Duration));
            }

            throw new InvalidTweenException("\"Move\" action can only be performed on Vector3 type tween objects.");
        }

        public Tween To(DOGetter<Vector3> _getter, DOSetter<Vector3> _setter) {
            if (Value is Vector3 _value) {
                return ApplySettings(DOTween.To(_getter, _setter, _value, Duration));
            }

            throw new InvalidTweenException("\"Move\" action can only be performed on Vector3 type tween objects.");
        }
        #endregion

        #region Float
        public Tween To(DOGetter<float> _getter, DOSetter<float> _setter) {
            if (Value is float _value) {
                return ApplySettings(DOTween.To(_getter, _setter, _value, Duration));
            }

            throw new InvalidTweenException("\"Move\" action can only be performed on Vector3 type tween objects.");
        }
        #endregion
    }

    // ===== Derived ===== \\

    /// <summary>
    /// Wrapper utility class for an ease curve type tween.
    /// </summary>
    [Serializable]
    public sealed class EaseTween<T> : EaseCurveTween<T> {
        #region Content
        /// <summary>
        /// Ease used to evaluate this tween.
        /// </summary>
        public Ease Ease = Ease.OutSine;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        public EaseTween() { }

        public EaseTween(T _value, float _duration, float _delay, Ease _ease) : base(_value, _duration, _delay) {
            Ease = _ease;
        }

        // -------------------------------------------
        // Core
        // -------------------------------------------

        public override Tween SetEase(Tween _tween) {
            return _tween.SetEase(Ease);
        }
        #endregion
    }

    /// <summary>
    /// Wrapper utility class for an animation curve type tween.
    /// </summary>
    [Serializable]
    public sealed class CurveTween<T> : EaseCurveTween<T> {
        #region Content
        /// <summary>
        /// Animation curve used to evaluate this tween.
        /// </summary>
        [Enhanced, EnhancedCurve(0f, 0f, 1f, 1f, SuperColor.Lime)] public AnimationCurve Curve = AnimationCurve.Constant(0f, 1f, 0f);

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        public CurveTween() { }

        public CurveTween(T _value, float _duration, float _delay, AnimationCurve _curve) : base(_value, _duration, _delay) {
            Curve = _curve;
        }

        // -------------------------------------------
        // Core
        // -------------------------------------------

        public override Tween SetEase(Tween _tween) {
            return _tween.SetEase(Curve);
        }
        #endregion
    }

    #region Exception
    /// <summary>
    /// Exception for wrong-typed <see cref="EaseCurveTween{T}"/> when requesting to perform a tween.
    /// </summary>
    public sealed class InvalidTweenException : Exception {
        public InvalidTweenException() : base() { }

        public InvalidTweenException(string _message) : base(_message) { }

        public InvalidTweenException(string _message, Exception _innerException) : base(_message, _innerException) { }
    }
    #endregion
}
#endif
