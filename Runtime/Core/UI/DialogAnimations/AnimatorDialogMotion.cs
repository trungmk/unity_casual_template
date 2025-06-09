using System;
using UnityEngine;

namespace Core
{
    public class AnimatorDialogMotion : BaseViewMotion
    {
        [SerializeField]
        public Animator _animator;
        
        private const string SHOW_TRIGGER = "show";

        private const string HIDE_TRIGGER = "hide";

        private static readonly int _showAnimHash = Animator.StringToHash(SHOW_TRIGGER);

        private static readonly int _hideAnimHash = Animator.StringToHash(HIDE_TRIGGER);

        private Action _onShowCompleted;

        private Action _onHideCompleted;


#if UNITY_EDITOR
    
        private void OnValidate()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }
    
#endif

        public override void PrepareShow(BaseView view) { }

        public override void PrepareHide(BaseView view) { }

        public override void ShowMotion(BaseView view, Action onComplete = null)
        {
            if (_animator == null)
            {
                onComplete?.Invoke();
                return;
            }

            _animator.SetTrigger(_showAnimHash);
            _onShowCompleted = onComplete;
        }

        public override void HideMotion(BaseView view, Action onComplete = null)
        {
            if (_animator == null)
            {
                onComplete?.Invoke();
                return;
            }

            _animator.SetTrigger(_hideAnimHash);
            _onHideCompleted = onComplete;
        }

        public void OnCompleteAnimationShow()
        {
            _onShowCompleted?.Invoke();
        }

        public void OnCompleteAnimationHide()
        {
            _onHideCompleted?.Invoke();
        }
    }
}

