using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Core
{
    public class ShowingUIHandler : IShowingView, IShowingCallback
    {
        public object[] Args => _openingData.Args;        
        
        private readonly UIExecutionData _openingData;

        private readonly ViewExecution _viewExecutionOwner;
        
        private IHandlingExecution _handler;

        public ViewExecution ViewExecution => _viewExecutionOwner;

        public BaseView View
        {
            get { return _viewExecutionOwner.View; }

            set
            {
                if (_viewExecutionOwner.View == value)
                {
                    return;
                }

                _viewExecutionOwner.View = value;
            }
        }

        public int ViewKey => _openingData.ViewKey;
        
        public ShowingUIHandler() { }

        public ShowingUIHandler(IHandlingExecution handler, ViewExecution viewExecution)
        {
            _handler = handler;
            _openingData = new UIExecutionData();
            _viewExecutionOwner = viewExecution;
        }
    
        public void OnShowCompleted(Action<BaseView> callback)
        {
            if (_openingData != null)
            {
                _openingData.Callback = callback;
            } 
        }
    
        public IShowingCallback SetupShow(int viewKey, bool isWidget = false, params object[] args)
        {
            if (viewKey == 0)
            {
                Debug.LogError("ShowingUIHandler: Cannot show UI because view key is invalid! viewKey := " + viewKey);
                return this;
            }

            Clear();
            _openingData.ViewKey = viewKey;
            _openingData.Args = args;
            _openingData.Callback = null;
            _openingData.IsWidget = isWidget;
            
            // a bit hacky here.
            Timing.RunCoroutine(ProcessShow());
    
            return this;
        }
        
        public void Clear()
        {
            if (_openingData == null)
            {
                return;
            }
            
            _openingData.Args = null;
            _openingData.Callback = null;
            _openingData.View = null;
        }
    
        public void SetHandler(IHandlingExecution handler)
        {
            _handler = handler;
        }
    
        public Action<BaseView> GetCallback()
        {
            return _openingData.Callback;
        }
    
        public IShowingCallback WaitUntil(int uiId)
        {
            return this;
        }

        private IEnumerator<float> ProcessShow()
        {
            yield return Timing.WaitForOneFrame;
            Execute();
        }

        private void Execute()
        {
            if (_openingData.ViewKey != 0 && _handler != null)
            {
                _handler.RunShowingExecution(this, _openingData.IsWidget);
            }
            else
            {
                Debug.LogError("Cannot show UI");
            }
        }
    }
}

