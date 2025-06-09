using UnityEngine;

namespace Core
{
    /// <summary>  
    /// Base class for a single startup/initialization task (can have dependencies on other tasks).  
    /// </summary>  
    public abstract class StartupTaskBase : ScriptableObject
    {
        [Tooltip("Other startup tasks that must complete before this one runs.")]
        public StartupTaskBase[] DependencyTasks;

        [Tooltip("This task will be executed alongside default (early) tasks if true; otherwise, it's a secondary task.")]
        public bool IsDefault;

        public bool HasStarted { get; set; }
        public bool HasCompleted { get; set; }

        public virtual void Init()
        {
            HasStarted = false;
            HasCompleted = false;
        }

        public abstract void Execute();

        /// <summary>  
        /// True if this task is ready to run (not started, not completed, dependencies finished).  
        /// </summary>  
        public bool CanExecution()
        {
            if (HasStarted || HasCompleted)
                return false;

            if ((DependencyTasks == null) || (DependencyTasks.Length == 0))
                return true;

            for (int i = 0; i < DependencyTasks.Length; i++)
            {
                var dep = DependencyTasks[i];
                if (dep == null)
                {
                    Debug.LogError($"{name}: DependencyTasks[{i}] is null.");
                    continue;
                }
                if (!dep.HasCompleted)
                    return false;
            }

            return true;
        }
    }
}