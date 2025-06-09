using System;

namespace Core
{
    /// <summary>
    /// Manages scene loading, unloading and transitions.
    /// Handles communication with scene controllers and lifecycle events.
    /// </summary>
    public interface ISceneManager
    {
        SceneSO CurrentScene { get; }
        string CurrentSceneName { get; }
        Action OnSceneUnloaded { get; set; }
        Action OnScenePreUnloaded { get; set; }
        Action OnSceneLoaded { get; set; }

        void LoadInitScene();
        void ChangeScene(int contextKey);
        void ChangeGameToPause(bool isPause);
        void SetSceneController(SceneController controller);
        void UpdateContext(float deltaTime);
        void LateUpdateContext(float fixedDeltaTime);
        void FixedUpdateContext(float deltaTime);
    }
}
