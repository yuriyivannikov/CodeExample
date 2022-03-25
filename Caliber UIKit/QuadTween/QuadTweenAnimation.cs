using System;
using Assets.UI.Colors;
using TheraBytes.BetterUi;
using UnityEngine;

namespace Plugins.QuadTween
{
    [Serializable]
    public class QuadTweenAnimation
    {
        [SerializeField]
        private GameObject _target;

        [SerializeField]
        private string _id;
        public string Id => _id;

        [SerializeField]
        private float _delay;
        public float Delay
        {
            get => _delay;
            set => _delay = value;
        }

        [SerializeField]
        private float _duration = 1.0f;
        public float Duration
        {
            get => _duration;
            set => _duration = value;
        }

        [SerializeField]
        private AnimationCurve _curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

        [SerializeField]
        private float _speed = 1.0f;
        public float Speed
        {
            get => _speed;
            set => _speed = value;
        }


        private float _time;

        // Animations

        [Flags]
        private enum AnimationType
        {
            Fade = 1 << 1,
            Move = 1 << 2,
            Rotate = 1 << 3,
            Scale = 1 << 4,
            Transform = 1 << 5,
            Color = 1 << 6,
            Material = 1 << 7
        }

        [SerializeField]
        private AnimationType _animationType;

        //

        #region Animation types
        [Serializable]
        private class AnimationItem
        {
            [SerializeField]
            private bool _overrideCurve;
            public bool OverrideCurve => _overrideCurve;

            [SerializeField]
            private AnimationCurve _curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
            public AnimationCurve Curve => _curve;

            [SerializeField]
            private bool _isForward = true;
            public bool IsForward => _isForward;

            [SerializeField]
            private bool _isBackward = true;
            public bool IsBackward => _isBackward;
        }

        [Serializable]
        private class AnimationFloat : AnimationItem
        {
            [SerializeField]
            private float _start;
            public float Start => _start;

            [SerializeField]
            private float _finish;
            public float Finish => _finish;

            public AnimationFloat()
            {
            }

            public AnimationFloat(float start, float finish)
            {
                _start = start;
                _finish = finish;
            }
        }

        [Serializable]
        private class AnimationVector3 : AnimationItem
        {
            [SerializeField]
            private Vector3 _start;
            public Vector3 Start => _start;

            [SerializeField]
            private Vector3 _finish;
            public Vector3 Finish => _finish;

            public AnimationVector3()
            {
            }

            public AnimationVector3(Vector3 start, Vector3 finish)
            {
                _start = start;
                _finish = finish;
            }
        }

        [Serializable]
        private class AnimationColor : AnimationItem
        {
            [SerializeField]
            private ColorLibraryItem _start;
            public ColorLibraryItem Start => _start;

            [SerializeField]
            private ColorLibraryItem _finish;
            public ColorLibraryItem Finish => _finish;

            public AnimationColor()
            {
            }

            public AnimationColor(ColorLibraryItem start, ColorLibraryItem finish)
            {
                _start = start;
                _finish = finish;
            }
        }

        [Serializable]
        private class AnimationTransform : AnimationItem
        {
            [Flags]
            public enum TransformType
            {
                Position = 1 << 1,
                Rotate = 1 << 2,
                Scale = 1 << 3,
                Size = 1 << 4
            }

            [SerializeField]
            private TransformType _type = TransformType.Position | TransformType.Rotate | TransformType.Scale | TransformType.Size;
            public TransformType Type => _type;

            [SerializeField]
            private RectTransform _start;
            public RectTransform Start => _start;

            [SerializeField]
            private RectTransform _finish;
            public RectTransform Finish => _finish;
        }

        [Serializable]
        private class AnimationMaterial : AnimationItem
        {
            [SerializeField]
            private Material _start;
            public Material Start => _start;

            [SerializeField]
            private Material _finish;
            public Material Finish => _finish;

            public AnimationMaterial()
            {
            }

            public AnimationMaterial(Material start, Material finish)
            {
                _start = start;
                _finish = finish;
            }
        }
        #endregion

        // Fade

        private bool IsFade => IsVisible && (_animationType & AnimationType.Fade) == AnimationType.Fade;
        //private string FadeLabel { get { return $"Fade: {_fade.Start} -> {_fade.Finish}"; } }

        [SerializeField]
        private AnimationFloat _fade = new AnimationFloat(0f, 1f);

        // Move

        private bool IsMove => IsVisible && (_animationType & AnimationType.Move) == AnimationType.Move;
        //private string MoveLabel { get { return $"Move: {_move.Start} -> {_move.Finish}"; } }

        [SerializeField]
        private AnimationVector3 _move = new AnimationVector3();

        // Rotate

        private bool IsRotate => IsVisible && (_animationType & AnimationType.Rotate) == AnimationType.Rotate;
        //private string RotateLabel { get { return $"Rotate: {_rotate.Start} -> {_rotate.Finish}"; } }

        [SerializeField]
        private AnimationVector3 _rotate = new AnimationVector3();

        // Rotate

        private bool IsScale => IsVisible && (_animationType & AnimationType.Scale) == AnimationType.Scale;
        //private string ScaleLabel { get { return $"Scale: {_scale.Start} -> {_scale.Finish}"; } }

        [SerializeField]
        private AnimationVector3 _scale = new AnimationVector3(new Vector3(1, 1, 1), new Vector3(1, 1, 1));

        // Color

        private bool IsColor => IsVisible && (_animationType & AnimationType.Color) == AnimationType.Color;
        //private string ColorLabel { get { return $"Color: {_color.Start} -> {_color.Finish}"; } }

        [SerializeField]
        private AnimationColor _color = new AnimationColor(new ColorLibraryItem(), new ColorLibraryItem());

        // Transform

        private bool IsTransform => IsVisible && (_animationType & AnimationType.Transform) == AnimationType.Transform;
        //private string MaterialLabel { get { return $"Material: {_material.Start} -> {_material.Finish}"; } }

        [SerializeField]
        private AnimationTransform _transform = new AnimationTransform();

        // Material

        private bool IsMaterial => IsVisible && (_animationType & AnimationType.Material) == AnimationType.Material;
        //private string MaterialLabel { get { return $"Material: {_material.Start} -> {_material.Finish}"; } }

        [SerializeField]
        private AnimationMaterial _material = new AnimationMaterial();

        //

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private BetterImage _image;

        #region Properties
        private CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = _target.GetComponent<CanvasGroup>();
                }
                return _canvasGroup;
            }
        }

        private RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = _target.transform as RectTransform;
                }
                return _rectTransform;
            }
        }

        private BetterImage Image
        {
            get
            {
                if (_image == null)
                {
                    _image = _target.GetComponent<BetterImage>();
                }
                return _image;
            }
        }
        #endregion

        //

        private bool IsVisible => _target != null;
        private string GroupName
        {
            get
            {
                var groupName = "Animation";
                if (!string.IsNullOrEmpty(_id))
                    groupName += "[" + _id + "]";
                if (_target != null)
                    groupName += ": " + _target.name;
                return groupName;
            }
        }

        public float Length => _delay + _duration;

        public void Init()
        {
            if (_target != null)
            {
                _canvasGroup = _target.GetComponent<CanvasGroup>();
                _rectTransform = _target.transform as RectTransform;
            }
        }

        public void SetTime(float time)
        {
            _time = time;
            var t = Mathf.Clamp01((_time - _delay) / _duration);
            var commonTime = _curve.Evaluate(time);
            SetTimeFade(t, commonTime);
            SetTimeMove(t, commonTime);
            SetTimeRotate(t, commonTime);
            SetTimeScale(t, commonTime);
            SetTimeTransform(t, commonTime);
            SetTimeColor(t, commonTime);
            SetTimeMaterial(t, commonTime);
        }

        public void AddTime(float time)
        {
            var addTime = time * _speed;
            _time = Mathf.Clamp(_time + addTime, 0, _delay + _duration);
            var t = Mathf.Clamp01((_time - _delay) / _duration);
            var commonTime = _curve.Evaluate(t);

            if (addTime > 0 && _fade.IsForward || addTime < 0 && _fade.IsBackward)
                SetTimeFade(t, commonTime);
            if (addTime > 0 && _move.IsForward || addTime < 0 && _move.IsBackward)
                SetTimeMove(t, commonTime);
            if (addTime > 0 && _rotate.IsForward || addTime < 0 && _rotate.IsBackward)
                SetTimeRotate(t, commonTime);
            if (addTime > 0 && _scale.IsForward || addTime < 0 && _scale.IsBackward)
                SetTimeScale(t, commonTime);
            if (addTime > 0 && _transform.IsForward || addTime <= 0 && _transform.IsBackward)
                SetTimeTransform(t, commonTime);
            if (addTime > 0 && _color.IsForward || addTime < 0 && _color.IsBackward)
                SetTimeColor(t, commonTime);
            if (addTime > 0 && _material.IsForward || addTime <= 0 && _material.IsBackward)
                SetTimeMaterial(t, commonTime);
        }

        private void SetTimeFade(float time, float commonTime)
        {
            if (IsFade && CanvasGroup != null)
            {
                time = _fade.OverrideCurve ? _fade.Curve.Evaluate(time) : commonTime;
                var value = Mathf.Lerp(_fade.Start, _fade.Finish, Mathf.Clamp(time, 0.0f, 1.0f));
                CanvasGroup.alpha = Mathf.Lerp(_fade.Start, _fade.Finish, Mathf.Clamp(time, 0.0f, 1.0f));
            }
        }

        private void SetTimeMove(float time, float commonTime)
        {
            if (IsMove && _target.transform != null)
            {
                time = _fade.OverrideCurve ? _move.Curve.Evaluate(time) : commonTime;
                var position = _target.transform.localPosition;
                if (_move.Start.x != _move.Finish.x)
                    position.x = (1f - time) * _move.Start.x + time * _move.Finish.x;
                if (_move.Start.y != _move.Finish.y)
                    position.y = (1f - time) * _move.Start.y + time * _move.Finish.y;
                if (_move.Start.z != _move.Finish.z)
                    position.z = (1f - time) * _move.Start.z + time * _move.Finish.z;
                _target.transform.localPosition = position;
            }
        }

        private void SetTimeRotate(float time, float commonTime)
        {
            if (IsRotate && _target.transform != null)
            {
                time = _rotate.OverrideCurve ? _rotate.Curve.Evaluate(time) : commonTime;
                _target.transform.rotation = Quaternion.Euler(Vector3.Lerp(_rotate.Start, _rotate.Finish, time));
            }
        }

        private void SetTimeScale(float time, float commonTime)
        {
            if (IsScale && _target.transform != null)
            {
                time = _scale.OverrideCurve ? _scale.Curve.Evaluate(time) : commonTime;
                _target.transform.localScale = Vector3.Lerp(_scale.Start, _scale.Finish, time);
            }
        }

        private void SetTimeColor(float time, float commonTime)
        {
            if (IsColor && Image != null)
            {
                time = _color.OverrideCurve ? _color.Curve.Evaluate(time) : commonTime;
                if (Image.ColoringMode == ColorMode.ColorLibrary)
                {
                    Image.ColorLibraryItem.Color = Color.Lerp(_color.Start, _color.Finish, time);
                }
                else
                {
                    Image.color = Color.Lerp(_color.Start, _color.Finish, time);
                }
            }
        }

        private void SetTimeTransform(float time, float commonTime)
        {
            if (IsTransform && RectTransform != null)
            {
                time = _transform.OverrideCurve ? _transform.Curve.Evaluate(time) : commonTime;

                if ((_transform.Type & AnimationTransform.TransformType.Position) == AnimationTransform.TransformType.Position)
                {
                    //RectTransform.pivot = Vector2.Lerp(_transform.Start.pivot, _transform.Finish.pivot, time);
                    RectTransform.position = Vector3.Lerp(
                        new Vector3(_transform.Start.position.x + _transform.Start.rect.x, _transform.Start.position.y + _transform.Start.rect.y, 0),
                        new Vector3(_transform.Finish.position.x + _transform.Finish.rect.x, _transform.Finish.position.y + _transform.Finish.rect.y, 0),
                        time);
                    //RectTransform.anchoredPosition = Vector2.Lerp(_transform.Start.anchoredPosition, _transform.Finish.anchoredPosition, time);
                }

                if ((_transform.Type & AnimationTransform.TransformType.Size) == AnimationTransform.TransformType.Size)
                {
                    RectTransform.sizeDelta = Vector2.Lerp(_transform.Start.rect.size, _transform.Finish.rect.size, time);
                    //RectTransform.anchorMax = Vector2.Lerp(_transform.Start.anchorMax, _transform.Finish.anchorMax, time);
                    //RectTransform.anchorMin = Vector2.Lerp(_transform.Start.anchorMin, _transform.Finish.anchorMin, time);
                }

                if ((_transform.Type & AnimationTransform.TransformType.Rotate) == AnimationTransform.TransformType.Rotate)
                    RectTransform.localRotation = Quaternion.Lerp(_transform.Start.localRotation, _transform.Finish.localRotation, time);

                if ((_transform.Type & AnimationTransform.TransformType.Scale) == AnimationTransform.TransformType.Scale)
                    RectTransform.localScale = Vector3.Lerp(_transform.Start.localScale, _transform.Finish.localScale, time);
            }
        }

        private void SetTimeMaterial(float time, float commonTime)
        {
            if (IsMaterial && _target.transform != null)
            {
                time = _material.OverrideCurve ? _material.Curve.Evaluate(time) : commonTime;
                //Transform.localScale = Vector3.Lerp(_material.Start, _material.Finish, time);
            }
        }
    }
}