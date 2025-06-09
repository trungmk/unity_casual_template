using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public interface IShowingView
    {
        IShowingCallback SetupShow(int viewKey, bool isWidget = false, params object[] args);

        void Clear();

        void SetHandler(IHandlingExecution handler);
    }

    public interface IShowingCallback : IShowingCondition
    {
        void OnShowCompleted(Action<BaseView> callback);
    }

    public interface IShowingCondition
    {
        IShowingCallback WaitUntil(int uiID);
    }
}

