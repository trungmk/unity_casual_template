using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public abstract class LocalDataManagerBase : ILocalDataManager
    {
        public Action OnLoadDataCompleted { get; set; }
        
        private readonly Dictionary<Type, ILocalDataWrapper> _localDataWrapperDict = new Dictionary<Type, ILocalDataWrapper>();

        private bool _isInit;
        
        public virtual void Init()
        {
            if (_isInit)
            {
                return;
            }

            _isInit = true;
            AssignData();
            
            using Dictionary<Type, ILocalDataWrapper>.Enumerator enumerator =  _localDataWrapperDict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ILocalDataWrapper localDataWrapper = enumerator.Current.Value;
                localDataWrapper.Init();   
            }

            if (OnLoadDataCompleted != null)
            {
                OnLoadDataCompleted();
            }
        }

        public void SaveData<T>() where T : ILocalData
        {
            try
            {
                Type key = typeof(T);
                ILocalDataWrapper wrapper = _localDataWrapperDict[key];
                wrapper.SaveData<T>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Save data error: {ex.Message}");
            }
        }

        public void SaveData<T>(object obj, bool suppressErrors = false) where T : ILocalData
        {
            try
            {
                Type key = typeof(T);
                ILocalDataWrapper wrapper = _localDataWrapperDict[key];
                wrapper.SaveData<T>(obj);
            }
            catch (Exception ex)
            {
                if (!suppressErrors)
                {
                    Debug.LogError($"Save data error: {ex.Message}");
                }
            }
        }

        public T GetData<T>() where T : ILocalData
        {
            try
            {
                Type key = typeof(T);
                ILocalDataWrapper wrapper = _localDataWrapperDict[key];
                if (wrapper != null)
                {
                    return wrapper.GetData<T>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Get data error: {ex.Message}");
            }

            return default;
        }

        protected abstract void AssignData();
    }
}


