using UnityEngine;

namespace Core
{
    [CreateAssetMenu(menuName = "StartupTasks/RemoteConfigTask")]
    public class _RemoteConfigTask : StartupTaskBase
    {
        public override void Execute()
        {
            Debug.Log("Fetching remote config...");
            // Async/Coroutine/fake async then mark done  
            HasCompleted = true; // Replace with callback when actually finished  
        }
    }
}
