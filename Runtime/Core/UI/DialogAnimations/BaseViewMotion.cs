using System;
using UnityEngine;

namespace Core
{
    public class BaseViewMotion : MonoBehaviour
    {
        public virtual void PrepareShow(BaseView view) { }

        public virtual void PrepareHide(BaseView view) { }

        public virtual void ShowMotion(BaseView view, Action onComplete = null)
        {
            onComplete?.Invoke();
        }

        public virtual void HideMotion(BaseView view, Action onComplete = null)
        {
            onComplete?.Invoke();
        }
    }
}


