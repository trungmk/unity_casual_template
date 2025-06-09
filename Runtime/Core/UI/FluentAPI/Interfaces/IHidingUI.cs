using System;
using System.Collections.Generic;

namespace Core
{
    public interface IHidingUI
    {
        IHidingCallback SetupHide(int uiId, bool isDisable, bool isDestroy, bool isWidget = false, params object[] args);
        
        IHidingCallback SetupHide(List<BaseView> views, bool isDisable, bool isDestroy, bool isWidget = false, params object[] args);

        void Clear();
    
        void SetHandler(IHandlingExecution handler);
    }

    public interface IHidingCallback
    {
        void OnHideCompleted(Action<BaseView> callback);
    }
    
    public interface IHidingManyViewsCallback
    {
        void OnHideCompleted(Action<List<BaseView>> callback);
    }
}
