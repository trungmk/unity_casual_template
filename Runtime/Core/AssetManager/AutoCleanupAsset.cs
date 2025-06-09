using UnityEngine;
using VContainer;

namespace Core
{
    /// <summary>
    /// Auto clean up asset when this object is unloaded.
    /// </summary>
    public class AutoCleanupAsset : MonoBehaviour
    {
        private AssetManager _assetManager;

        public void Init(AssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        private void OnDestroy()
        {
            if (_assetManager == null)
            {
                return;
            }

            _assetManager.ReleaseInstance(gameObject);
        }
    }
}
