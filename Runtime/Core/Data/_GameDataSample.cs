using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using MEC;
using UnityEngine;
using UnityEngine.Networking;
using VContainer;

namespace Core
{
    public class GameDataSample : BaseSystem, IGameDataManager
    {
        private IGameDefinitionManager _gameDefinitionManager = null;
        private ILocalDataManager _localDataManager;
        private IAssetManager _assetManager;

        public bool IsInitialized { get; private set; }
        public IGameDefinitionManager GameDefinitionManager => _gameDefinitionManager;

        private event Action _onLoadDataCompleted;
        private event Action _onLoadLocalDataCompleted;
        
        public event Action OnLoadDataCompleted
        {
            add { _onLoadDataCompleted += value; }
            remove { _onLoadDataCompleted -= value; }
        }
        
        public event Action OnLoadLocalDataCompleted
        {
            add { _onLoadLocalDataCompleted += value; }
            remove { _onLoadLocalDataCompleted -= value; }
        }

        [Inject]
        public void Construct(IAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        private IEnumerator CheckNetwork(Action<bool> callback)
        {
            bool isNetworkAvailable;

            yield return new WaitForSeconds(1.5f);

            // ping to an address to check network
            using UnityWebRequest www = UnityWebRequest.Get("https://www.google.com/");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                isNetworkAvailable = true;
            }
            else
            {
                isNetworkAvailable = false;
            }

            if (callback != null)
            {
                callback(isNetworkAvailable);
            }
        }

        private void Handle_OnFetchDataTimeOut()
        {
            if (_gameDefinitionManager == null)
            {
                _gameDefinitionManager = new GameDefinitionManager(_assetManager);

                _gameDefinitionManager.OnInitCompleted += () =>
                {
                    IsInitialized = true;
                };

                _gameDefinitionManager.Init();
                Debug.Log("DEBUG: Init local data!");
            }
        }

        private void Handle_OnFetchingDataCompleted()
        {
            if (_gameDefinitionManager == null)
            {
                IsInitialized = true;
            }
        }

        public void InitLocalData()
        {
            _localDataManager.OnLoadDataCompleted = () =>
            {
                _onLoadLocalDataCompleted?.Invoke();
            };

            _localDataManager.Init();
        }

        public List<T> LoadGameDefinition<T>() where T : IGameDefinition
        {
            return _gameDefinitionManager.GetDefinitions<T>();
        }

        public T LoadLocalData<T>() where T : ILocalData
        {
            return _localDataManager.GetData<T>();
        }

        public void SaveLocalData<T>() where T : ILocalData
        {
            _localDataManager.SaveData<T>();
        }

        public void SaveLocalData<T>(object obj) where T : ILocalData
        {
            _localDataManager.SaveData<T>(obj);
        }
    }
}
