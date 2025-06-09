using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Core
{
    /// <summary>
    /// Manages the lifecycle of UI views and widgets, including showing, hiding, and caching.
    /// </summary>
    public class UIManager : BaseSystem, IUIManager
    {
        [SerializeField] private UIViewStack[] _uiViewStacks = null;

        /// <summary>
        /// List of all active views managed by the UIManager.
        /// </summary>
        public List<BaseView> Views { get; } = new List<BaseView>();

        private IUIExecutionHandler _executionHandler;
        private ISceneManager _sceneManager;
        private IAssetManager _assetManager;

        /// <summary>
        /// Injects dependencies into the UIManager.
        /// </summary>
        /// <param name="assetManager">The asset manager for loading UI assets.</param>
        /// <param name="sceneManager">The scene manager for managing scene-related UI logic.</param>
        [Inject]
        public void Construct(ISceneManager sceneManager, IAssetManager assetManager)
        {
            _sceneManager = sceneManager;
            _assetManager = assetManager;
        }

        private void Awake()
        {
            _executionHandler = new UIExecutionHandler(this, _assetManager, _sceneManager);
        }

        /// <summary>
        /// Shows a UI view of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the view to show.</typeparam>
        /// <param name="args">Optional arguments to pass to the view.</param>
        /// <returns>An IShowingCallback for the view.</returns>
        public IShowingCallback Show<T>(params object[] args) where T : BaseView
        {
            UIData uiData = UIHandler.GetUIData<T>();
            if (uiData == null)
            {
                Debug.LogError("Not found view type := " + typeof(T));
                return null;
            }

            int viewKey = uiData.Key;
            ViewExecution viewExecution = _executionHandler.SetupShowingExecution(viewKey, viewType: typeof(T), args: args);

            return viewExecution?.ShowingHandler.SetupShow(viewKey, isWidget: false, args: args);
        }

        /// <summary>
        /// Shows a widget of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the widget to show.</typeparam>
        /// <param name="args">Optional arguments to pass to the widget.</param>
        /// <returns>An IShowingCallback for the widget.</returns>
        public IShowingCallback ShowWidget<T>(params object[] args) where T : WidgetView
        {
            UIData uiData = UIHandler.GetUIData<T>();
            if (uiData == null)
            {
                Debug.LogError("Not found view type := " + typeof(T));
                return new ShowingUIHandler();
            }

            int viewKey = uiData.Key;
            ViewExecution viewExecution = _executionHandler.SetupWidgetShowingExecution(viewKey, viewType: typeof(T), args: args);

            return viewExecution?.ShowingHandler.SetupShow(viewKey, isWidget: true, args: args);
        }

        /// <summary>
        /// Shows a UI view by its key.
        /// </summary>
        /// <param name="viewKey">The key of the view to show.</param>
        /// <param name="args">Optional arguments to pass to the view.</param>
        /// <returns>An IShowingCallback for the view.</returns>
        public IShowingCallback Show(int viewKey, params object[] args)
        {
            UIData uiData = UIHandler.GetUIData(viewKey);
            if (uiData == null)
            {
                Debug.LogError("Cannot find view with key:= " + viewKey);
                return null;
            }

            ViewExecution viewExecution = _executionHandler.SetupWidgetShowingExecution(viewKey, uiData.ViewType, args);

            return viewExecution?.ShowingHandler.SetupShow(viewKey, isWidget: false, args: args);
        }

        /// <summary>
        /// Hides a UI view of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the view to hide.</typeparam>
        /// <param name="isDisable">Whether to disable the view.</param>
        /// <param name="isDestroy">Whether to destroy the view.</param>
        /// <param name="args">Optional arguments to pass to the view.</param>
        /// <returns>An IHidingCallback for the view.</returns>
        public IHidingCallback Hide<T>(bool isDisable = false, bool isDestroy = false, params object[] args) where T : BaseView
        {
            ViewExecution viewExecution = _executionHandler.SetupHidingExecution(typeof(T), isDestroy, args);

            if (ReferenceEquals(viewExecution, null))
            {
                Debug.LogWarning("Unable to hide:= " + typeof(T) + " cause the ViewExecution instance is null.");
                return new HidingUIHandler();
            }

            return viewExecution.HidingHandler.SetupHide(viewExecution.ViewKey, isDisable: isDisable, isDestroy: isDestroy, isWidget: false, args: args);
        }

        /// <summary>
        /// Hides all views of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the views to hide.</typeparam>
        /// <param name="isDisable">Whether to disable the views.</param>
        /// <param name="isDestroy">Whether to destroy the views.</param>
        /// <param name="args">Optional arguments to pass to the views.</param>
        /// <returns>An IHidingCallback for the views.</returns>
        public IHidingCallback HideAll<T>(bool isDisable = false, bool isDestroy = false, params object[] args) where T : BaseView
        {
            ViewExecution viewExecution = _executionHandler.SetupHidingExecution(typeof(T), isDestroy, args);

            List<BaseView> baseViews = new List<BaseView>();
            for (int i = 0; i < Views.Count; i++)
            {
                if (Views[i].GetType() == typeof(T))
                {
                    baseViews.Add(Views[i]);
                }
            }

            return viewExecution.HidingHandler.SetupHide(baseViews, isDisable: isDisable, isDestroy: isDestroy, isWidget: false, args: args);
        }

        /// <summary>
        /// Hides a specific widget.
        /// </summary>
        /// <param name="widgetView">The widget to hide.</param>
        /// <param name="isDisable">Whether to disable the widget.</param>
        /// <param name="isDestroy">Whether to destroy the widget.</param>
        /// <param name="args">Optional arguments to pass to the widget.</param>
        /// <returns>An IHidingCallback for the widget.</returns>
        public IHidingCallback HideWidget(WidgetView widgetView, bool isDisable = false, bool isDestroy = false, params object[] args)
        {
            if (widgetView == null || !widgetView.gameObject.activeSelf)
            {
                return new HidingUIHandler();
            }

            ViewExecution viewExecution = _executionHandler.SetupWidgetHidingExecution(widgetView, isDestroy, args);

            if (viewExecution.HidingHandler != null)
            {
                return viewExecution.HidingHandler.SetupHide(viewExecution.ViewKey, isDisable: isDisable, isDestroy: isDestroy, isWidget: true, args: args);
            }

            Debug.LogError("UIManager HideWidget:= viewExecution.HidingHandler is null");
            return new HidingUIHandler();
        }

        /// <summary>
        /// Hides a UI view by its key.
        /// </summary>
        /// <param name="viewKey">The key of the view to hide.</param>
        /// <param name="isDisable">Whether to disable the view.</param>
        /// <param name="isDestroy">Whether to destroy the view.</param>
        /// <param name="args">Optional arguments to pass to the view.</param>
        /// <returns>An IHidingCallback for the view.</returns>
        public IHidingCallback Hide(int viewKey, bool isDisable = false, bool isDestroy = false, params object[] args)
        {
            ViewExecution viewExecution = _executionHandler.SetupHidingExecution(viewKey, isDestroy, args);

            if (ReferenceEquals(viewExecution, null))
            {
                Debug.LogWarning("Unable to hide:= " + viewKey + " cause the ViewExecution instance is null.");
                return new HidingUIHandler();
            }

            return viewExecution.HidingHandler.SetupHide(viewKey, isDisable: isDisable, isDestroy: isDestroy, isWidget: true, args: args);
        }

        /// <summary>
        /// Checks if a view with the specified key is currently presenting.
        /// </summary>
        /// <param name="viewKey">The key of the view to check.</param>
        /// <returns>True if the view is presenting, otherwise false.</returns>
        public bool IsPresenting(int viewKey)
        {
            BaseView view = GetCache(viewKey);
            if (view == null)
            {
                return false;
            }

            return view.IsPresenting;
        }

        /// <summary>
        /// Checks if a view of the specified type is currently presenting.
        /// </summary>
        /// <typeparam name="T">The type of the view to check.</typeparam>
        /// <returns>True if the view is presenting, otherwise false.</returns>
        public bool IsPresenting<T>() where T : BaseView
        {
            BaseView view = GetCache<T>();
            if (view == null)
            {
                return false;
            }

            return view.IsPresenting;
        }

        /// <summary>
        /// Checks if any dialog is currently presenting.
        /// </summary>
        /// <returns>True if a dialog is presenting, otherwise false.</returns>
        public bool IsDialogPresenting()
        {
            for (int i = 0; i < Views.Count; i++)
            {
                if (Views[i].IsPresenting && Views[i].GetType().IsSubclassOf(typeof(BaseDialog)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets all views in the specified context.
        /// </summary>
        /// <param name="contextName">The name of the context.</param>
        /// <returns>A list of views in the context.</returns>
        private List<BaseView> GetAllViewsInContext(string contextName)
        {
            List<BaseView> views = new List<BaseView>();

            for (int i = 0; i < Views.Count; i++)
            {
                BaseView view = Views[i];

                if (view != null
                    && !string.IsNullOrEmpty(view.ContextName)
                    && string.Equals(view.ContextName, contextName))
                {
                    views.Add(view);
                }
            }

            return views;
        }

        /// <summary>
        /// Hides all widgets currently being presented.
        /// </summary>
        public void HideAllWidgets()
        {
            for (int i = 0; i < Views.Count; i++)
            {
                BaseView view = Views[i];

                if (view != null
                    && view.IsPresenting
                    && view.Layer == UILayer.Widget)
                {
                    HideWidget((WidgetView)view);
                }
            }
        }

        /// <summary>
        /// Gets the UIViewStack for the specified UILayer.
        /// </summary>
        /// <param name="layer">The UILayer to get the stack for.</param>
        /// <returns>The UIViewStack for the specified layer.</returns>
        public UIViewStack GetLayer(UILayer layer)
        {
            for (int i = 0; i < _uiViewStacks.Length; i++)
            {
                if (_uiViewStacks[i].Layer == layer)
                {
                    return _uiViewStacks[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a cached view by its key.
        /// </summary>
        /// <param name="uiKey">The key of the view to retrieve.</param>
        /// <returns>The cached view, or null if not found.</returns>
        public BaseView GetCache(int uiKey)
        {
            for (int i = 0; i < Views.Count; i++)
            {
                if (Views[i].Key == uiKey)
                {
                    return Views[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a cached view of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the view to retrieve.</typeparam>
        /// <returns>The cached view, or null if not found.</returns>
        public T GetCache<T>() where T : BaseView
        {
            for (int i = 0; i < Views.Count; i++)
            {
                if (Views[i].GetType() == typeof(T))
                {
                    return Views[i] as T;
                }
            }

            return null;
        }

        /// <summary>
        /// Handles unloading of the UIManager, hiding or destroying views as necessary.
        /// </summary>
        public override void OnUnloaded()
        {
            for (int i = 0; i < Views.Count; i++)
            {
                BaseView view = Views[i];

                if (!string.Equals(view.ContextName, _sceneManager.CurrentScene.SceneName)
                    || view.Layer == UILayer.ScreenTransition)
                {
                    continue;
                }

                if (view.Layer == UILayer.Panel)
                {
                    Hide(view.Key, isDisable: true, isDestroy: true);
                }
                else if (view.Layer == UILayer.Widget)
                {
                    HideWidget((WidgetView)view, isDisable: false);
                }
                else
                {
#if UNITY_EDITOR
                    view.name = view.name.Replace(" [ACTIVE]", "");
#endif

                    view.ForceHide();
                    view.gameObject.SetActive(false);
                }
            }
        }
    }
}
