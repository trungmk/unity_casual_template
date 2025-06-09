using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Core
{
    public abstract class SceneController : MonoBehaviour
    {
        public virtual void OnLoaded() { }

        public virtual void OnUnloaded() { }

        public virtual void OnPreUnloaded() { }

        public virtual void OnPause() { }

        public virtual void OnResume() { }

        public virtual void OnUpdate(float deltaTime) { }

        public virtual void OnFixedUpdate(float fixedDeltaTime) { }

        public virtual void OnLateUpdate(float deltaTime) { }
    }
}

