using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using VContainer.Unity;

namespace Core
{
    /// <summary>
    /// Serve a simple entry point for the game.
    /// Manage lifecycle of the game. (include pause, resume, reload game).
    /// </summary>
    public class GameHub : MonoSingleton<GameHub>
    {
        [SerializeField] 
        private CoreSceneManager _sceneManager = default;

        [SerializeField] 
        private Transform _gameSystemGroup = default;

        [SerializeField]
        private int _targetFramerate = 60;

        public static Action<bool> OnGamePause { get; set; }

        private bool _pauseStatus;

        private BaseSystem[] _CoreBaseSystems;

        public BaseSystem[] CoreBaseSystems => _CoreBaseSystems;

        private int _baseSystemCount = 0;

        private void Start()
        {
            Application.targetFrameRate = _targetFramerate;
            _CoreBaseSystems = _gameSystemGroup.GetComponentsInChildren<BaseSystem>();

#if UNITY_ANDROID
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
            
            EnterPoint();
        }

        /// <summary>
        /// The simple entry point that init the game.
        /// </summary>
        public void EnterPoint()
        {
            if (_sceneManager == null)
            {
                Debug.LogError("ContextManager is NULL!");
            }

            _baseSystemCount = _CoreBaseSystems.Length;
            for (int i = 0; i < _baseSystemCount; i++)
            {
                _CoreBaseSystems[i].Initialize();
            }

            _sceneManager.OnSceneLoaded = Handle_OnSceneLoaded;
            _sceneManager.OnSceneUnloaded = Handle_OnSceneUnloaded;
            _sceneManager.OnScenePreUnloaded = Handle_OnScenePreUnloaded;
            _sceneManager.LoadInitScene();
        }

        private void Handle_OnScenePreUnloaded()
        {
            for (int i = 0; i < _baseSystemCount; i++)
            {
                _CoreBaseSystems[i].OnPreUnloaded();
            }
        }

        private void Handle_OnSceneLoaded()
        {
            for (int i = 0; i < _baseSystemCount; i++)
            {
                _CoreBaseSystems[i].OnLoaded();
            }
        }
        
        private void Handle_OnSceneUnloaded()
        {
            for (int i = 0; i < _baseSystemCount; i++)
            {
                _CoreBaseSystems[i].OnUnloaded();
            }
        }

        public void PauseGame()
        {
            if (_CoreBaseSystems == null)
            {
                return;
            }
                
            for (int i = 0; i < _baseSystemCount; i++)
            {
                _CoreBaseSystems[i].OnPause();
            }
            
            _sceneManager.ChangeGameToPause(true);
        }
        
        public void ResumeGame()
        {
            if (_CoreBaseSystems == null)
            {
                return;
            }
                
            for (int i = 0; i < _baseSystemCount; i++)
            {
                _CoreBaseSystems[i].OnResume();
            }
            
            _sceneManager.ChangeGameToPause(false);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            _pauseStatus = pauseStatus;

            if (_pauseStatus)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
            
            if (this != null)
            {
                OnGamePause?.Invoke(pauseStatus);
            }
            else
            {
                Debug.LogError("OnApplicationPause with GameHub instance is null!");
            }
        }

        private void OnApplicationQuit()
        {
            GC.Collect();
        }
    }
}


