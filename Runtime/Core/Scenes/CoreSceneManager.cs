using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>  
    /// Core scene manager: handles loading, unloading, and switching game scenes using additive mode and custom controllers.  
    /// </summary>  
    public class CoreSceneManager : BaseSystem, ISceneManager
    {
        [Header("Scene configuration")]
        public List<SceneSO> Scenes;   // List of contexts (scene configurations)  

        public SceneSO InitializeScene; // The initial scene to load  

#if UNITY_EDITOR  
        [Header("Editor only")]
        public string GeneratePath;    // Used for editor tool if needed  
#endif  

        // Public events for scene state changes  
        public Action OnSceneUnloaded;
        public Action OnScenePreUnloaded;
        public Action OnSceneLoaded;

        // Current context information  
        public SceneSO CurrentScene { get; private set; }
        public string NextSceneName { get; private set; }
        public string CurrentSceneName { get; private set; }

        // Unused interface events (should be removed or implemented if needed)  
        Action ISceneManager.OnSceneUnloaded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Action ISceneManager.OnScenePreUnloaded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Action ISceneManager.OnSceneLoaded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private SceneController _currentController;

        /// <summary>  
        /// Loads the initial scene configured in InitializeScene.  
        /// </summary>  
        public void LoadInitScene()
        {
            Timing.RunCoroutine(ProcessLoadScene(InitializeScene.SceneName));
        }

        /// <summary>  
        /// Unloads the current scene and triggers relevant events/cleanup.  
        /// </summary>  
        private IEnumerator<float> UnloadScene()
        {
            if (_currentController != null)
            {
                _currentController.OnPreUnloaded();
                OnScenePreUnloaded?.Invoke();
                yield return Timing.WaitForOneFrame;
            }

            Scene currentScene = SceneManager.GetSceneByName(CurrentSceneName);
            if (currentScene.isLoaded)
            {
                if (_currentController != null)
                {
                    _currentController.OnUnloaded();
                    yield return Timing.WaitForOneFrame;
                }

                yield return Timing.WaitUntilDone(SceneManager.UnloadSceneAsync(CurrentSceneName));
                yield return Timing.WaitForOneFrame;
                OnSceneUnloaded?.Invoke();
            }

            GC.Collect();
            yield return Timing.WaitForOneFrame;
        }

        /// <summary>  
        /// Loads a scene by name, sets up its SceneController, and triggers events.  
        /// </summary>  
        private IEnumerator<float> LoadContext(string sceneName)
        {
            NextSceneName = sceneName;
            yield return Timing.WaitUntilDone(SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive));

            CurrentSceneName = sceneName;
            yield return Timing.WaitForOneFrame;

            CurrentScene = GetSceneSOByName(sceneName);

            Scene scene = SceneManager.GetSceneByName(sceneName);
            GameObject[] rootGameObjects = scene.GetRootGameObjects();

            bool foundSceneController = false;
            // Look for the SceneController on the root objects of the scene  
            for (int i = 0; i < rootGameObjects.Length; i++)
            {
                SceneController sceneController = rootGameObjects[i].GetComponent<SceneController>();
                if (sceneController != null)
                {
                    SetSceneController(sceneController);
                    _currentController.OnLoaded();
                    OnSceneLoaded?.Invoke();
                    foundSceneController = true;
                    break;
                }
            }

            if (!foundSceneController)
            {
                Debug.LogError("No SceneController found in the loaded scene! Please add one to the scene root.");
            }
        }

        /// <summary>  
        /// Orchestrates a full scene switch: unload+load with additively.  
        /// </summary>  
        private IEnumerator<float> ProcessLoadScene(string sceneName, Action loadContextCompleted = null)
        {
            // Unload current scene first  
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(UnloadScene()));
            yield return Timing.WaitForOneFrame;

            // Additively load context and set controller  
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(LoadContext(sceneName)));

            loadContextCompleted?.Invoke();
        }

        /// <summary>  
        /// Helper: Find the SceneSO by scene name.  
        /// </summary>  
        private SceneSO GetSceneSOByName(string sceneName)
        {
            for (int i = 0; i < Scenes.Count; i++)
            {
                SceneSO context = Scenes[i];
                if (string.Equals(context.SceneName, sceneName))
                    return context;
            }
            return null;
        }

        /// <summary>  
        /// Sets the current SceneController instance.  
        /// </summary>  
        public void SetSceneController(SceneController controller)
        {
            _currentController = controller;
        }

        /// <summary>  
        /// Changes scene by context key (calls handler to get scene name).  
        /// </summary>  
        public void ChangeScene(int contextKey)
        {
            string contextName = SceneHandler.GetSceneName(contextKey);
            if (!string.IsNullOrEmpty(contextName))
                Timing.RunCoroutine(ProcessLoadScene(contextName));
        }

        /// <summary>  
        /// Pauses or resumes the game context at scene-level.  
        /// </summary>  
        public void ChangeGameToPause(bool isPause)
        {
            if (_currentController == null) return;
            if (isPause) _currentController.OnPause();
            else _currentController.OnResume();
        }

        /// <summary>  
        /// Delegates update to the scene context controller.  
        /// </summary>  
        public void UpdateContext(float deltaTime)
        {
            if (_currentController == null) return;
            _currentController.OnUpdate(deltaTime);
        }

        public void LateUpdateContext(float fixedDeltaTime)
        {
            if (_currentController == null) return;
            _currentController.OnLateUpdate(fixedDeltaTime);
        }

        public void FixedUpdateContext(float deltaTime)
        {
            if (_currentController == null) return;
            _currentController.OnFixedUpdate(deltaTime);
        }

        /// <summary>  
        /// Ensures garbage collection when manager is destroyed.  
        /// </summary>  
        protected virtual void OnDestroy()
        {
            GC.Collect();
        }
    }
}

