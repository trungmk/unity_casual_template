using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Core
{
    public class UIExecutionHandler : IHandlingExecution, IUIExecutionHandler
    {
        private readonly List<ViewExecution> _viewExecutionList = new List<ViewExecution>();
    
        protected IUIManager UIManager;

        protected IAssetManager AssetManager;

        protected ISceneManager SceneManager;

        public UIExecutionHandler(IUIManager uiManager, IAssetManager assetManager, ISceneManager sceneManager)
        {
            UIManager = uiManager;
            AssetManager = assetManager;
            SceneManager = sceneManager;
        }

        public ViewExecution SetupShowingExecution(int viewKey, Type viewType = null, params object[] args)
        {
            ViewExecution viewExecution = GetViewExecution(viewType);
            
            if (viewExecution == null)
            {
                viewExecution = new ViewExecution(this, viewKey, viewType);
                _viewExecutionList.Add(viewExecution);
            }
    
            return viewExecution;
        }
        
        public ViewExecution SetupWidgetShowingExecution(int viewKey, Type viewType = null, params object[] args)
        {
            ViewExecution viewExecution = GetWidgetViewExecution(viewKey);
            
            if (viewExecution == null)
            {
                viewExecution = new ViewExecution(this, viewKey, viewType)
                {
                    IsPresenting = true
                };
                
                _viewExecutionList.Add(viewExecution);
            }
            else
            {
                viewExecution.IsPresenting = true;
            }
            
            return viewExecution;
        }
        
        public ViewExecution SetupHidingExecution(Type viewType, bool isDestroy = false, params object[] args)
        {
            ViewExecution viewExecution = GetViewExecution(viewType);
            if (ReferenceEquals(viewExecution, null))
            {
                Debug.LogWarning("Unable to hide:= " + viewType + " cause the ViewExecution instance is null.");
                return null;
            }
            
            BaseView view = UIManager.GetCache(viewExecution.ViewKey);
            if (!ReferenceEquals(view, null))
            {
                viewExecution.HidingHandler.Clear();
                viewExecution.HidingHandler.SetHandler(this);
                
                return viewExecution;
            }
                
            Debug.LogError("View := " + viewType + " is NULL. Could not setup to hide this view");
            return null;
        }
        
        public ViewExecution SetupHidingExecution(int viewKey, bool isDestroy = false, params object[] args)
        {
            ViewExecution viewExecution = GetViewExecution(viewKey);
            if (ReferenceEquals(viewExecution, null))
            {
                Debug.LogWarning("Unable to hide:= " + viewKey + " cause the ViewExecution instance is null.");
                return null;
            }
            
            BaseView view = UIManager.GetCache(viewExecution.ViewKey);
            if (!ReferenceEquals(view, null))
            {
                viewExecution.HidingHandler.Clear();
                viewExecution.HidingHandler.SetHandler(this);
                
                return viewExecution;
            }
                
            Debug.LogError("View := " + viewKey + " is NULL. Could not hide this view  this view");
            return null;
        }
        
        public ViewExecution SetupWidgetHidingExecution(WidgetView view, bool isDestroy = false, params object[] args)
        {
            if (view == null)
            {
                Debug.LogWarning("Unable to hide widget because the view need to hide is null!");
            }
            
            ViewExecution viewExecution = GetWidgetViewExecution(view);
            if (ReferenceEquals(viewExecution, null))
            {
                Debug.LogWarning("Unable to hide:= " + view.Name + " cause the ViewExecution instance is null.");
                return null;
            }
            
            if (!ReferenceEquals(view, null))
            {
                viewExecution.HidingHandler.SetHandler(this);
                
                return viewExecution;
            }

            Debug.LogError("View:= " + view.Name + " is NULL. Could not setup hide widget");
            return null;
        }
    
        private ViewExecution GetViewExecution(int viewKey)
        {
            for (int i = 0; i < _viewExecutionList.Count; i++)
            {
                ViewExecution viewExecution = _viewExecutionList[i];
                
                if (viewExecution != null && viewExecution.ViewKey == viewKey)
                {
                    return viewExecution;
                }
            }
    
            return null;
        }
        
        private ViewExecution GetViewExecution(Type type)
        {
            for (int i = 0; i < _viewExecutionList.Count; i++)
            {
                if (_viewExecutionList != null && _viewExecutionList[i].ViewType == type)
                {
                    return _viewExecutionList[i];
                }
            }
    
            return null;
        }
        
        private ViewExecution GetViewExecution(BaseView view)
        {
            for (int i = 0; i < _viewExecutionList.Count; i++)
            {
                if (_viewExecutionList[i].View == view)
                {
                    return _viewExecutionList[i];
                }
            }
    
            return null;
        }
        
        private ViewExecution GetWidgetViewExecution(WidgetView view)
        {
            for (int i = 0; i < _viewExecutionList.Count; i++)
            {
                ViewExecution viewExecution = _viewExecutionList[i];
                if (viewExecution.View == view && viewExecution.View == view)
                {
                    return _viewExecutionList[i];
                }
            }
    
            return null;
        }
        
        private ViewExecution GetWidgetViewExecution(int viewKey)
        {
            for (int i = 0; i < _viewExecutionList.Count; i++)
            {
                ViewExecution viewExecution = _viewExecutionList[i];
                if (viewExecution.ViewKey == viewKey && viewExecution.CouldShow())
                {
                    return _viewExecutionList[i];
                }
            }
    
            return null;
        }
    
        public virtual async void RunShowingExecution(ShowingUIHandler openingHandler, bool isWidget = false)
        {
            int viewKey = openingHandler.ViewKey;
            BaseView view = isWidget ? openingHandler.View : UIManager.GetCache(viewKey);
    
            // If the view isn't existing then we're going to creating one.
            if (view == null)
            {
                UIData uiData = UIHandler.GetUIData(viewKey);
                BaseView viewInstance = await LoadUI(uiData.Name);

                if (viewInstance == null)
                {
                    Debug.LogError("viewInstance RunShowingExecution null!");
                    return;
                }

                // Setup UI View info
                view = viewInstance;
                view.Name = uiData.Name;
                view.Key = viewKey;
                view.Layer = uiData.Layer;
                view.OnShowCompleted = openingHandler.GetCallback();
                view.ContextName = SceneManager.CurrentScene.SceneName;
                
                openingHandler.View = view;
                UIManager.Views.Add(view);
                    
                openingHandler.ViewExecution.IsPresenting = true;
                ShowView(view, openingHandler.Args);
            }
            else
            {
                if (!view.gameObject.activeSelf || !view.isActiveAndEnabled)
                {
                    view.gameObject.SetActive(true);
                }
                
                Action<BaseView> callback = openingHandler.GetCallback();
                
                if (!UIManager.IsPresenting(view.Key))
                {
                    view.OnShowCompleted = null;
                    view.OnShowCompleted = callback;
                
                    openingHandler.ViewExecution.IsPresenting = true;
                    
                    ShowView(view, openingHandler.Args);
                }
                
                openingHandler.Clear();
            }

#if UNITY_EDITOR

            var replace = view.name.Replace("[ACTIVE]", string.Empty);
            view.name = replace;
            view.name += "[ACTIVE]";
#endif
        }
    
        public virtual void RunHidingExecution(HidingUIHandler hidingHandler, bool isWidget = false)
        {
            UIHidingExecutionData hidingData = hidingHandler.GetHidingData();
            BaseView view = isWidget ? hidingHandler.View : UIManager.GetCache(hidingData.ViewKey);
    
            if (!ReferenceEquals(hidingData.Callback, null))
            {
                view.OnHideCompleted += hidingData.Callback;
            }
          
            if (hidingData.IsDestroy)
            {
                if (view != null)
                {
                    view.OnHideCompleted += (v) =>
                    {
                        ViewExecution viewExecution = GetViewExecution(hidingData.ViewKey);
                        if (!ReferenceEquals(viewExecution, null))
                        {
                            _viewExecutionList.Remove(viewExecution);
                        }
                    
                        UIViewStack viewContainer = UIManager.GetLayer(view.Layer);
                        viewContainer.PopView(view);
                    
                        UIManager.Views.Remove(view);
                        view.DestroyThisView();
                    };
                }
                else
                {
                    Debug.LogError("Got view from cache and then it null! why?");
                }
            }
            else
            {
                if (view != null)
                {
                    view.OnHideCompleted += (viewNeedToHide) =>
                    {
                        if (viewNeedToHide == null) 
                            return;
                        
#if UNITY_EDITOR
                        view.name = view.name.Replace("[ACTIVE]", "");
#endif
                        
                        UIViewStack viewContainer = UIManager.GetLayer(viewNeedToHide.Layer);
                        viewContainer.PopView(viewNeedToHide);
                        viewNeedToHide.OnHideCompleted = null;
                        if (hidingData.IsDisable)
                        {
                            view.gameObject.SetActive(false);
                        }
                        
                        hidingHandler.ViewExecution.IsPresenting = false;
                    };
                }
                else
                {
                    Debug.LogError("Got view from cache and then it null! why?");
                }
            }
            
            if (view != null)
            {
                view.Hide(hidingData.Args);
            }
        }
        
        protected async Task<BaseView> LoadUI(string address)
        {
            GameObject obj = await AssetManager.InstantiateAsync(address);

            if (obj != null)
            {
                return obj.GetComponent<BaseView>();
            }
            
            Debug.LogError("LoadUI could not load asset address:= " + address);
            return null;
        }
    
        public void RemoveViewExecutionByKey(int viewKey)
        {
            ViewExecution viewExecution = GetViewExecution(viewKey);
            if (viewExecution != null)
            {
                _viewExecutionList.Remove(viewExecution);
            }
        }
        
        /// <summary>
        /// Choose appropriate Layer and push view into it.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="arg"></param>
        protected void ShowView(BaseView view, params object[] arg)
        {
            UIViewStack stack = UIManager.GetLayer(view.Layer);
            if(ReferenceEquals(stack, null))
            {
                Debug.LogError("Could not get layer := " + view.Layer);
                return;
            }
    
            stack.PushView(view);
            view.Show(arg);
        }
    }
}

