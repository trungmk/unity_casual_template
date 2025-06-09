using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Core
{
    public class UIHidingExecutionData : UIExecutionData
    {
        public bool IsDestroy;
    }
    
    public class HidingUIHandler : IHidingUI, IHidingCallback, IHidingManyViewsCallback
    {
        private readonly UIHidingExecutionData _hidingData;

        private readonly ViewExecution _viewExecutionOwner;
        
        private List<UIHidingExecutionData> _hidingDataList;
        
        private IHandlingExecution _handler;

        public ViewExecution ViewExecution => _viewExecutionOwner;

        public BaseView View
        {
            get { return ViewExecution.View; }

            set
            {
                if (ViewExecution.View == value)
                {
                    return;
                }

                ViewExecution.View = value;
            }
        }
        
        public HidingUIHandler() {}
    
        public HidingUIHandler(IHandlingExecution handler, ViewExecution viewExecution)
        {
            _handler = handler;
            _hidingData = new UIHidingExecutionData();
            _viewExecutionOwner = viewExecution;
        }
    
        public IHidingCallback SetupHide(int viewKey, bool isDisable, bool isDestroy, bool isWidget = false, params object[] args)
        {
            if (viewKey == 0)
            {
                Debug.LogError("UIHidingExecutionData: Cannot show UI because view key is invalid! viewKey := " + viewKey);
                return this;
            }
            
            _hidingData.ViewKey = viewKey;
            _hidingData.Args = args;
            _hidingData.IsDestroy = isDestroy;
            _hidingData.Callback = null;
            _hidingData.IsWidget = isWidget;
            _hidingData.IsDisable = isDisable;
    
            // a bit hacky here.
            Timing.RunCoroutine(ProcessHide());
            
            return this;
        }

        public IHidingCallback SetupHide(List<BaseView> views, bool isDisable, bool isDestroy, bool isWidget = false, params object[] args)
        {
            _hidingDataList = new List<UIHidingExecutionData>();
            for (int i = 0; i < views.Count; i++)
            {
                UIHidingExecutionData hidingExecutionData = new UIHidingExecutionData
                {
                    ViewKey = views[i].Key,
                    Args = args,
                    IsDestroy = isDestroy,
                    Callback = null,
                    IsWidget = isWidget,
                    IsDisable = isDisable
                };

                _hidingDataList.Add(hidingExecutionData);
            }
            
            Timing.RunCoroutine(ProcessHide());

            return this;
        }

        public void OnHideCompleted(Action<BaseView> callback)
        {
            if (_hidingData != null)
            {
                if (_hidingData.Callback != null)
                {
                    _hidingData.Callback = null;
                }
    
                _hidingData.Callback += callback;
            } 
        }

        public void OnHideCompleted(Action<List<BaseView>> callback)
        {
            if (_hidingDataList.Count > 0)
            {
                if (_hidingData.Callback != null)
                {
                    _hidingData.Callback = null;
                }
    
                _hidingData.Callbacks += callback;
            } 
        }

        public UIHidingExecutionData GetHidingData()
        {
            return _hidingData;
        }
    
        public void Clear()
        {
            if (_hidingData != null)
            {
                _hidingData.Args = null;
                _hidingData.IsDestroy = false;
                _hidingData.Callback = null;
                _hidingData.View = null;
            }
        }
    
        public void SetHandler(IHandlingExecution handler)
        {
            _handler = handler;
        }
        
        private void Execute()
        {
            if(_hidingData.ViewKey != 0 && _handler != null)
            {
                _handler.RunHidingExecution(this, _hidingData.IsWidget);
            }
            else
            {
                Debug.LogError("Cannot hide UI");
            }
        }
        
        private IEnumerator<float> ProcessHide()
        {
            yield return Timing.WaitForOneFrame;
            Execute();
        }
    }
}

