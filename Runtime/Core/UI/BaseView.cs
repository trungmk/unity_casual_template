using UnityEngine;
using System;

namespace Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseView : MonoBehaviour
    {
        [Header("Currently this field (UseBlackImage) only use for dialog.")]
        public bool UseBlackImage;
        
        public int Key { get; set; }
    
        public string Name { get; set; } = string.Empty;

        public string ContextName { get; set; } = string.Empty;

        public UILayer Layer { get; set; }

        public Action<BaseView> OnHideCompleted { get; set; }

        public Action<BaseView> OnShowCompleted { get; set; }
    
        private CanvasGroup _canvasGroup;
    
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            OnAwake();
        }

        private void OnDisable()
        {
            IsPresenting = false;
            OnViewDisable();
        }

        protected virtual void OnAwake() { }
        
        protected virtual void OnViewDisable() { }
        
        /// <summary>
        /// Use this function instead Update()
        /// </summary>
        protected virtual void  OnUpdate() { }
        
        protected virtual void  OnLateUpdate() { }
    
        /// <summary>
        /// DO NOT directly use Update() method of Mono,
        /// Because when UI hide, the update function is still called. 
        /// </summary>
        private void Update()
        {
            if (!IsPresenting)
            {
                return;
            }
    
            OnUpdate();
        }
        
        private void LateUpdate()
        {
            if (!IsPresenting)
            {
                return;
            }
    
            OnLateUpdate();
        }
    
        public bool IsPresenting { get; private set; }
        
        protected virtual void OnPrepareShow(params object[] args) { }
    
        protected virtual void OnPrepareHide(params object[] args) { }
    
        protected virtual void OnShow(params object[] args)
        {
            OnShowCompleted?.Invoke(this);
        }
    
        protected virtual void OnHide(params object[] args)
        {
            OnHideCompleted?.Invoke(this);
        }
    
        public void Show(params object[] args)
        {
            OnPrepareShow(args);
            OnShow(args);
        }
    
        public void Present()
        {
            EnableView(true);
        }
        
        public void ForceHide()
        {
            EnableView(false);
        }
    
        public void Hide(params object[] args)
        {
            OnPrepareHide(args);
            OnHide(args);
            EnableView(false);
        }
        
        public void DestroyThisView()
        {
            if(gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    
        public void Reset()
        {
            OnHideCompleted = null;
            OnShowCompleted = null;
        }
    
        protected void EnableView(bool isEnable)
        {
            _canvasGroup.alpha = isEnable ? 1 : 0;
            _canvasGroup.blocksRaycasts = isEnable;
            IsPresenting = isEnable;
        }
    }
}

