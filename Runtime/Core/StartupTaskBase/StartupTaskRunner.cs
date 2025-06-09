using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Core
{
    /// <summary>  
    /// Executes all startup tasks needed for initialization.  
    /// Supports cancellation, progress, and resetting.  
    /// This runner should be placed in the Boot scene.  
    /// </summary>  
    public class StartupTaskRunner : BaseSystem, IStartupTaskRunner
    {
        [Tooltip("List of startup tasks to execute during initialization.")]
        public StartupTaskBase[] StartupTasks = default;

        private event Action _onInitializeCompleted;
        /// <summary>  
        /// Invoked when all startup tasks are completed.  
        /// </summary>  
        public event Action OnInitializeCompleted
        {
            add { _onInitializeCompleted += value; }
            remove { _onInitializeCompleted -= value; }
        }

        /// <summary>  
        /// Invoked for each progress change (value from 0 to 1).  
        /// </summary>  
        public event Action<float> OnProgressChanged;

        private readonly List<StartupTaskBase> _nodesDefault = new List<StartupTaskBase>();
        private readonly List<StartupTaskBase> _nodes = new List<StartupTaskBase>();

        private bool _isCanceled = false;
        private CoroutineHandle _currentCoroutine;

        /// <summary>  
        /// Initiate all startup tasks.  
        /// </summary>  
        public void Init()
        {
            ResetTasks();
            SetupNodes();
            _isCanceled = false;
            _currentCoroutine = Timing.RunCoroutine(ProcessNodes());
        }

        /// <summary>  
        /// Call this to cancel the initialization process.  
        /// </summary>  
        public void CancelInitialization()
        {
            _isCanceled = true;
            if (_currentCoroutine.IsValid)
                Timing.KillCoroutines(_currentCoroutine);

            Debug.LogWarning("StartupTaskRunner: Initialization Canceled.");
        }

        /// <summary>  
        /// Reset the task states and clear events/progress.  
        /// </summary>  
        public void ResetTasks()
        {
            if (StartupTasks != null)
                foreach (var node in StartupTasks)
                    node?.Init();
            _nodesDefault.Clear();
            _nodes.Clear();
        }

        /// <summary>  
        /// Get a task by name (case-sensitive).  
        /// </summary>  
        public StartupTaskBase GetTask(string taskName)
        {
            if (StartupTasks == null) return null;
            foreach (var t in StartupTasks)
                if (t != null && t.name == taskName)
                    return t;
            return null;
        }

        /// <summary>  
        /// Get a task by type.  
        /// </summary>  
        public T GetTask<T>() where T : StartupTaskBase
        {
            if (StartupTasks == null) return null;
            foreach (var t in StartupTasks)
                if (t is T task)
                    return task;
            return null;
        }

        /// <summary>  
        /// Prepare the startup nodes and categorize them.  
        /// </summary>  
        private void SetupNodes()
        {
            if (StartupTasks == null) return;
            for (int i = 0; i < StartupTasks.Length; i++)
            {
                StartupTaskBase node = StartupTasks[i];
                if (node == null)
                {
                    Debug.LogError($"StartupTaskRunner: StartupTasks[{i}] is null.");
                    continue;
                }
                node.Init();
                if (node.IsDefault)
                    _nodesDefault.Add(node);
                else
                    _nodes.Add(node);
            }
        }

        /// <summary>  
        /// Run default tasks (if any) then other startup tasks.  
        /// Progress and cancellation are handled.  
        /// </summary>  
        private IEnumerator<float> ProcessNodes()
        {
            if (_isCanceled) 
                yield break;

            yield return Timing.WaitUntilDone(Timing.RunCoroutine(Process(_nodesDefault, 0f, 0.5f)));

            if (_isCanceled) 
                yield break;

            yield return Timing.WaitUntilDone(Timing.RunCoroutine(Process(_nodes, 0.5f, 1f)));

            if (_isCanceled) 
                yield break;

            OnProgressChanged?.Invoke(1f);
            _onInitializeCompleted?.Invoke();
        }

        /// <summary>  
        /// Executes a list of nodes with progress update.  
        /// </summary>  
        private IEnumerator<float> Process(List<StartupTaskBase> nodes, float start, float end)
        {
            int count = 0;
            int n = nodes.Count;
            while (count < n)
            {
                if (_isCanceled) yield break;

                StartupTaskBase nodeToRun = null;
                for (int i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    if (node == null)
                    {
                        Debug.LogError($"StartupTaskRunner: Task at index {i} is null.");
                        continue;
                    }
                    if (node.CanExecution())
                    {
                        nodeToRun = node;
                        nodeToRun.HasStarted = true;
                        nodeToRun.Execute();
                        break;
                    }
                }

                if (nodeToRun != null)
                {
                    count++;
                    while (!nodeToRun.HasCompleted)
                    {
                        if (_isCanceled) yield break;
                        yield return Timing.WaitForOneFrame;
                    }
                }

                float progress = Mathf.Lerp(start, end, n > 0 ? (float) count / n : 1f);
                OnProgressChanged?.Invoke(progress);

                yield return Timing.WaitForOneFrame;
            }
        }
    }
}