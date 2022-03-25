using UnityEngine;

namespace GameClient
{
    public class TweenComponent : MonoBehaviour
    {
        public enum MotionDirection
        {
            None,
            UpScreenHeight,
            DownScreenHeight,
            LeftScreenWidth,
            RightScreenWidth,
            UpHeight,
            DownHeight,
            LeftWidth,
            RightWidth,
        }
        
        [SerializeField] private EasyTween _moveTween;
        [SerializeField] private EasyTween[] _tweens = {};
        public MotionDirection Direction;
        public AnimationCurve AnimationCurveOpen;
        public AnimationCurve AnimationCurveClose;

        public bool IsOpened => _moveTween.IsObjectOpened();
        

        private void Awake()
        {
            
        }

        public void OpenClose()
        {
            if (_moveTween != null)
            {
                Vector2 startPosition;
                switch (Direction)
                {
                    case MotionDirection.None:
                        startPosition = Vector2.zero;
                        break;
                    case MotionDirection.UpScreenHeight:
                        startPosition = new Vector2(0, -Screen.height);
                        break;
                    case MotionDirection.DownScreenHeight:
                        startPosition = new Vector2(0, Screen.height);
                        break;
                    case MotionDirection.LeftScreenWidth:
                        startPosition = new Vector2(Screen.width, 0);
                        break;
                    case MotionDirection.RightScreenWidth:
                        startPosition = new Vector2(-Screen.width, 0);
                        break;
                    case MotionDirection.UpHeight:
                        startPosition = new Vector2(transform.position.x, transform.position.y - (transform as RectTransform).rect.height);
                        break;
                    case MotionDirection.DownHeight:
                        startPosition = new Vector2(transform.position.x, transform.position.y + (transform as RectTransform).rect.height);
                        break;
                    case MotionDirection.LeftWidth:
                        startPosition = new Vector2(transform.position.x + (transform as RectTransform).rect.width, transform.position.y);
                        break;
                    case MotionDirection.RightWidth:
                        startPosition = new Vector2(transform.position.x - (transform as RectTransform).rect.width, transform.position.y);
                        break;
                    default:
                        Debug.LogError("Error");
                        return;
                }

                _moveTween.transform.localPosition = startPosition;
                _moveTween.SetAnimationPosition(startPosition, Vector2.zero, AnimationCurveOpen, AnimationCurveClose);
                _moveTween.OpenCloseObjectAnimation();
            }

            foreach (var tween in _tweens)
            {
                tween.OpenCloseObjectAnimation();
            }
        }
    }
}