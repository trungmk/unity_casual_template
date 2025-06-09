using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public class WidgetView : BaseView
    {
        [SerializeField]
        private BaseViewMotion _motion = null;

        protected virtual void OnWidgetShowed(params object[] args) { }

        protected virtual void OnWidgetHided(params object[] args) { }
	
        /// <summary>
        /// DO NOT override this function
        /// </summary>
        /// <param name="args"></param>
        protected override void OnHide(params object[] args)
        {
            if (_motion != null)
            {
                _motion.HideMotion(this, () =>
                {
                    OnWidgetHided(args);
                    OnHideCompleted?.Invoke(this);
                });
            }
            else
            {
                OnHideCompleted?.Invoke(this);
            }
        }

        /// <summary>
        /// DO NOT override this function
        /// </summary>
        /// <param name="args"></param>
        protected override void OnShow(params object[] args)
        {
            OnWidgetShowed(args);
            
            OnShowCompleted?.Invoke(this);
            
            if (_motion != null)
            {
                _motion.ShowMotion(this);
            }
        }
        
        protected override void OnPrepareShow(params object[] args)
        {
            if (_motion != null)
            {
                _motion.PrepareShow(this);
            }
        }

        protected override void OnPrepareHide(params object[] args)
        {
            if (_motion != null)
            {
                _motion.PrepareHide(this);
            }
        }
    }
}


