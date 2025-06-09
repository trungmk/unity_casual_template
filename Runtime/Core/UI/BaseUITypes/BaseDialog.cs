using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BaseDialog : BaseView 
    {
        [SerializeField]
        private BaseViewMotion _motion = null;
    
        protected virtual void OnDialogShowed(params object[] args) { }

        protected virtual void OnDialogHidden(params object[] args) { }

        protected override void OnPrepareShow(params object[] args)
        {
            _motion.PrepareShow(this);
        }

        protected override void OnPrepareHide(params object[] args)
        {
            _motion.PrepareHide(this);
        }

        protected override void OnHide(params object[] args)
        {
            _motion.HideMotion(this, () =>
            {
                OnDialogHidden(args);

                if (OnHideCompleted != null)
                {
                    OnHideCompleted(this);
                }
            });
        }

        protected override void OnShow(params object[] args)
        {
            OnDialogShowed(args);
        
            _motion.ShowMotion(this, () =>
            {
                if (OnShowCompleted != null)
                {
                    OnShowCompleted(this);
                }
            });
        }
    }
}

