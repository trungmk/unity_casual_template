using System;
using System.Collections.Generic;

namespace Core
{
    /// <summary>
    /// Interface for managing UI views, widgets, and their lifecycle.
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// List of all active views managed by the UIManager.
        /// </summary>
        List<BaseView> Views { get; }

        /// <summary>
        /// Shows a UI view of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the view to show.</typeparam>
        /// <param name="args">Optional arguments to pass to the view.</param>
        /// <returns>An IShowingCallback for the view.</returns>
        IShowingCallback Show<T>(params object[] args) where T : BaseView;

        /// <summary>
        /// Shows a widget of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the widget to show.</typeparam>
        /// <param name="args">Optional arguments to pass to the widget.</param>
        /// <returns>An IShowingCallback for the widget.</returns>
        IShowingCallback ShowWidget<T>(params object[] args) where T : WidgetView;

        /// <summary>
        /// Shows a UI view by its key.
        /// </summary>
        /// <param name="viewKey">The key of the view to show.</param>
        /// <param name="args">Optional arguments to pass to the view.</param>
        /// <returns>An IShowingCallback for the view.</returns>
        IShowingCallback Show(int viewKey, params object[] args);

        /// <summary>
        /// Hides a UI view of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the view to hide.</typeparam>
        /// <param name="isDisable">Whether to disable the view.</param>
        /// <param name="isDestroy">Whether to destroy the view.</param>
        /// <param name="args">Optional arguments to pass to the view.</param>
        /// <returns>An IHidingCallback for the view.</returns>
        IHidingCallback Hide<T>(bool isDisable = false, bool isDestroy = false, params object[] args) where T : BaseView;

        /// <summary>
        /// Hides all views of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the views to hide.</typeparam>
        /// <param name="isDisable">Whether to disable the views.</param>
        /// <param name="isDestroy">Whether to destroy the views.</param>
        /// <param name="args">Optional arguments to pass to the views.</param>
        /// <returns>An IHidingCallback for the views.</returns>
        IHidingCallback HideAll<T>(bool isDisable = false, bool isDestroy = false, params object[] args) where T : BaseView;

        /// <summary>
        /// Hides a specific widget.
        /// </summary>
        /// <param name="widgetView">The widget to hide.</param>
        /// <param name="isDisable">Whether to disable the widget.</param>
        /// <param name="isDestroy">Whether to destroy the widget.</param>
        /// <param name="args">Optional arguments to pass to the widget.</param>
        /// <returns>An IHidingCallback for the widget.</returns>
        IHidingCallback HideWidget(WidgetView widgetView, bool isDisable = false, bool isDestroy = false, params object[] args);

        /// <summary>
        /// Hides a UI view by its key.
        /// </summary>
        /// <param name="viewKey">The key of the view to hide.</param>
        /// <param name="isDisable">Whether to disable the view.</param>
        /// <param name="isDestroy">Whether to destroy the view.</param>
        /// <param name="args">Optional arguments to pass to the view.</param>
        /// <returns>An IHidingCallback for the view.</returns>
        IHidingCallback Hide(int viewKey, bool isDisable = false, bool isDestroy = false, params object[] args);

        /// <summary>
        /// Checks if a view with the specified key is currently presenting.
        /// </summary>
        /// <param name="viewKey">The key of the view to check.</param>
        /// <returns>True if the view is presenting, otherwise false.</returns>
        bool IsPresenting(int viewKey);

        /// <summary>
        /// Checks if a view of the specified type is currently presenting.
        /// </summary>
        /// <typeparam name="T">The type of the view to check.</typeparam>
        /// <returns>True if the view is presenting, otherwise false.</returns>
        bool IsPresenting<T>() where T : BaseView;

        /// <summary>
        /// Checks if any dialog is currently presenting.
        /// </summary>
        /// <returns>True if a dialog is presenting, otherwise false.</returns>
        bool IsDialogPresenting();

        /// <summary>
        /// Hides all widgets currently being presented.
        /// </summary>
        void HideAllWidgets();

        /// <summary>
        /// Gets the UIViewStack for the specified UILayer.
        /// </summary>
        /// <param name="layer">The UILayer to get the stack for.</param>
        /// <returns>The UIViewStack for the specified layer.</returns>
        UIViewStack GetLayer(UILayer layer);

        /// <summary>
        /// Gets a cached view by its key.
        /// </summary>
        /// <param name="uiKey">The key of the view to retrieve.</param>
        /// <returns>The cached view, or null if not found.</returns>
        BaseView GetCache(int uiKey);

        /// <summary>
        /// Gets a cached view of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the view to retrieve.</typeparam>
        /// <returns>The cached view, or null if not found.</returns>
        T GetCache<T>() where T : BaseView;
    }
}
